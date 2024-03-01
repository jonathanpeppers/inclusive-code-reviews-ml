using CsvHelper.Configuration.Attributes;

/// <summary>
/// See comments/classified.csv
/// </summary>
public class Comment
{
	[Name("text")]
	[Optional]
	public string Text { get; set; } = string.Empty;

	[Name("isnegative")]
	[Optional]
	public int IsNegative { get; set; }
}
