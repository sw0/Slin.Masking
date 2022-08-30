using NLog.Config;
using NLog.LayoutRenderers;
using NLog;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;

namespace Slin.Masking.NLog
{
	[LayoutRenderer("event-properties-masker")]
	[ThreadAgnostic]
	[MutableUnsafe]
	public class EventPropertiesMaskLayoutRenderer : LayoutRenderer
	{
		public readonly IObjectMasker _jsonMasker;

		public EventPropertiesMaskLayoutRenderer() : base()
		{
			_jsonMasker = ResolveService<IObjectMasker>();
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
				//var serialized = logEvent.Properties.ToJson();

				var serialized = JsonSerializer.SerializeToNode(logEvent.Properties, new JsonSerializerOptions {
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				});

				var masked = _jsonMasker.MaskJsonObjectString(serialized);
				builder.Append(masked);
				//try { System.IO.File.WriteAllText(@"c:\tmp\abcd.log", serialized); } catch { }
			}
		}
	}


	//[LayoutRenderer("masked-url")]
	//[ThreadAgnostic]
	//[MutableUnsafe]
	//public class UrlMakableLayoutRenderer : LayoutRenderer
	//{
	//	private readonly IUrlMasker _masker = null;

	//	public UrlMakableLayoutRenderer(IUrlMasker masker)
	//	{
	//		_masker = masker;
	//	}

	//	//public List<string> Patterns { get; set; }

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
	//			var uri = new Uri(logEvent.Message, UriKind.RelativeOrAbsolute);

	//			foreach (var item in Patterns)
	//			{

	//			}


	//			var masked = _maskEngine.MaskObjectString(logEvent.Message);
	//			builder.Append(masked);
	//		}
	//	}
	//}
}
