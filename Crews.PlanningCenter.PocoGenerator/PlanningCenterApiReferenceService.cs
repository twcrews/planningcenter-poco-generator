using System.Text.Json;
using Crews.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Crews.PlanningCenter.PocoGenerator;

public class PlanningCenterApiReferenceService
{
	private readonly HttpClient _client;

	private static JsonException NullJsonElementException => new("Unexpected null value in JSON element");
	private static JsonException BadJsonHierarchyException 
		=> new("The JSON string is properly formatted, but has an unexpected hierarchy");

	public class Options
	{
		public required Uri BaseAddress { get; set; }
	}

	public static IEnumerable<string> Products =>
 [
		"calendar",
		"check-ins",
		"giving",
		"groups",
		"people",
		"publishing",
		"services"
 ];

	public PlanningCenterApiReferenceService(HttpClient client, IOptions<Options> options)
	{
		_client = client;
		_client.SafelySetBaseAddress(options.Value.BaseAddress);
	}

	public async Task<IEnumerable<string>> GetVersionsAsync(string product)
	{
		HttpResponseMessage response = await _client.GetAsync($"{product}/v2/documentation");
		await using Stream content = await response.Content.ReadAsStreamAsync();
		JsonDocument document = await JsonDocument.ParseAsync(content);

		if (document.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
			dataElement.TryGetProperty("relationships", out JsonElement relationshipsElement) &&
			relationshipsElement.TryGetProperty("versions", out JsonElement versionsElement) &&
			versionsElement.TryGetProperty("data", out JsonElement versionsDataElement))
		{
			List<JsonElement> versionsElements = [.. versionsDataElement.EnumerateArray()];
			return versionsElements.Select(e =>
			{
				if (e.TryGetProperty("id", out JsonElement idElement))
				{
					return idElement.GetString() ?? throw NullJsonElementException;
				}
				throw BadJsonHierarchyException;
			});
		}
		throw BadJsonHierarchyException;
	}

	public async Task<IEnumerable<PlanningCenterResourceInfo>> GetResourcesInfoAsync(string product, string version)
	{
		HttpResponseMessage response = await _client.GetAsync($"{product}/v2/documentation/{version}");
		await using Stream content = await response.Content.ReadAsStreamAsync();
		JsonDocument document = await JsonDocument.ParseAsync(content);

		if (document.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
			dataElement.TryGetProperty("relationships", out JsonElement relationshipsElement) &&
			relationshipsElement.TryGetProperty("vertices", out JsonElement verticesElement) &&
			verticesElement.TryGetProperty("data", out JsonElement verticesDataElement))
		{
			List<JsonElement> verticesElements = [.. verticesDataElement.EnumerateArray()];
			return verticesElements.Select(e =>
			{
				if (e.TryGetProperty("attributes", out JsonElement vertexAttributesElement) &&
					vertexAttributesElement.TryGetProperty("name", out JsonElement nameElement) &&
					e.TryGetProperty("id", out JsonElement idElement))
				{
					JsonElement descriptionElement = vertexAttributesElement.GetProperty("description");
					string description = descriptionElement.GetString() ?? 
						"Planning Center does not provide a description for this resource.";

					return new PlanningCenterResourceInfo
					{
						ID = idElement.GetString() ?? throw NullJsonElementException,
						Name = nameElement.GetString() ?? throw NullJsonElementException,
						Description = description
					};
				}
				throw BadJsonHierarchyException;
			});
		}
		throw BadJsonHierarchyException;
	}

	public async Task<IEnumerable<PlanningCenterResourceAttributeInfo>> GetAttributesInfoAsync(
		string product, string version, string resource)
	{
		JsonDocument document = await GetResourceDocumentAsync(product, version, resource);

		if (document.RootElement.TryGetProperty("data", out JsonElement data) &&
			data.TryGetProperty("relationships", out JsonElement relationships) &&
			relationships.TryGetProperty("attributes", out JsonElement attributes) &&
			attributes.TryGetProperty("data", out JsonElement attributeData))
		{
			List<JsonElement> attributeElements = [.. attributeData.EnumerateArray()];
			return attributeElements.Select(e =>
			{
				if (e.TryGetProperty("attributes", out JsonElement elementAttributes) &&
					elementAttributes.TryGetProperty("name", out JsonElement nameElement) &&
					elementAttributes.TryGetProperty("type_annotation", out JsonElement typeAnnotation) &&
					typeAnnotation.TryGetProperty("name", out JsonElement typeNameElement))
				{
					JsonElement descriptionElement = elementAttributes.GetProperty("description");
					string description = descriptionElement.GetString() ?? 
						"Planning Center does not provide a description for this attribute.";

					return new PlanningCenterResourceAttributeInfo
					{
						Name = nameElement.GetString() ?? throw NullJsonElementException,
						Description = description,
						Type = typeNameElement.GetString() ?? throw NullJsonElementException,
						ClrTypeName = GetClrTypeName(typeNameElement.GetString() ?? throw NullJsonElementException)
					};
				}
				throw BadJsonHierarchyException;
			});
		}
		throw BadJsonHierarchyException;
	}

	private async Task<JsonDocument> GetResourceDocumentAsync(string product, string version, string resource)
	{
		HttpResponseMessage response = await _client.GetAsync($"{product}/v2/documentation/{version}/vertices/{resource}");
		await using Stream content = await response.Content.ReadAsStreamAsync();
		return await JsonDocument.ParseAsync(content);
	}

	private static string GetClrTypeName(string typeName) => typeName switch
	{
		"string" or "primary_key" or "currency_abbreviation" => "string",
		"date_time" => "DateTime",
		"integer" => "int",
		"boolean" => "bool",
		"float" => "double",
		"array" => "IEnumerable<JsonElement>",
		"date" => "DateOnly",
		_ => "JsonElement",
	};
}
