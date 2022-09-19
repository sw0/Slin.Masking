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
using System.Linq;
using System.Collections.Generic;

namespace Slin.Masking.NLog
{
	[LayoutRenderer("event-properties-masker")]
	[ThreadAgnostic]
	[MutableUnsafe]
	public class EventPropertiesMaskerLayoutRenderer : LayoutRenderer
	{
		public readonly IObjectMasker _objectMasker;

		private string _mode = "object";
		[DefaultParameter]
		/// <summary>
		/// Mode: 
		/// - object (default)
		/// - url
		/// - reserialize
		/// </summary>
		public string Mode
		{
			get { return _mode; }
			set { _mode = (value ?? "object").ToLower().Trim(); }
		}

		/// <summary>
		/// - disabled : masking is disabled
		/// </summary>
		public bool Disabled { get; set; }

		private List<string> _excludeProperties;

		/// <summary>
		/// it can be set like 'field1,field2,field3'. If <see cref="Item"/> is set, this property will be ignored.
		/// </summary>
		public string ExcludeProperties
		{
			get { return _excludeProperties == null ? "" : string.Join(",", _excludeProperties); }
			set { _excludeProperties = value?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList(); }
		}

		/// <summary>
		/// Property Name. 
		/// <remarks>after v0.1.30, it's became case-insensitive.</remarks>
		/// </summary>
		public string Item { get; set; }

		public EventPropertiesMaskerLayoutRenderer() : base()
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


			if (Disabled || !_objectMasker.Enabled)//including disabled
			{
				if (string.IsNullOrEmpty(Item))
				{
					var converter = ResolveService<IJsonConverter>();

					var started = false;
					builder.Append('{');
					foreach (var item in logEvent.Properties.Where(kvp =>
					_excludeProperties == null || _excludeProperties.Count == 0
					|| !_excludeProperties.Contains(kvp.Key?.ToString(), StringComparer.OrdinalIgnoreCase)))
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
					var value = logEvent.Properties.FirstOrDefault(kvp => Item.Equals(kvp.Key?.ToString(), StringComparison.OrdinalIgnoreCase)).Value;

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

				var value = logEvent.Properties.FirstOrDefault(kvp => Item.Equals(kvp.Key?.ToString(), StringComparison.OrdinalIgnoreCase)).Value;

				if (value == null)
					return;

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
				if (string.IsNullOrEmpty(Item)) return;//todo warning

				var value = logEvent.Properties.FirstOrDefault(kvp => Item.Equals(kvp.Key?.ToString(), StringComparison.OrdinalIgnoreCase)).Value;

				if (value == null)
					return;

				var masked = _objectMasker.MaskObject(value);
				builder.Append(masked);
			}
			else
			{
				object data = null;
				if (string.IsNullOrEmpty(Item))
				{
					data = logEvent.Properties;

					if (_excludeProperties != null && _excludeProperties.Any())
					{
						data = logEvent.Properties.Where(kvp =>
							!_excludeProperties.Contains(kvp.Key?.ToString(), StringComparer.OrdinalIgnoreCase))
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
					}
				}
				else
				{
					data = logEvent.Properties.FirstOrDefault(kvp => Item.Equals(kvp.Key?.ToString(), StringComparison.OrdinalIgnoreCase)).Value;
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
