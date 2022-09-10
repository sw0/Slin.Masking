using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Slin.Masking
{
	public interface IJsonMasker
	{
		string MaskJsonObjectString(JsonNode node);
	}

	public interface IXmlMasker
	{
		string MaskXmlElementString(XElement node);
	}

	public interface IObjectMasker : IJsonMasker, IXmlMasker, IUrlMasker
	{
		string MaskObject(object value);

		/// <summary>
		/// get if masking is enabled
		/// </summary>
		bool IsEnabled { get; }
	}

	/// <summary>
	/// ObjectMasker can mask values for those properties set in rules
	/// </summary>
	public class ObjectMasker : IObjectMasker
	{
		private readonly IMasker _masker = null;
		private readonly IObjectMaskingOptions _options;

		private bool SerializedUnfoldable => _options.MaskJsonSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();

		private bool IsMaskUrlEnabled => _options.MaskUrlEnabled && _options.UrlKeys != null
			&& _options.UrlKeys.Any();


		private bool IsNestedKvpEnabled => true;//todo from configuration

		public bool IsEnabled => _options.Enabled;

		private static readonly List<KeyValuePair<string, string>> DefaultKeyValueNames = new List<KeyValuePair<string, string>> {
			new KeyValuePair<string, string>("Key","Value"),
			new KeyValuePair<string, string>("key","value"),
		};

		public ObjectMasker(IMasker masker, IObjectMaskingOptions options)
		{
			_masker = masker;
			_options = options ?? new ObjectMaskingOptions();
		}

		public string MaskObject(object data)
		{
			if (data == null)
				return null;

			if (data is JsonNode node)
			{
				return MaskJsonObjectString(node);
			}

			if (data is XElement element)
				return MaskXmlElementString(element);
			//if (data is XNode element2)
			//	return MaskXmlElementString(element);

			if (data is string json && TryParseJson(json, out node))
			{
				return MaskJsonObjectString(node);
			}
			else if (data is string xml && TryParseXDoc(xml, out element))
			{
				return MaskXmlElementString(element);
			}
			else
			{
				try
				{
					node = JsonSerializer.SerializeToNode(data);

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
				var noneEmptyValueList = GetNotEmptyJsonValueItems(obj).ToList();

				if (IsNestedKvpEnabled &&
					noneEmptyValueList.Count >= 2 && noneEmptyValueList.Count < 5
					&& ContainsKeyValuePair(noneEmptyValueList, out string keyKeyName, out string valKeyName))
				{
					if (noneEmptyValueList.First(kvp => kvp.Key == keyKeyName).Value.AsValue().TryGetValue(out string key))
					{
						var jval = (JsonValue)noneEmptyValueList.First(kvp => kvp.Key == valKeyName).Value;

						var value = default(string);
						var valueIsString = true;

						if (!FirstJsonMaskAttempt(key, valKeyName, jval, obj, out value, out valueIsString))
						{
							SerializedJsonMaskAttempt(key, value, valKeyName, obj);

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

						if (FirstJsonMaskAttempt(item.Key, item.Key, jval, obj, out value, out valueIsString))
							continue;

						//if (!jval.TryGetValue<string>(out var value))
						//{
						//	isString = false;
						//	//if not string, it would be ValueKind.Number 
						//	value = jval.GetValue<double>().ToString();
						//}
						//if (_masker.TryMask(key, value, out string masked))
						//{
						//	obj[item.Key] = masked;
						//	continue;
						//}

						////mask serialized item
						//if (IsSerializedKey(key))
						//{
						//	try
						//	{
						//		if (TryParseJson(value, out var parsedNode))
						//		{
						//			obj[item.Key] = parsedNode;

						//			MaskObjectInternal(parsedNode);
						//		}
						//		else if (TryParseXDoc(value, out var element))
						//		{
						//			var masked = MaskXmlElementString(element);
						//			obj[item.Key] = masked;
						//		}
						//	}
						//	catch (Exception)
						//	{
						//		//todo Parse Json failed
						//	}
						//}
						SerializedJsonMaskAttempt(key, value, key, obj);

						////mask url
						//if (IsMaskUrlEnabled && valueIsString
						//	&& _options.UrlKeys.Contains(item.Key, _options.SerializedKeysCaseSensitive
						//	? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
						//	&& IsLikeUrlOrQuery(value))
						//{
						//	var masked = _masker.MaskUrl(value);
						//	obj[item.Key] = masked;
						//}
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
		private IEnumerable<KeyValuePair<string, JsonNode>> GetNotEmptyJsonValueItems(JsonObject node)
		{
			foreach (var item in node)
			{
				if (item.Value == null) continue;
				if (item.Value is JsonValue jv)
				{
					yield return item;
					//todo improve performance by returning JsonElement back also
					//var v = jv.GetValue<JsonElement>();
					//if (v.ValueKind == JsonValueKind.Number && _options.MaskJsonNumberEnabled)
					//{
					//	yield return item;
					//}
					//if (v.ValueKind == JsonValueKind.String && jv.GetValue<string>().Length >= _options.ValueMinLength) //todo I think we can put X here 
					//	yield return item;
					//	yield return new KeyValuePair<string, JsonNode>(item.Key, "53");

					//todo we does not support number for now.
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
		private bool ContainsKeyValuePair(ICollection<KeyValuePair<string, JsonNode>> source, out string keyName, out string valueName, List<KeyValuePair<string, string>> eligibleKeyValueNames = null)
		{
			keyName = valueName = null;

			foreach (var item in eligibleKeyValueNames ?? DefaultKeyValueNames)
			{
				//here: item.Key is keyName, item.Value is value keyName. By default: it's "Key","Value".
				//Let's make it simple here, that assuming the value type are strings for matched item for key and value.
				var k = source.FirstOrDefault(x => x.Key == item.Key);
				if (k.Key != null && k.Value != null)
				{
					//var valueOfKey = ((JsonValue)k.Value).GetValue<JsonElement>();
					//if (valueOfKey.ValueKind != JsonValueKind.String)
					//	continue;

					var v = source.FirstOrDefault(x => x.Key == item.Value);

					if (v.Key != null && v.Value != null)
					{
						//var valueOfValue = ((JsonValue)v.Value).GetValue<JsonElement>();

						//if (valueOfValue.ValueKind != JsonValueKind.String 
						//	&& valueOfValue.ValueKind != JsonValueKind.Number) //number check for Amount
						//	continue;

						keyName = k.Key;
						valueName = v.Key;
						return true;
					}
				}
			}
			//}

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

		private bool SerializedJsonMaskAttempt(string key, string value, string valueKeyName, JsonObject source)
		{
			if (IsSerializedKey(key))
			{
				try
				{
					if (TryParseJson(value, out var parsedNode))
					{
						source[valueKeyName] = parsedNode;

						MaskObjectInternal(parsedNode);

						return true;
					}
					else if (TryParseXDoc(value, out var element))
					{
						var masked = MaskXmlElementString(element);
						source[value] = masked;
						return true;
					}
				}
				catch (Exception)
				{
					//todo Parse Json failed
				}
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
		#endregion
		#endregion

		#region -- XML -- 

		public string MaskXmlElementString(XElement element)
		{
			MaskXElementInternal(element);

			return element.ToString();
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
					else if (IsSerializedKey(attr.Name.LocalName))
					{

					}
				}
			}

			// read the element and do with your node 
			if (element.HasElements)
			{
				// here you can reach nested node 
				foreach (var item in element.Elements())
				{
					MaskXElementInternal(item);
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
				else if (IsSerializedKey(name))
				{
					try
					{
						if (TryParseJson(value, out var parsedNode))
						{
							element.Value = MaskObjectInternal(parsedNode);
						}
						else if (TryParseXDoc(element.Value, out var parsedElement))
						{
							element.Value = MaskXmlElementString(parsedElement);
						}
					}
					catch (Exception)
					{
						//todo Parse Json failed
					}
				}

				//mask url
				if (IsMaskUrlEnabled && _options.UrlKeys.Contains(name, _options.SerializedKeysCaseSensitive
					? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
					&& IsLikeUrlOrQuery(value))
				{
					masked = _masker.MaskUrl(value);
					element.Value = masked;
				}
			}
		}
		#endregion

		#region -- helper methods--

		bool TryParseJson(string value, out JsonNode node)
		{
			node = null; value = value?.Trim();
			//here I think for JSON, it at least has 15 char?...
			if (value == null || value.Length < 15 || value == "null") return false;

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

		bool TryParseXDoc(string value, out XElement doc)
		{
			doc = null; value = value?.Trim();
			//here I think for xml, it at least has 30 char?...
			if (value == null || value.Length < 30) return false;
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
			if (SerializedUnfoldable && _options.SerializedKeys.Contains(key,
				_options.SerializedKeysCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase))
				return true;

			return false;
		}
		#endregion

	}
}
