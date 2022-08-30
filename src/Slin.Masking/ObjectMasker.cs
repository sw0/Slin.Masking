using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Net.Http.Headers;

namespace Slin.Masking
{
	public interface IJsonMasker
	{
		string MaskJsonObjectString(JsonNode node);
	}

	public interface IObjectMasker : IJsonMasker
	{
		/// <summary>
		/// get if masking is enabled
		/// </summary>
		bool IsEnabled { get; }
	}

	public class ObjectMasker : IObjectMasker
	{
		private readonly IMasker _masker = null;
		private readonly ObjectMaskingOptions _options;

		private bool SerializedUnfoldable => _options.MaskJsonSerializedEnabled
			&& _options.SerializedKeys != null && _options.SerializedKeys.Any();

		private bool IsMaskUrlEnabled => _options.MaskUrlEnabled && _options.UrlKeys != null
			&& _options.UrlKeys.Any();

		public ObjectMasker(IMasker masker, ObjectMaskingOptions options)
		{
			_masker = masker;
			_options = options ?? new ObjectMaskingOptions();
		}

		public bool IsEnabled => _options.Enabled;

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

				foreach (var item in noneEmptyValueList)
				{
					var jval = (JsonValue)item.Value; //here all items are JsonValue
					var isString = true;
					if (!jval.TryGetValue<string>(out var value))
					{
						isString = false;
						//if not string, it would be ValueKind.Number 
						value = jval.GetValue<double>().ToString();
					}
					if (_masker.TryMask(item.Key, value, out string masked))
					{
						obj[item.Key] = masked;
						continue;
					}

					//mask serialized item
					if (IsSerializedKey(item.Key))
					{
						try
						{
							if (value.StartsWith("[") && value.EndsWith("]")
								|| value.StartsWith("{") && value.EndsWith("}"))
							{
								//todo do some simple validation 
								var parsedNode = JsonNode.Parse(value);
								if (parsedNode != null)
								{
									obj[item.Key] = parsedNode;

									MaskObjectInternal(parsedNode);
								}
							}
						}
						catch (Exception)
						{
							//todo Parse Json failed
						}
					}

					//mask url
					if (IsMaskUrlEnabled && isString
						&& _options.UrlKeys.Contains(item.Key, _options.SerializedKeysCaseSensitive
						? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
						&& IsLikeUrlOrQuery(value))
					{
						masked = _masker.MaskUrl(value);
						obj[item.Key] = masked;
					}
				}

				foreach (var item in obj)
				{
					if (item.Value == null) continue;
					if (item.Value is JsonValue) continue;
					if (IsSerializedKey(item.Key)) continue;
					MaskObjectInternal(item.Value);
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
					var v = jv.GetValue<JsonElement>();
					if (v.ValueKind == JsonValueKind.Number && _options.MaskJsonNumberEnabled)
					{
						yield return item;
					}
					if (v.ValueKind == JsonValueKind.String && jv.GetValue<string>().Length >= _options.ValueMinLength) //todo I think we can put X here 
						yield return item;
					//	yield return new KeyValuePair<string, JsonNode>(item.Key, "53");

					//todo we does not support number for now.
				}
			}
		}
	}
}
