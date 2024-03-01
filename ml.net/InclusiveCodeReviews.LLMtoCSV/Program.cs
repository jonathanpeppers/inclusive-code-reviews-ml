// This is a "script" to iterate over all the negative rows in classified.csv
// We take 10 rows at a time and ask an LLM to "reword" the negative rows

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// SemanticKernel setup
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Debug).AddDebug());
builder.Services.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: "FILL_ME_OUT");

Kernel kernel = builder.Build();

const string FunctionDefinition =
	"""
	{{sentences}}

	Answer to the following questions using JSON syntax, using the format: { "comments": ["comment1", "comment2", ...] }
	You are expert software engineer that is particularly good at writing inclusive, well-written, thoughtful code reviews.
	Reword each comment in the JSON above to be more inclusive, constructive, and nice.
	@github is a placeholder for a GitHub username, include this in each comment as-is.
	#code and #url are placeholders for code and URLs, include this in each comment as-is.
	""";

var plugin = kernel.ImportPluginFromType<BatchedCommentPlugin>("Comments");
var promptTemplateFactory = new KernelPromptTemplateFactory();
var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
var renderedPrompt = await promptTemplate.RenderAsync(kernel);

// Create the "prompt" function
var function = kernel.CreateFunctionFromPrompt(FunctionDefinition, new OpenAIPromptExecutionSettings { MaxTokens = 1000 });

// CSV setup
string location = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
var path = Path.Combine(location, "..", "..", "..", "..", "..", "comments", "classified.csv");
var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
{
	HasHeaderRecord = true,
};
using var reader = new StreamReader(path);
using var inputCSV = new CsvReader(reader, configuration);

foreach (var comment in inputCSV.GetRecords<Comment>())
{
	if (comment.IsNegative == 1)
	{
		BatchedCommentPlugin.Comments.Add(comment);
	}

	// Ask the LLM
	if (BatchedCommentPlugin.Comments.Count == BatchedCommentPlugin.BatchSize)
	{
		var result = await kernel.InvokeAsync<string>(function);
		ArgumentNullException.ThrowIfNull(result);
		if (JsonDocument.Parse(result).RootElement.TryGetProperty("comments", out var prop))
		{
			var comments = prop.EnumerateArray().ToArray();
			Debug.Assert (comments.Length == BatchedCommentPlugin.Comments.Count, "Input/Output length should match!");
			for (int i = 0; i < BatchedCommentPlugin.Comments.Count; i++)
			{
				Console.WriteLine($"Original: {BatchedCommentPlugin.Comments[i].Text}");
				Console.WriteLine($"Improved: {comments[i]}");
			}
		}
		else
		{
			throw new InvalidOperationException("Could not parse JSON!");
		}

		BatchedCommentPlugin.Comments.Clear();
	}
}

Console.WriteLine("DONE!");
