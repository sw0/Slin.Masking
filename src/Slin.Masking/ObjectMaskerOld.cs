using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Xml.Linq;
using System.Diagnostics.Contracts;
using System.Text;
using System.Reflection;

namespace Slin.Masking
{
	/// <summary>
	/// it has bug and kept it here just for comprison, like performance...
	/// </summary>
	internal class ObjectMaskerV1 : IObjectMasker
	{
		protected readonly IMasker _masker = null;
		protected readonly IJsonMasker _jMasker = null;
		protected readonly IXmlMasker _xMasker = null;
		protected readonly IObjectMaskingOptions _options;

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

		///// <summary>
		///// Enabled is exposed to caller. It has no internal logic check on it inside <see cref="ObjectMasker"/>.
		///// </summary>
		//public bool Enabled => _options.Enabled;

		public ObjectMaskerV1(IMasker masker, IObjectMaskingOptions options)
		{
			_masker = masker;
			var jMasker = new JsonMasker(masker, options);
			var xMasker = new XmlMasker(masker, options);

			jMasker.SetXmlMasker(xMasker);
			xMasker.SetJsonMasker(jMasker);

			_jMasker = jMasker;
			_xMasker = xMasker;

			_options = options ?? new ObjectMaskingOptions();
		}

		public bool TryParse(string value, out JsonElement? node, bool basicValidation = true)
		{
			Contract.Assert(_jMasker != null, "_jMasker should be initialized");
			return _jMasker.TryParse(value, out node, basicValidation);
		}

		public bool TryParse(string value, out XElement node, bool basicValidation = true)
		{
			Contract.Assert(_xMasker != null, "_xMasker should be initialized");
			return _xMasker.TryParse(value, out node, basicValidation);
		}

		/// <summary>
		/// MaskObject and append the masked reuslt to builder.
		/// NOTE: if value is XElement or JsonNode, the original instance will be masked if masking happened.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="builder"></param>
		public void MaskObject(object value, StringBuilder builder)
		{
			if (value == null) return;

			if (value is JsonElement ele)
			{
				_jMasker.MaskObject(ele, builder);
			}
			else if (value is JsonNode node)
			{
				//todo please don't use JsonNode
				var result = MaskObject(node);
				builder.Append(result);
			}
			else if (value is XElement xEle)
			{
				var result = _xMasker.MaskXmlElementString(xEle);
				builder.Append(result);
			}
			else if (value is string str)
			{
				if (_jMasker.TryParse(str, out var ele2))
				{
					_jMasker.MaskObject(ele2, builder);
				}
				else if (_xMasker.TryParse(str, out var parsedNode))
				{
					var result = _xMasker.MaskXmlElementString(parsedNode);
					builder.Append(result);
				}
			}
			else
			{
				_jMasker.MaskObject(value, builder);
			}
		}

		/// <summary>
		/// MaskObject and append the masked reuslt to builder.
		/// NOTE: if value is XElement or JsonNode, the original instance will be masked if masking happened.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string MaskObject(object value)
		{
			if (value == null) return "";

			var builder = new StringBuilder();

			MaskObject(value, builder);

			return builder.ToString();
		}

		/// <summary>
		/// this way has performance issue.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		internal string MaskObjectOldway(object data)
		{
			if (data == null)
				return null;

			if (data is JsonNode node)
			{
				return MaskJsonObjectString(node);
			}
			else if (data is JsonElement jEle)
			{
				var sb = new StringBuilder();

				_jMasker.MaskObject(jEle, sb);

				return sb.ToString();
			}
			else if (data is XElement element)
				return MaskXmlElementString(element);
			else if (data is XDocument xdoc)
				return MaskXmlElementString(xdoc.Root);
			else if (data is string json && TryParseJson(json, out node))
			{
				return MaskJsonObjectString(node);
			}
			else if (data is string xml && TryParseXEle(xml, out var element2))
			{
				return MaskXmlElementString(element2);
			}
			else
			{
				try
				{
					//var encoderSettings = new TextEncoderSettings();
					//encoderSettings.AllowCharacters('\u0026', '&');
					//encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
					var jsonOptions = new JsonSerializerOptions()
					{
						//Encoder = JavaScriptEncoder.Create(encoderSettings)
						Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
					};
					//TODO looks like it's Microsoft has bug here for SerializeToNode method to use Encoder
					node = JsonSerializer.SerializeToNode(data, jsonOptions);

					return MaskJsonObjectString(node);
				}
				catch (Exception)
				{
					//todo logging
				}
				return data.ToString();
			}
		}

		#region -- URL --

		public string MaskUrl(string url, bool maskParamters = true, params UrlMaskingPattern[] overwrittenPatterns) => _masker.MaskUrl(url, maskParamters, overwrittenPatterns);

		#endregion

		#region -- JSON --

		public string MaskJsonObjectString(JsonNode node)
		{
			return MaskObjectInternal(node);
		}

