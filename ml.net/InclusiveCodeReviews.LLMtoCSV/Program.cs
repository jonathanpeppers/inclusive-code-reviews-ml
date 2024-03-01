// This is a "script" to iterate over all the negative rows in classified.csv
// We take 10 rows at a time and ask an LLM to "reword" the negative rows

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// SemanticKernel setup
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Debug).AddDebug());

Kernel kernel = builder.Build();

const string FunctionDefinition =
	"""
	{{sentences}}

	Answer to the following questions using JSON syntax, including the data used.
	You are expert software engineer that is particularly good at writing inclusive, well-written, thoughtful code reviews.
	Reword each sentence above to be more inclusive, constructive, and nice.
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
		FunctionResult result = await kernel.InvokeAsync(function);

		BatchedCommentPlugin.Comments.Clear();
	}
}

Console.WriteLine("DONE!");
