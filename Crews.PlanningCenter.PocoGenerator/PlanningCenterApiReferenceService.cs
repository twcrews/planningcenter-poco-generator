using System.Text.Json;
using Crews.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Crews.PlanningCenter.PocoGenerator;

public class PlanningCenterApiReferenceService
{
	private readonly HttpClient _client;

	public class Options
	{
		public required Uri BaseAddress { get; set; }
	}

	public PlanningCenterApiReferenceService(HttpClient client, IOptions<Options> options)
	{
		_client = client;
		_client.SafelySetBaseAddress(options.Value.BaseAddress);
	}

	public async Task<JsonDocument> GetExample(string product, string version, string resource)
	{
		HttpResponseMessage response = await _client.GetAsync($"{product}/v2/documentation/{version}/vertices/{resource}");
		using JsonDocument document = await JsonDocument.ParseAsync(response.Content.ReadAsStream());

		if (document.RootElement.TryGetProperty("data", out JsonElement data) &&
			data.TryGetProperty("attributes", out JsonElement attributes) &&
			attributes.TryGetProperty("example", out JsonElement example))
		{
			string? exampleValue = example.GetString();
			if (string.IsNullOrWhiteSpace(exampleValue)) 
			{
				throw new JsonException("The JSON string is properly formatted, but the example object is empty");
			}
			return JsonDocument.Parse(example.GetString()!);
		}

		throw new JsonException("The JSON string is properly formatted, but has an unexpected hierarchy");
	}
}
