using Xunit.Abstractions;
using Crews.PlanningCenter.PocoGenerator.Extensions;
using Crews.Extensions.Primitives;

namespace Crews.PlanningCenter.PocoGenerator.Tests;

public class Sandbox(ITestOutputHelper output)
{
  [Fact]
  public void Test1()
  {
    output.WriteLine("This\nis a\ntest.".ToXmlDocSummary(indentSpaces: 4));
    output.WriteLine("2022-07-07".ToPascalCase());
    output.WriteLine("2024-12-01".ToSnakeCase());
  }
}