		protected string MaskObjectInternal(JsonNode node)
		{
			if (node == null) return string.Empty;

			if (node is JsonArray array)
			{
				foreach (var item in array)
				{
					if (item == null) continue;
					MaskObjectInternal(item);
				}
			}
			else if (node is JsonObject obj)
			{
				var noneEmptyValueList = NotEmptyJsonValueItems(obj).ToList();
				//we need update node. so ToList()

				if (MaskNestedKvpEnabled
					&& ContainsKeyValuePair(noneEmptyValueList, out string keyKeyName, out string valKeyName, _options.KeyKeyValueKeys)
					)
				{
					if (noneEmptyValueList.First(kvp => kvp.Key == keyKeyName).Value.AsValue().TryGetValue(out string key))
					{
						obj.TryGetPropertyValue(valKeyName, out var valNode);
						var jval = valNode as JsonValue;

						var value = default(string);
						var valueIsString = true;

						if (!jval.TryGetValue<bool>(out _) && !FirstJsonMaskAttempt(key, valKeyName, jval, obj, out value, out valueIsString)
							&& value != null && value.Length > _options.ValueMinLength)
						{
							SerializedMaskAttempt(key, value, valKeyName, obj);

							UrlJsonMaskAttempt(key, value, valKeyName, obj);
						}
					}

					foreach (var item in obj)
					{
						if (item.Key == keyKeyName) continue;
						if (item.Key == valKeyName) continue;
						if (item.Value == null) continue;
						if (item.Value is JsonValue) continue;
						if (key != null && IsSerializedKey(key)) continue;
						MaskObjectInternal(item.Value);
					}
				}
				else
				{
					foreach (var item in noneEmptyValueList)
					{
						var key = item.Key;

						var jval = (JsonValue)item.Value; //here all items are JsonValue
						var value = default(string);
						var valueIsString = true;

						if (jval.TryGetValue<bool>(out _) || FirstJsonMaskAttempt(item.Key, item.Key, jval, obj, out value, out valueIsString))
							continue;

						if (value == null || value.Length <= _options.ValueMinLength)
							continue;

						//mask serialized (xml or JSON)
						SerializedMaskAttempt(key, value, key, obj);

						//mask url
						if (valueIsString)
							UrlJsonMaskAttempt(key, value, key, obj);
					}

					foreach (var item in obj)
					{
						if (item.Value == null) continue;
						if (item.Value is JsonValue) continue;
						if (IsSerializedKey(item.Key)) continue;
						MaskObjectInternal(item.Value);
					}
				}

			}
			else
			{
				//do nothing here, it should be already masked above
				if (node is JsonValue == false)
				{
					throw new Exception("asdfsdfdsfsf NOT value");
				}
			}

			return node.ToJsonString();
		}


		/// <summary>
		/// Only JsonValue item will be returned.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private IEnumerable<KeyValuePair<string, JsonNode>> NotEmptyJsonValueItems(JsonObject node)
		{
			foreach (var item in node)
			{
				if (item.Value == null) continue;
				if (item.Value is JsonValue)
				{
					yield return item;
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
		private bool ContainsKeyValuePair(IEnumerable<KeyValuePair<string, JsonNode>> source, out string keyName, out string valueName, List<KeyKeyValueKey> eligibleKeyValueNames = null)
		{
			keyName = valueName = null;

			foreach (var item in eligibleKeyValueNames ?? KeyKeyValueKey.DefaultKeyKeyValueKeys)
			{
				//here: item.Key is keyName, item.Value is value keyName. By default: it's "Key","Value".
				//Let's make it simple here, that assuming the value type are strings for matched item for key and value.
				var k = source.FirstOrDefault(x => x.Key == item.KeyKeyName);
				if (k.Key != null && k.Value != null)
				{
					var v = source.FirstOrDefault(x => x.Key == item.ValueKeyName);

					if (v.Key != null && v.Value != null)
					{
						keyName = k.Key;
						valueName = v.Key;
						return true;
					}
				}
			}

			return false;
		}

		private bool IsKeyValuePairObject(JsonObject source, out string keyKeyName, out string valKeyName, List<KeyKeyValueKey> eligibleKeyValueNames = null)
		{
			keyKeyName = valKeyName = null;

			foreach (var item in eligibleKeyValueNames ?? KeyKeyValueKey.DefaultKeyKeyValueKeys)
			{
				if (source.TryGetPropertyValue(item.KeyKeyName, out var keyNode)
					&& keyNode != null && keyNode is JsonValue a
					&& source.TryGetPropertyValue(item.ValueKeyName, out var valNode))
				{
					keyKeyName = item.KeyKeyName;
					valKeyName = item.ValueKeyName;
					return true;
				}
			}

			return false;
		}

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
				if (MaskJsonSerializedEnabled && TryParseJson(value, out var parsedNode))
				{
					source[valueKeyName] = parsedNode;

					MaskObjectInternal(parsedNode);

					return true;
				}
				else if (MaskXmlSerializedEnabled && TryParseXEle(value, out var element))
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
				if (MaskJsonSerializedEnabled && TryParseJson(source.Value, out var parsedNode))
				{
					var masked = MaskObjectInternal(parsedNode);
					source.Value = masked;
					return true;
				}
				else if (MaskXmlSerializedEnabled && TryParseXEle(source.Value, out var element))
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
					&& TryParseJson(source.Value, out var parsedNode))
				{
					//todo depth limit? for attribute, it's probably simple JSON, don't go too deeper.
					var masked = MaskObjectInternal(parsedNode);
					source.Value = masked;
					return true;
				}
				else if (MaskXmlSerializedEnabled && MaskXmlSerializedOnXmlAttributeEnabled
					&& TryParseXEle(source.Value, out var element))
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
		#endregion
		#endregion

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
		/// <param name="doc"></param>
		/// <returns></returns>
		bool TryParseXEle(string value, out XElement doc)
		{
			doc = null; value = value?.Trim();
			//here I think for xml, it at least has 30 char?...
			if (value == null || value.Length < _options.XmlMinLength) return false;
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
