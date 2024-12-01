namespace Crews.PlanningCenter.PocoGenerator.Extensions;

public static class StringExtensions
{
	public static string ToXmlDocSummary(this string target, int indentSpaces = 0)
	{
		string indent = indentSpaces > 0 ? new(' ', indentSpaces) : string.Empty;

		string content = string.Join('\n', target
			.Split('\n', StringSplitOptions.TrimEntries)
			.Select(substring => $"{indent}/// {substring}"));

		return $"{indent}/// <summary>\n"
			+ content
			+ $"\n{indent}/// </summary>";
	}

	public static string ToSnakeCase(this string target) => target.Replace('-', '_');
}
