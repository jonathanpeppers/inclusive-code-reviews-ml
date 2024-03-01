using System.ComponentModel;
using System.Text;
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
		var builder = new StringBuilder();
		foreach (var comment in Comments)
		{
			builder.AppendLine(comment.Text);
		}
		return builder.ToString();
	}
}
