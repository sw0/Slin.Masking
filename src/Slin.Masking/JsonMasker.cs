using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

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
				//todo from configuration
				Encoder = JavaScriptEncoder.Create(options.GetTextEncoderSettings()),
				MaxDepth = _options.JsonMaxDepth
			};
			_jsonOptions = jsonOptions;
		}

		public void SetXmlMasker(IXmlMasker xmlMasker) => _xmlMasker = xmlMasker;

		public string MaskJsonObjectString(JsonNode node)
		{
			//todo ...
			throw new NotImplementedException("should not use, I probably want to remove this method");
		}

		public void MaskObject(object source, StringBuilder builder)
		{
			if (source == null) return;

			var element = JsonSerializer.SerializeToElement(source, _jsonOptions);
			MaskJsonElement(null, element, builder);
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

			if (JsonElement.TryParseValue(ref reader, out var nodex))
			{
				node = nodex.Value;
				return true;
			}
			return false;
		}

		private void MaskJsonElement(string propertyName, JsonElement element, StringBuilder builder)
		{
			//depth check?...

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
								.Append('"').Append(keyName).Append('"').Append(',')
								.Append('"').Append(valKey).Append('"').Append(':');
							MaskJsonElement(keyName, value, builder);
						}
						foreach (var child in element.EnumerateObject())
						{
							//todo skip properties of key and value
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

					if (_options.ArrayItemHandleMode == ArrayItemHandleMode.SingleItemAsValue
						&& !string.IsNullOrEmpty(propertyName) && element.EnumerateArray().Count() == 1
						|| _options.ArrayItemHandleMode == ArrayItemHandleMode.AnyItemsAsValues
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
							builder.Append(string.Concat("\"", masked, "\"")); //todo quote
						}
						else if (_options.MaskUrlEnabled && _options.UrlKeys.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
						{
							masked = _masker.MaskUrl(value, true);

							builder.Append(string.Concat("\"", masked, "\"")); //todo quote
						}
						else if (propertyName != null && value != null && SerializedMaskAttempt(propertyName, value, builder))
						{
							//do nothing
						}
						else
						{
							//todo if Url enabled
							builder.Append(string.Concat("\"", value, "\"")); //todo quote
						}
					}
					break;
				case JsonValueKind.Number:
					{
						if (!string.IsNullOrEmpty(propertyName) && _options.MaskJsonNumberEnabled && _masker.TryMask(propertyName, element.GetRawText(), out var masked))
						{
							if (masked == null) builder.Append("null");
							else if (masked != null && masked.All(char.IsNumber))
								builder.Append(masked);
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
			builder.Append('"').Append(property.Name).Append('"').Append(':');
			MaskJsonElement(property.Name, property.Value, builder);
		}
		private bool SerializedMaskAttempt(string key, string value, StringBuilder builder)
		{
			if (!_options.MaskJsonSerializedEnabled || !_options.SerializedKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) return false;

			try
			{
				if (_options.MaskJsonSerializedEnabled && TryParse(value, out var parsedNode))
				{
					MaskJsonElement(key, parsedNode.Value, builder);

					return true;
				}
				else if (_options.MaskXmlSerializedEnabled && _xmlMasker != null && _xmlMasker.TryParse(value, out var element))
				{
					var masked = _xmlMasker.MaskXmlElementString(element);
					//source[valueKeyName] = masked;
					builder.Append('"').Append(masked).Append('"');
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
			key = value = default(JsonElement);
			if (ele.ValueKind != JsonValueKind.Object) return false;

			int flag = 0;
			int count = ele.EnumerateObject().Count();
			foreach (var kv in _options.KeyKeyValueKeys)
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
	}
}
