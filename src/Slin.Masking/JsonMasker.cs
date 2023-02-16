using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Slin.Masking
{
    /// <summary>
    /// JsonMasker
    /// </summary>
    internal class JsonMasker : IJsonMasker
    {
        private readonly IObjectMaskingOptions _options;

        private readonly IMasker _masker;
        private IXmlMasker _xmlMasker;

        private readonly JsonSerializerOptions _jsonOptions;

        public JsonMasker(IMasker masker, IObjectMaskingOptions options)
        {
            _masker = masker;
            _options = options;

            var jsonOptions = new JsonSerializerOptions()
            {
                //read encoder settings from configuration
                Encoder = JavaScriptEncoder.Create(_options.GetTextEncoderSettings()),
                MaxDepth = _options.JsonMaxDepth
            };
            _jsonOptions = jsonOptions;
        }

        public void SetXmlMasker(IXmlMasker xmlMasker) => _xmlMasker = xmlMasker;

        public void MaskObject(object source, StringBuilder builder)
        {
            if (source == null) return;
            try
            {
                var element = JsonSerializer.SerializeToElement(source, _jsonOptions);
                MaskJsonElement(null, element, builder);
            }
            catch (Exception ex)
            {
                //todo internal log
            }
        }

        public bool TryParse(string value, out JsonElement? node, bool basicValidation = true)
        {
            node = null;

            //here I think for JSON, it at least has 15 char?...
            if (value == null) return false;

            if (basicValidation && value.Length < _options.JsonMinLength || value == "null") return false;

            if (basicValidation &&
                !(value.StartsWithExt('{') && value.EndsWithExt('}')
                || (value.StartsWithExt('[') && value.EndsWithExt(']'))))
                return false;

            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(value), new JsonReaderOptions
            {
                //todo JsonReaderOptions?
                MaxDepth = _options.JsonMaxDepth
            });

            try
            {
                if (JsonElement.TryParseValue(ref reader, out var nodex))
                {
                    node = nodex.Value;
                    return true;
                }
            }
            catch
            {
                //ignore JsonReaderException/JsonException
            }
            return false;
        }

        private void MaskJsonElement(string propertyName, JsonElement element, StringBuilder builder)
        {
            //todo depth check?...

            bool previousAppeared = false;
            switch (element.ValueKind)
            {
                case JsonValueKind.Undefined:
                    builder.Append("null");
                    break;
                case JsonValueKind.Object:
                    {
                        builder.Append('{');
                        var isKvp = IsKvpObject(element, out string keyKey, out var key, out string valKey, out var value);
                        if (isKvp)
                        {
                            var keyName = key.GetString();
                            builder.Append('"').Append(keyKey).Append('"').Append(':')
                                .Append('"');
                            AppendStringEscape(builder, keyName, false, false);
                            builder.Append('"').Append(',')
                                .Append('"').Append(valKey).Append('"').Append(':');
                            MaskJsonElement(keyName, value, builder);
                        }
                        foreach (var child in element.EnumerateObject())
                        {
                            //skip properties of key and value
                            if (isKvp && (child.Name == keyKey || child.Name == valKey))
                                continue;

                            if (isKvp)
                                builder.Append(',');

                            MaskProperty(child, builder);
                            builder.Append(',');

                            if (!previousAppeared)
                                previousAppeared = true;
                        }

                        if (previousAppeared)
                        {
                            builder.Remove(builder.Length - 1, 1);
                        }
                        builder.Append('}');
                    }
                    break;
                case JsonValueKind.Array:
                    builder.Append('[');

                    if (!string.IsNullOrEmpty(propertyName)
                        //&& _masker.IsKeyDefined(propertyName, true)
                        && _options.GlobalModeForArray == ModeIfArray.HandleSingle
                        && element.EnumerateArray().Count() == 1
                        || _options.GlobalModeForArray == ModeIfArray.HandleAll
                        && !string.IsNullOrEmpty(propertyName))
                    {
                        foreach (var child in element.EnumerateArray())
                        {
                            if (child.ValueKind == JsonValueKind.String || child.ValueKind == JsonValueKind.Number)
                            {
                                MaskJsonElement(propertyName, child, builder);
                            }
                            else
                            {
                                MaskJsonElement(null, child, builder);
                            }
                            builder.Append(',');
                            if (!previousAppeared)
                                previousAppeared = true;
                        }
                        if (previousAppeared)
                        {
                            builder.Remove(builder.Length - 1, 1);
                        }
                    }
                    else
                    {
                        foreach (var child in element.EnumerateArray())
                        {
                            MaskJsonElement(null, child, builder);

                            builder.Append(',');

                            if (!previousAppeared)
                                previousAppeared = true;
                        }

                        if (previousAppeared)
                        {
                            builder.Remove(builder.Length - 1, 1);
                        }
                    }
                    builder.Append(']');
                    break;
                case JsonValueKind.String:
                    {
                        var value = element.GetString();

                        if (!string.IsNullOrEmpty(propertyName) && _masker.TryMask(propertyName, value, out var masked))
                        {
                            builder.Append(string.Concat('"', masked, '"'));
                        }
                        else if (_options.MaskUrlEnabled && _options.UrlKeys.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
                        {
                            masked = _masker.MaskUrl(value, true);

                            builder.Append(string.Concat('"', masked, '"'));
                        }
                        else if (propertyName != null && value != null && SerializedMaskAttempt(propertyName, value, builder))
                        {
                            //do nothing
                        }
                        else
                        {
                            //builder.Append(string.Concat('"', value, '"')); //todo quote, escape value?
                            builder.Append('"');
                            AppendStringEscape(builder, value, false, false);
                            builder.Append('"');
                        }
                    }
                    break;
                case JsonValueKind.Number:
                    {
                        if (!string.IsNullOrEmpty(propertyName) && _options.MaskJsonNumberEnabled && _masker.TryMask(propertyName, element.GetRawText(), out var masked))
                        {
                            if (masked == null) builder.Append("null");
                            else if (masked != null && double.TryParse(masked, out var _))
                                builder.Append(masked);  //number
                            else
                                builder.Append(string.Concat('"', masked, '"'));
                        }
                        else
                        {
                            builder.Append(element.GetRawText());
                        }
                    }
                    break;
                case JsonValueKind.True:
                    builder.Append(element.GetRawText());
                    break;
                case JsonValueKind.False:
                    builder.Append(element.GetRawText());
                    break;
                case JsonValueKind.Null:
                    builder.Append("null");
                    break;
                default:
                    throw new NotImplementedException("unkonwn valuekind");
            }
        }

        private void MaskProperty(JsonProperty property, StringBuilder builder)
        {
            builder.Append('"');
            AppendStringEscape(builder, property.Name, false, false);
            builder.Append('"').Append(':');
            MaskJsonElement(property.Name, property.Value, builder);
        }

        private bool SerializedMaskAttempt(string key, string value, StringBuilder builder)
        {
            if (!_options.MaskJsonSerializedEnabled || _options.SerializedKeys == null
                || !_options.SerializedKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) return false;

            try
            {
                if (_options.MaskJsonSerializedEnabled && TryParse(value, out var parsedNode))
                {
                    if (_options.MaskJsonSerializedParsedAsNode)
                    {
                        MaskJsonElement(key, parsedNode.Value, builder);
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        MaskJsonElement(key, parsedNode.Value, sb);
                        builder.Append('"');
                        AppendStringEscape(builder, sb.ToString(), false, false);
                        builder.Append('"');
                    }

                    return true;
                }
                else if (_options.MaskXmlSerializedEnabled && _xmlMasker != null && _xmlMasker.TryParse(value, out var element))
                {
                    var masked = _xmlMasker.MaskXmlElementString(element);

                    builder.Append('"');
                    AppendStringEscape(builder, masked, false, false);
                    builder.Append('"');

                    return true;
                }
            }
            catch (Exception)
            {
                //todo Parse Json failed
            }
            return false;
        }


        public bool IsKvpObject(JsonElement ele, out string keyKey, out JsonElement key, out string valKey, out JsonElement value)
        {
            keyKey = valKey = "";
            key = value = default;
            if (ele.ValueKind != JsonValueKind.Object) return false;

            int flag = 0;
            //int count = ele.EnumerateObject().Count();
            foreach (var kv in _options.KeyKeyValueKeys ?? KeyKeyValueKey.DefaultKeyKeyValueKeys)
            {
                keyKey = kv.KeyKeyName;
                valKey = kv.ValueKeyName;
                if (ele.TryGetProperty(keyKey, out key))
                {
                    if (key.ValueKind == JsonValueKind.String)
                    {
                        flag |= 1;
                    }
                }
                if (ele.TryGetProperty(valKey, out value))
                {
                    flag |= 2;
                }
                if (flag == 3) return true;
            }
            return false;
        }


        /// <summary>
        /// Checks input string if it needs JSON escaping, and makes necessary conversion
        /// </summary>
        /// <param name="destination">Destination Builder</param>
        /// <param name="text">Input string</param>
        /// <param name="escapeUnicode">Should non-ASCII characters be encoded</param>
        /// <param name="escapeForwardSlash"></param>
        /// <returns>JSON escaped string</returns>
        internal static void AppendStringEscape(StringBuilder destination, string text, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (string.IsNullOrEmpty(text))
                return;

            StringBuilder sb = null;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];
                if (!RequiresJsonEscape(ch, escapeUnicode, escapeForwardSlash))
                {
                    sb?.Append(ch);
                    continue;
                }
                else if (sb is null)
                {
                    sb = destination;
                    sb.Append(text, 0, i);
                }

                switch (ch)
                {
                    case '"':
                        sb.Append("\\\"");
                        break;

                    case '\\':
                        sb.Append("\\\\");
                        break;

                    case '\b':
                        sb.Append("\\b");
                        break;

                    case '/':
                        if (escapeForwardSlash)
                        {
                            sb.Append("\\/");
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;

                    case '\r':
                        sb.Append("\\r");
                        break;

                    case '\n':
                        sb.Append("\\n");
                        break;

                    case '\f':
                        sb.Append("\\f");
                        break;

                    case '\t':
                        sb.Append("\\t");
                        break;

                    default:
                        if (EscapeChar(ch, escapeUnicode))
                        {
                            sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            if (sb is null)
                destination.Append(text);   // Faster to make single Append
        }

        internal static bool RequiresJsonEscape(char ch, bool escapeUnicode, bool escapeForwardSlash)
        {
            if (!EscapeChar(ch, escapeUnicode))
            {
                switch (ch)
                {
                    case '/': return escapeForwardSlash;
                    case '"':
                    case '\\':
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static bool EscapeChar(char ch, bool escapeUnicode)
        {
            if (ch < 32)
                return true;
            else
                return escapeUnicode && ch > 127;
        }
    }
}
