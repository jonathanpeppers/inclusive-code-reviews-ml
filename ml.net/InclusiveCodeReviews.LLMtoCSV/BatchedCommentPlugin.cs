using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

public class BatchedCommentPlugin
{
	public const int BatchSize = 10;

	/// <summary>
	/// TODO: how do I make this not static?
	/// </summary>
	public static readonly List<Comment> Comments = new(capacity: BatchSize);

	[KernelFunction("sentences"), Description("Returns the current list of sentences")]
	public string GetSentences(IFormatProvider? formatProvider = null)
	{
		string json = JsonSerializer.Serialize(new Dictionary<string, object>
		{
			{ "comments", Comments.Select (c => c.Text).ToArray() },
		});
		return json;
	}
}
