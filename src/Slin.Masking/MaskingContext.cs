using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Slin.Masking
{

    internal class MaskingContext : IMaskingContext
    {
        protected Dictionary<string, ValueFormatterDefinition> NamedFormatters => Options.NamedFormatters;

        protected Dictionary<string, MaskRuleDefinition> Items => Options.Rules;

        //private readonly Dictionary<string, MaskRuleDefinition> _lookup = new Dictionary<string, MaskRuleDefinition>();

        private readonly ConcurrentDictionary<string, KeyedMasker> _pooled;

        private readonly List<KeyedMasker> _pooledPatternedKeyMaskers = new List<KeyedMasker>();

        //todo this might not be shared.
        public static readonly ConcurrentDictionary<string, Regex> PooledRegex = new ConcurrentDictionary<string, Regex>();

        /// <summary>
        /// When <see cref="IMaskingOptions.EnableUnmatchedKeysCache"/> is true. 
        /// Unmatched keys cached to improve performance in case we got a lot regular expression patterns. 
        /// </summary>
        public readonly HashSet<string> _unmatchedKeys = null;

        /// <summary>
        /// maybe it's not good to use public here, but it's intenal use. so just keep it here
        /// </summary>
        public List<UrlMaskingPattern> UrlMaskingPatterns => Options.UrlMaskingPatterns;

        public IMaskingOptions Options { get; }

        public IMaskFormatter MaskFormatter { get; private set; }

        private bool _initalized = false;

        public MaskingContext(IMaskingOptions options, IMaskFormatter maskFormatter = null)
        {
            if (options == null) throw new ArgumentNullException("options");

            options.Normalize();

            Options = options;

            //todo print configurations

            if (Options.KeyedMaskerPoolIgnoreCase)
            {
                _pooled = new ConcurrentDictionary<string, KeyedMasker>(StringComparer.OrdinalIgnoreCase);
                _unmatchedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _pooled = new ConcurrentDictionary<string, KeyedMasker>();
                _unmatchedKeys = new HashSet<string>();
            }

            MaskFormatter = maskFormatter ?? new MaskFormatter();

            Initialize();

            _pooledPatternedKeyMaskers = _pooled.Values.Where(km => km.KeyNamePatterned).ToList();
        }

        private KeyedMasker NormalizeMaskDefinition(KeyedMasker definition)
        {
            //todo 
            var namedFormatters = NamedFormatters;
            var count = definition.Formatters.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var formatter = definition.Formatters[i];
                if (!string.IsNullOrEmpty(formatter.Name))
                {
                    if (namedFormatters.ContainsKey(formatter.Name))
                    {
                        var found = namedFormatters[formatter.Name];
                        definition.Formatters[i] = new ValueFormatter(this, found);
                    }
                    else
                    {
                        definition.Formatters.RemoveAt(i);

                        throw new Exception($"{nameof(Options.NamedFormatters)} does not found: {formatter.Name}");
                    }
                }
            }

            return definition;
        }

        private void Initialize()
        {
            if (_initalized) throw new Exception("already initialized");

            var normalized = new List<KeyedMasker>();

            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.Value.KeyName)) item.Value.KeyName = item.Key;

                normalized.Add(NormalizeMaskDefinition(new KeyedMasker(this, item.Value)));
            }

            #region regex cache
            foreach (var item in normalized)
            {
                if (item.Formatters.Count == 0) continue;
                try
                {
                    //todo make sure not duplicated key
                    _pooled.TryAdd(item.KeyName, item);

                    if (IsLikePattern(item.KeyName))
                    {
                        if (item.KeyName.EndsWith("(?#casesensitive)"))
                            GetOrAddRegex(item.KeyName, RegexOptions.Compiled);
                        else
                            GetOrAddRegex(item.KeyName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    }


                    int idx = 0;
                    foreach (var fmter in item.Formatters)
                    {
                        if (fmter.HasValuePatterned)
                            GetOrAddRegex(fmter.ValuePattern, fmter.IgnoreCase ? RegexOptions.Compiled | RegexOptions.IgnoreCase : RegexOptions.Compiled);

                        if (!string.IsNullOrEmpty(fmter.Format) && MaskFormatter.IsFormatMatched(fmter.Format))
                        {
                            //todo throw 
                            throw new Exception($"Format '{fmter.Format}' defined in for key '{item.KeyName}' on position '{idx + 1}' is not valid format");
                        }
                        idx++;
                    }
                }
                catch (Exception)
                {
                    //todo ...
                    throw;
                }
            }

            if (UrlMaskingPatterns != null)
            {
                UrlMaskingPatterns.ForEach(item =>
                {
                    if (!item.Enabled) return;
                    if (string.IsNullOrWhiteSpace(item.Pattern)) return;

                    try
                    {
                        var reg = new Regex(item.Pattern, RegexOptions.Compiled |
                            (item.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));

                        PooledRegex.TryAdd(item.CacheKey, reg);
                    }
                    catch (Exception)
                    {
                        //todo logging						
                    }
                });
            }
            #endregion

            _initalized = true;
        }

        public bool IsLikePattern(string input)
        {
            return input != null && Options.RegexCheckChars.Any(c => input.Contains(c));
        }

        public Regex GetOrAddRegex(string pattern, RegexOptions options)
        {
            var regex = PooledRegex.GetOrAdd(pattern, new Regex(pattern, options));

            return regex;
        }

        public Regex GetRequiredRegex(string pattern)
        {
            if (PooledRegex.TryGetValue(pattern, out var regex)) return regex;

            throw new Exception($"Regex for pattern '{pattern}' was expected");
        }

        public bool IsKeyDefined(string nameOfSensitiveData, bool add2PoolIfMatched = false)
        {
            Contract.Assert(nameOfSensitiveData != null);
            if (string.IsNullOrEmpty(nameOfSensitiveData))
                throw new ArgumentNullException(nameof(nameOfSensitiveData));

            if (_pooled.TryGetValue(nameOfSensitiveData, out var masker))
            {
                if (masker == null) return false;

                return true;
            }
            else
            {
                foreach (var item in _pooledPatternedKeyMaskers)
                {
                    if (GetRequiredRegex(item.KeyName).IsMatch(nameOfSensitiveData))
                    {
                        if (add2PoolIfMatched && (item.KeyNameLenLimitToCache <= 0
                            || nameOfSensitiveData.Length < item.KeyNameLenLimitToCache))
                            _pooled.TryAdd(nameOfSensitiveData, item);
                        return true;
                    }
                }
            }
            return false;
        }


        public IKeyedMasker GetKeyedMasker(string key, string value)
        {
            if (_pooled.TryGetValue(key, out var masker))
            {
                if (masker == null) return null;

                if (masker.IsEligibleToMask(value)) return masker;

                if (masker.Formatters.Any(f => f.ValueMatchesPattern(value)))
                    return masker;
                return null;
            }
            else if (Options.EnableUnmatchedKeysCache && _unmatchedKeys.Contains(key))
            {
                //regular express would affect some performance. Use hashset to bypass regular expression evaluation
                return null;
            }
            else
            {
                foreach (var item in _pooledPatternedKeyMaskers)
                {
                    if (GetRequiredRegex(item.KeyName).IsMatch(key))
                    {
                        //TODO maybe we'd better to add limits here
                        if (item.KeyNameLenLimitToCache <= 0
                            || key.Length <= item.KeyNameLenLimitToCache)
                            _pooled.TryAdd(key, item);

                        return item;
                    }
                }

                if (Options.EnableUnmatchedKeysCache
                    && (Options.KeyNameLenLimitToCache <= 0 || key.Length <= Options.KeyNameLenLimitToCache))
                {
                    _unmatchedKeys.Add(key);
                }
            }
            return null;
        }
    }
}
