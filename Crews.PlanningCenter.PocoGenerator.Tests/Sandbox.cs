using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Crews.PlanningCenter.PocoGenerator.Tests;

public class Sandbox
{
  [Fact]
  public async Task Test1()
  {
    PlanningCenterApiReferenceService service = new(
      new(), Options.Create<PlanningCenterApiReferenceService.Options>(new()
      {
        BaseAddress = new("https://api.planningcenteronline.com/")
      }));

    JsonDocument document = await service.GetExample("calendar", "2022-07-07", "conflict");
    
    if (document.RootElement.TryGetProperty("type", out JsonElement type))
    {
      Console.WriteLine(type.GetString());
    }
  }
}