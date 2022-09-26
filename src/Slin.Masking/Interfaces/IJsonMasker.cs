using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Slin.Masking
{
	public interface IJsonMasker
	{
		string MaskJsonObjectString(JsonNode node);

		void MaskObject(object source, StringBuilder builder);

		bool TryParse(string source, out JsonElement? node, bool basicValidation = true);
	}
}
