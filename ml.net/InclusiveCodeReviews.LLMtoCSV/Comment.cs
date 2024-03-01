using CsvHelper.Configuration.Attributes;

/// <summary>
/// See comments/classified.csv
/// </summary>
public class Comment
{
	[Name("text")]
	public string Text { get; set; } = string.Empty;

	[Name("isnegative")]
	public int IsNegative { get; set; }

	[Name("importance")]
	public float Importance { get; set; } = 0.5f;
}
