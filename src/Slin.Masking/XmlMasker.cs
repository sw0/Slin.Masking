using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.Text;

namespace Slin.Masking
{
	internal class XmlMasker : IXmlMasker
	{
		private readonly IObjectMaskingOptions _options;

		private readonly IMasker _masker;
		private IJsonMasker _jsonMasker;

		protected bool MaskJsonSerializedEnabled => _options.MaskJsonSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();
		protected bool MaskXmlSerializedEnabled => _options.MaskXmlSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();
		protected bool MaskXmlSerializedOnXmlAttributeEnabled => _options.MaskXmlSerializedOnXmlAttributeEnabled;
		protected bool MaskJsonSerializedOnXmlAttributeEnabled => _options.MaskJsonSerializedOnXmlAttributeEnabled;

		protected bool IsMaskUrlEnabled => _options.MaskUrlEnabled && _options.UrlKeys != null
			&& _options.UrlKeys.Any();

		/// <summary>
		/// indicates whether to support nested key-value-pairs. 
		/// Like to support mask {"key":"ssn", "value":"123456789"}
		/// </summary>
		protected bool MaskNestedKvpEnabled => _options.MaskNestedKvpEnabled;

		public XmlMasker(IMasker masker, IObjectMaskingOptions options)
		{
			_masker = masker;
			_options = options;
		}

		public void SetJsonMasker(IJsonMasker jsonMasker) => _jsonMasker = jsonMasker;

		#region -- XML -- 

		public string MaskXmlElementString(XElement element)
		{
			MaskXElementInternal(element);

			var result = element.ToString(SaveOptions.DisableFormatting);

			return result;
		}

		private void MaskXElementInternal(XElement element)
		{
			if (element.HasAttributes)
			{
				//todo mask attributes
				foreach (var attr in element.Attributes())
				{
					if (_masker.TryMask(attr.Name.LocalName, attr.Value, out string masked))
					{
						attr.Value = masked;
					}
					else if (!UrlJsonMaskAttempt(attr))
					{
						//other masking attempts
						SerializedMaskAttempt(attr);
					}
				}
			}

			// read the element and do with your node 
			if (element.HasElements)
			{
				if (_options.MaskNestedKvpEnabled && ContainsKeyValuePair(element.Elements(), out var keyKeyName, out var valKeyName))
				{
					var key = default(string);
					var value = default(string);
					XElement valueElement = null;

					//get the key key and value and value element
					foreach (var item in element.Elements())
					{
						if (item.Name.LocalName == keyKeyName && !item.HasElements)
						{
							key = item.Value;
						}
						else if (item.Name.LocalName == valKeyName && !item.HasElements)
						{
							value = item.Value;
							valueElement = item;
						}
					}

					Contract.Assert(!string.IsNullOrEmpty(key));

					//hit here, key 
					if (valueElement != null
						&& _masker.TryMask(key, value, out var masked))
					{
						valueElement.Value = masked;
					}
					else if (valueElement != null)
					{
						SerializedMaskAttempt(key, valueElement);

						UrlJsonMaskAttempt(key, valueElement);
					}

					foreach (var item in element.Elements())
					{
						if (item.Name.LocalName == keyKeyName && !!item.HasElements) continue;
						if (item.Name.LocalName == valKeyName && !item.HasElements) continue;

						MaskXElementInternal(item);
					}
				}
				else
				{
					// here you can reach nested node 
					foreach (var item in element.Elements())
					{
						MaskXElementInternal(item);
					}
				}
			}
			else
			{
				var name = element.Name.LocalName;
				var value = element.Value;
				if (_masker.TryMask(name, value, out string masked))
				{
					element.Value = masked;
				}
				else
				{
					SerializedMaskAttempt(name, element);

					UrlJsonMaskAttempt(name, element);
				}
			}
		}

