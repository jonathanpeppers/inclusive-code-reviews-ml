using CsvHelper.Configuration.Attributes;

namespace MLTrainer.Models;

public class Sentence
{
	[Name("Body")]
	public string Body { get; set; } = string.Empty;
}
