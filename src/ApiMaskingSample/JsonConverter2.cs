using System.Text.Json.Nodes;
using System.Text;
using NLog;
using NLog.MessageTemplates;
using System.Text.Json.Serialization;

namespace WebApi6
{
	public class JsonConverter2 : NLog.IJsonConverter
	{
		public bool SerializeObject(object value, StringBuilder builder)
		{
			if (value == null) return true;

			var json = System.Text.Json.JsonSerializer.SerializeToNode(value);

			MaskNode(json!);

			var jsonString = json!.ToJsonString();
			builder.Append(jsonString);
			return true;
		}

		void MaskNode(JsonNode node)
		{
			if (node == null) return;

			if (node is JsonArray array)
			{
				foreach (var item in array)
				{
					if (item == null) continue;
					MaskNode(item);
				}
			}
			else if (node is JsonObject obj)
			{
				foreach (var item in obj)
				{
					if (item.Value == null) continue;
					MaskNode(item.Value);
				}
			}
			else
			{

			}
		}
	}

}
