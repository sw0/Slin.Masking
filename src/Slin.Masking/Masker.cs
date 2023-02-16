namespace Slin.Masking
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    /// <summary>
    /// default Masker
    /// </summary>
    public class Masker : IMasker
    {
        private readonly IMaskingContext _context;

        public Masker(IMaskingOptions options, IMaskFormatter maskFormatter = null)
        {
            _context = new MaskingContext(options, maskFormatter);
        }

        public bool IsKeyDefined(string key, bool addToCache = false)
        {
            return _context.IsKeyDefined(key, addToCache);
        }

        public bool TryMask(string key, string value, out string masked)
        {
            var masker = _context.GetKeyedMasker(key, value);

            if (masker == null)
            {
                masked = value;
                return false;
            }

            masked = masker.Mask(value);
            return true;
        }

        private string MaskQuery(string query)
        {
            var items = HttpUtility.ParseQueryString(query);

            if (items.HasKeys())
            {
                bool changed = false;

                for (var i = 0; i < items.Count; i++)
                {
                    var key = items.Keys[i];
                    var value = items[i];
                    if (value != "")
                    {
                        if (TryMask(key, value, out var masked))
                        {
                            if (masked != value && changed == false)
                                changed = true;

                            if (masked != value) items[key] = masked;
                        }
                    }
                }

                if (changed)
                {
                    List<string> rows = new List<string>();

                    foreach (string name in items)
                        rows.Add(string.Concat(name, "=", items[name]));
                    //No need urlEncode here System.Web.HttpUtility.UrlEncode

                    var result = $"{(query.StartsWith("?") ? "?" : "")}{string.Join("&", rows)}";
                    return result;
                }
            }

            return query;
        }

        public string MaskUrl(string url, bool maskQueries = true, params UrlMaskingPattern[] overwrittenPatterns)
        {
            try
            {
                var result = url;

                var patterns = overwrittenPatterns == null || overwrittenPatterns.Length == 0
                    ? _context.UrlMaskingPatterns : overwrittenPatterns.ToList();

                if (patterns != null && patterns.Any())
                {
                    foreach (var item in patterns.Where(x => x.Enabled && !string.IsNullOrEmpty(x.Pattern)))
                    {
                        var reg = _context.GetRequiredRegex(item.CacheKey);
                        var gns = reg.GetGroupNames().Where(gn => !char.IsNumber(gn[0]));

                        result = reg.Replace(result, (m) =>
                        {
                            var g0Val = m.Groups[0].Value;
                            var g0Idx = m.Groups[0].Index;
                            var g0Len = m.Groups[0].Length;

                            var capturedGroup = GetMatchedGroup(m.Groups, gns, out var groupName);
                            var cIdx = capturedGroup.Index;
                            var cVal = capturedGroup.Value;
                            var cLen = capturedGroup.Length;

                            if (TryMask(groupName, cVal, out var masked) && masked != cVal)
                            {
                                var replaced = g0Val.Insert(cIdx - g0Idx, masked).Remove(cIdx - g0Idx + masked.Length, cLen);
                                return replaced;
                            }
                            else
                            {
                                return g0Val;
                            }
                        });
                    }
                }

                var idx = url.IndexOf('?');
                if (maskQueries
                    && (idx >= 0 || idx == -1 && IsKvpStrings(url)))
                {
                    var query = idx == -1 ? url : result.Substring(idx);
                    var masked = MaskQuery(query);
                    if (query == masked)
                        return result;
                    else if (idx == -1)
                        result = masked;
                    else
                        result = result.Substring(0, idx) + masked;
                }
                return result;
            }
            catch (Exception ex)
            {
                //todo internal log
            }
            return url;
        }

        private bool IsKvpStrings(string url)
        {
            return (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && url.Contains('='));
        }

        Group GetMatchedGroup(GroupCollection gc, IEnumerable<string> groupNames, out string groupName)
        {
            foreach (var gn in groupNames)
            {
                if (char.IsNumber(gn[0])) continue;

                var g = gc[gn];
                if (g.Success)
                {
                    groupName = gn;
                    return g;
                }
            }
            groupName = null;
            throw new Exception("should not happen");
        }
    }

}
