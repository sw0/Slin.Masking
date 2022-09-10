using NLog.Config;
using NLog.LayoutRenderers;
using NLog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System;

namespace Slin.Masking.NLog
{
	[LayoutRenderer("event-properties-masker")]
	[ThreadAgnostic]
	[MutableUnsafe]
	public class EventPropertiesMaskLayoutRenderer : LayoutRenderer
	{
		public readonly IObjectMasker _objectMasker;


		private string _mode = "object";
		/// <summary>
		/// Mode: OBJECT, URL, RESERIALIZE
		/// </summary>
		public string Mode
		{
			get { return _mode; }
			set { _mode = (value ?? "object").ToLower().Trim(); }
		}

		/// <summary>
		/// Property Name
		/// </summary>
		public string Item { get; set; }

		public EventPropertiesMaskLayoutRenderer() : base()
		{
			_objectMasker = ResolveService<IObjectMasker>();
		}

		/// <summary>
		/// Renders the specified environmental information and appends it to the specified <see cref="T:System.Text.StringBuilder" />.
		/// </summary>
		/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to append the rendered data to.</param>
		/// <param name="logEvent">Logging event.</param>
		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			if (logEvent.HasProperties)
			{
				if (logEvent.Properties == null || logEvent.Properties.Count == 0)
					return;

				if (!_objectMasker.IsEnabled)
				{
					var converter = this.ResolveService<IJsonConverter>();
					converter.SerializeObject(logEvent.Properties, builder);
					return;
				}

				if (!string.IsNullOrEmpty(Item))
				{
					if (!logEvent.Properties.ContainsKey(Item)) return;
				}

				if (Mode == "url")
				{
					if (string.IsNullOrEmpty(Item)) return;//todo warning

					if (!logEvent.Properties.TryGetValue(Item, out var value))
					{
						return;
					}
					if (value is string url)
					{
						var masked = _objectMasker.MaskUrl(url);
						builder.Append(string.Concat("\"", masked, "\""));
					}
					else
					{
						MaskObject(value, builder);
					}
				}
				else if (Mode == "reserialize")
				{
					if (!string.IsNullOrEmpty(Item) && logEvent.Properties.TryGetValue(Item, out var value))
					{
						if (value == null)
							return;

						if (value is string str)
						{
							if ((str.StartsWith("{") && str.EndsWith("}")
							|| str.StartsWith("[") && str.EndsWith("]")))
							{
								try
								{
									var data = JsonObject.Parse(str);

									var serialized = JsonSerializer.SerializeToNode(data, new JsonSerializerOptions
									{
										Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
									});

									var masked = _objectMasker.MaskJsonObjectString(serialized);
									builder.Append(masked);
								}
								catch (Exception)
								{
									//todo logging
									builder.Append("\"parsed failed\"");
								}
							}
							else
							{
								builder.Append(str);
							}
						}
						else
						{
							MaskObject(value, builder);
						}
					}
				}
				else
				{
					object data;
					if (string.IsNullOrEmpty(Item))
					{
						data = logEvent.Properties;
					}
					else
					{
						logEvent.Properties.TryGetValue(Item, out data);
					}

					MaskObject(data, builder);
				}
				//try { System.IO.File.WriteAllText(@"c:\tmp\abcd.log", serialized); } catch { }
			}
		}

		private void MaskObject(object data, StringBuilder builder)
		{
			if (data != null)
			{
				var serialized = JsonSerializer.SerializeToNode(data, new JsonSerializerOptions
				{
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				});

				var masked = _objectMasker.MaskJsonObjectString(serialized);
				builder.Append(masked);
			}
		}
	}


	//[LayoutRenderer("event-property-object-masker")]
	//[ThreadAgnostic]
	//[MutableUnsafe]
	//public class EventPropertyObjectMaskLayoutRenderer : LayoutRenderer
	//{
	//	public readonly IObjectMasker _objectMasker;

	//	[DefaultParameter]
	//	public string PropertyName { get; set; }

	//	public EventPropertyObjectMaskLayoutRenderer() : base()
	//	{
	//		_objectMasker = ResolveService<IObjectMasker>();
	//	}

	//	/// <summary>
	//	/// Renders the specified environmental information and appends it to the specified <see cref="T:System.Text.StringBuilder" />.
	//	/// </summary>
	//	/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to append the rendered data to.</param>
	//	/// <param name="logEvent">Logging event.</param>
	//	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	//	{
	//		if (logEvent.Properties.TryGetValue(PropertyName, out object value) && value != null)
	//		{
	//			if (_objectMasker.IsEnabled)
	//			{
	//				var serialized = JsonSerializer.SerializeToNode(value, new JsonSerializerOptions
	//				{
	//					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	//				});

	//				var masked = _objectMasker.MaskJsonObjectString(serialized);
	//				builder.Append(masked);

	//			}
	//			else
	//			{
	//				var converter = this.ResolveService<IJsonConverter>();
	//				converter.SerializeObject(value, builder);
	//			}
	//		}
	//	}
	//}

	//[LayoutRenderer("masked-url")]
	//[ThreadAgnostic]
	//[MutableUnsafe]
	//public class EventPropertyUrlMaskLayoutRenderer : LayoutRenderer
	//{
	//	private readonly IObjectMasker _objectMasker = null;

	//	[DefaultParameter]
	//	public string PropertyName { get; set; }
	//	//public List<string> Patterns { get; set; }

	//	public EventPropertyUrlMaskLayoutRenderer() : base()
	//	{
	//		_objectMasker = this.ResolveService<IObjectMasker>();
	//	}

	//	/// <summary>
	//	/// Renders the specified environmental information and appends it to the specified <see cref="T:System.Text.StringBuilder" />.
	//	/// </summary>
	//	/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to append the rendered data to.</param>
	//	/// <param name="logEvent">Logging event.</param>
	//	protected override void Append(StringBuilder builder, LogEventInfo logEvent)
	//	{
	//		if (logEvent.Properties.TryGetValue(PropertyName, out object value) && value != null && (value is string || value is Uri))
	//		{
	//			//if (_objectMasker.IsEnabled)
	//			//{
	//			//	var serialized = JsonSerializer.SerializeToNode(value, new JsonSerializerOptions
	//			//	{
	//			//		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
	//			//	});

	//			//	var masked = _objectMasker.MaskJsonObjectString(serialized);
	//			//	builder.Append(masked);

	//			//}
	//			//else
	//			//{
	//			//	var converter = this.ResolveService<IJsonConverter>();
	//			//	converter.SerializeObject(value, builder);
	//			//}
	//		}
	//	}
	//}
}