		/// <summary>
		/// source will be like: [{"Key":"DOB","Value":"2022-01-01"},{"Key":"SSN","Value":"123456789"}]
		/// </summary>
		/// <param name="source"></param>
		/// <param name="eligibleKeyValueNames"></param>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <returns></returns>
		private bool ContainsKeyValuePair(IEnumerable<XElement> source, out string keyName, out string valueName, List<KeyKeyValueKey> eligibleKeyValueNames = null)
		{
			keyName = valueName = null;

			foreach (var item in eligibleKeyValueNames ?? KeyKeyValueKey.DefaultKeyKeyValueKeys)
			{
				//here: item.Key is keyName, item.Value is value keyName. By default: it's "Key","Value".
				//Let's make it simple here, that assuming the value type are strings for matched item for key and value.

				var k = source.FirstOrDefault(x => x.HasElements == false && x.Name.LocalName == item.KeyKeyName);
				if (k != null && !string.IsNullOrEmpty(k.Value))
				{
					var v = source.FirstOrDefault(x => x.HasElements == false && x.Name.LocalName == item.ValueKeyName);

					if (v != null && v.Value != null && v.Value.Length > _options.ValueMinLength)
					{
						keyName = k.Name.LocalName;
						valueName = v.Name.LocalName;
						return true;
					}
				}
			}
			//}

			return false;
		}
		#endregion


		#region -- json mask attempts --

		private bool FirstJsonMaskAttempt(string key, string valueKey, JsonValue jval, JsonObject source, out string value, out bool valueIsString)
		{
			valueIsString = true;
			if (!jval.TryGetValue<string>(out value))
			{
				valueIsString = false;
				//if not string, it would be ValueKind.Number 
				value = jval.GetValue<double>().ToString();
			}
			if (_masker.TryMask(key, value, out string masked))
			{
				source[valueKey] = masked;
				return true;
			}
			return false;
		}

		private bool SerializedMaskAttempt(string key, string value, string valueKeyName, JsonObject source)
		{
			if (!IsSerializedKey(key)) return false;

			try
			{
				if (MaskJsonSerializedEnabled && _jsonMasker != null && _jsonMasker.TryParse(value, out var parsedNode))
				{
					//source[valueKeyName] = parsedNode.Value;

					//_jsonMasker.MaskObject(parsedNode);
					//todo ...important

					return true;
				}
				else if (MaskXmlSerializedEnabled && TryParse(value, out var element))
				{
					var masked = MaskXmlElementString(element);
					source[valueKeyName] = masked;
					return true;
				}
			}
			catch (Exception)
			{
				//todo Parse Json failed
			}
			return false;
		}

		private bool SerializedMaskAttempt(string key, XElement source)
		{
			if (!IsSerializedKey(key))
				return false;
			try
			{
				if (MaskJsonSerializedEnabled && _jsonMasker != null && _jsonMasker.TryParse(source.Value, out var parsedNode))
				{
					//var masked = MaskObjectInternal(parsedNode);
					var sb = new StringBuilder(source.Value.Length);
					_jsonMasker.MaskObject(parsedNode, sb);
					source.SetValue(sb.ToString());
					return true;
				}
				else if (MaskXmlSerializedEnabled && TryParse(source.Value, out var element))
				{
					var masked = MaskXmlElementString(element);
					source.Value = masked;
					return true;
				}
			}
			catch (Exception)
			{
				//todo Parse Json failed
			}
			return false;
		}
		private bool SerializedMaskAttempt(XAttribute source)
		{
			if (!IsSerializedKey(source.Name.LocalName))
				return false;
			try
			{
				if (MaskJsonSerializedEnabled && MaskJsonSerializedOnXmlAttributeEnabled
					&& _jsonMasker != null && _jsonMasker.TryParse(source.Value, out var parsedNode))
				{
					//todo depth limit? for attribute, it's probably simple JSON, don't go too deeper.
					var sb = new StringBuilder(source.Value.Length);
					//var masked = MaskObjectInternal(parsedNode);
					_jsonMasker.MaskObject(parsedNode, sb);
					source.SetValue(sb.ToString());
					return true;
				}
				else if (MaskXmlSerializedEnabled && MaskXmlSerializedOnXmlAttributeEnabled
					&& TryParse(source.Value, out var element))
				{
					//todo introduce a new configuration here?
					//this should not happen I think.
					var masked = MaskXmlElementString(element);
					source.Value = masked;
					return true;
				}
			}
			catch (Exception)
			{
				//todo Parse Json failed
			}
			return false;
		}

