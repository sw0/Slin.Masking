using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Slin.Masking.NLog
{
	public interface IJsonMasker
	{
		string MaskObjectString(JsonNode node);
	}

	public class JsonMasker : IJsonMasker
	{
		private readonly IMasker _masker = null;
		private readonly LogMaskingOptions _options;

		private bool SerializedUnfoldable => _options.EnabledUnfoldSerialized
			&& _options.SerializedKeys != null && _options.SerializedKeys.Count > 0;

		public JsonMasker(IMasker masker, LogMaskingOptions options)
		{
			_masker = masker;
			_options = options ?? new LogMaskingOptions();
		}

		public string MaskObjectString(JsonNode node)
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

					if (!jval.TryGetValue<string>(out var value))
					{
						//if not string, it would be ValueKind.Number 
						value = jval.GetValue<double>().ToString();
					}
					if (_masker.TryMask(item.Key, value, out string masked))
					{
						obj[item.Key] = masked;
						continue;
					}

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
					if (v.ValueKind == JsonValueKind.Number && _options.EnableJsonNumberMasking)
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


	//[LayoutRenderer("masked-url")]
	//[ThreadAgnostic]
	//[MutableUnsafe]
	//public class UrlMakableLayoutRenderer : LayoutRenderer
	//{
	//	public readonly IMaskEngine _maskEngine;

	//	public List<string> Patterns { get; set; }

	//	public UrlMakableLayoutRenderer() : base()
	//	{
	//		_maskEngine = this.ResolveService<IMaskEngine>();
	//	}

	//	/// <summary>
	//	/// Renders the specified environmental information and appends it to the specified <see cref="T:System.Text.StringBuilder" />.
	//	/// </summary>
	//	/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to append the rendered data to.</param>
	//	/// <param name="logEvent">Logging event.</param>
	//	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	//	{
	//		if (!string.IsNullOrWhiteSpace(logEvent.Message))
	//		{
	//			var uri = new Uri(logEvent.Message,UriKind.RelativeOrAbsolute);

	//			foreach (var item in Patterns)
	//			{

	//			}


	//			var masked = _maskEngine.MaskObjectString(logEvent.Message);
	//			builder.Append(masked);
	//		}
	//	}
	//}
}
