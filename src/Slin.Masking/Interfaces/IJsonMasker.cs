using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Slin.Masking
{
	public interface IJsonMasker
	{
		//[Obsolete("JsonNode has unicode escape issue, suggest not use it. Might be removed in future if the issue is not fixed.")]
		//string MaskJsonObjectString(JsonNode node);

		void MaskObject(object source, StringBuilder builder);

		bool TryParse(string source, out JsonElement? node, bool basicValidation = true);
	}
}