		private void UrlJsonMaskAttempt(string key, string value, string valueKeyName, JsonObject source)
		{
			if (IsMaskUrlEnabled //&& valueIsString
				&& _options.UrlKeys.Contains(key, _options.SerializedKeysCaseSensitive
				? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
				&& IsLikeUrlOrQuery(value))
			{
				var masked = _masker.MaskUrl(value);
				source[valueKeyName] = masked;
			}
		}
		private void UrlJsonMaskAttempt(string key, XElement source)
		{
			if (IsMaskUrlEnabled && !string.IsNullOrEmpty(source.Value)
				&& _options.UrlKeys.Contains(key, _options.SerializedKeysCaseSensitive
				? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
				&& IsLikeUrlOrQuery(source.Value))
			{
				var masked = _masker.MaskUrl(source.Value);
				source.Value = masked;
			}
		}
		private bool UrlJsonMaskAttempt(XAttribute source)
		{
			if (IsMaskUrlEnabled && !string.IsNullOrEmpty(source.Value)
				&& _options.UrlKeys.Contains(source.Name.LocalName, _options.SerializedKeysCaseSensitive
				? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
				&& IsLikeUrlOrQuery(source.Value))
			{
				var masked = _masker.MaskUrl(source.Value);
				source.Value = masked;

				return true;
			}

			return false;
		}

		/// <summary>
		/// try parse XElement
		/// </summary>
		/// <param name="value"></param>
		/// <param name="doc"></param>
		/// <returns></returns>
		public bool TryParseXEle(string value, out XElement doc, int minLength = 30)
		{
			doc = null; value = value?.Trim();
			//todo performance improve...

			//here I think for xml, it at least has 30 char?...
			if (value == null || value.Length < minLength) return false;
			if (!value.StartsWith("<") || !value.EndsWith(">"))
				return false;
			try
			{
				doc = XElement.Parse(value, LoadOptions.None);

				return true;
			}
			catch (Exception)
			{
				doc = null;
				return false;
			}
		}
		#endregion

		#region -- helper methods--

		bool TryParseJson(string value, out JsonNode node)
		{
			node = null; value = value?.Trim();
			//here I think for JSON, it at least has 15 char?...
			if (value == null || value.Length < _options.JsonMinLength || value == "null") return false;

			if (!(value.StartsWith("[") && value.EndsWith("]"))
				&& !(value.StartsWith("{") && value.EndsWith("}")))
				return false;
			try
			{
				node = JsonNode.Parse(value);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// try parse XElement
		/// </summary>
		/// <param name="value"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool TryParse(string value, out XElement node, bool basicValidation = true)
		{
			node = null;
			//here I think for xml, it at least has 30 char?...
			if (value == null) return false;
			if (basicValidation && value.Length < _options.XmlMinLength) return false;

			if (!value.StartsWithExt('<') || !value.EndsWithExt('>'))
				return false;
			try
			{
				node = XElement.Parse(value, LoadOptions.None);

				return true;
			}
			catch (Exception)
			{
				node = null;
				return false;
			}
		}
		private bool IsLikeUrlOrQuery(string value)
		{
			if (value == null) return false;
			if (value.Length <= 5) return false; //at least key=value
			if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
				|| value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (value.Contains("="))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool IsSerializedKey(string key)
		{
			if ((MaskJsonSerializedEnabled || MaskXmlSerializedEnabled)
				&& _options.SerializedKeys.Contains(key,
				_options.SerializedKeysCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase))
				return true;

			return false;
		}
		#endregion
	}
}
