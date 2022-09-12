using NLog.Config;
using NLog.LayoutRenderers;
using NLog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System;
using NLog.Layouts;

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
		/// Mode: 
		/// - object (default)
		/// - url
		/// - reserialize
		/// - disabled : masking is disabled
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
			if (logEvent.Properties == null || logEvent.Properties.Count == 0 || !logEvent.HasProperties)
				return;

			if (!string.IsNullOrEmpty(Item))
			{
				//NOTE: here it's case sensitive here!!!
				if (!logEvent.Properties.ContainsKey(Item)) return;
			}

			if (Mode.StartsWith("disable"))//including disabled
			{
				if (string.IsNullOrEmpty(Item))
				{
					var converter = ResolveService<IJsonConverter>();

					var started = false;
					builder.Append('{');
					foreach (var item in logEvent.Properties)
					{
						if (started) builder.Append(',');
						else { started = true; }

						builder.Append('"').Append(item.Key)
							.Append('"').Append(':');
						converter.SerializeObject(item.Value, builder);
					}
					builder.Append('}');
				}
				else
				{
					var value = logEvent.Properties[Item];

					if (value != null)
					{
						var converter = this.ResolveService<IJsonConverter>();
						converter.SerializeObject(value, builder);
					}
				}
				return;
			}
			else if (Mode == "url")
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
					var masked = _objectMasker.MaskObject(value);
					builder.Append(string.Concat("\"", masked, "\""));
				}
			}
			else if (Mode == "reserialize")
			{
				if (!string.IsNullOrEmpty(Item) && logEvent.Properties.TryGetValue(Item, out var value))
				{
					if (value == null)
						return;

					var masked = _objectMasker.MaskObject(value);
					builder.Append(masked);
				}
				else
				{
					//warnning
					//do nothing, item is not set!
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
				if (data != null)
				{
					var masked = _objectMasker.MaskObject(data);
					builder.Append(masked);
				}
			}
		}
	}
}
