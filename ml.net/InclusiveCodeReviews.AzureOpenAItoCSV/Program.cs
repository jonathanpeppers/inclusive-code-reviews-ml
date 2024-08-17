using OpenAI.Chat;
using Azure.AI.OpenAI;
using System.ClientModel;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

AzureOpenAIClient client = new(new Uri("https://icr-gpt-generate-content-2408.openai.azure.com/"), new ApiKeyCredential(""));
ChatClient chat = client.GetChatClient("icr-generation-gpt-4o");

//open ai does not tolerate negative
var tones = string.Join(", ", new[] { "positive", "constructive", "neutral" });

string prompt = $@"Answer to the following questions using JSON syntax, using the format: {{ ""comments"": [""comment1"", ""comment2"", ...] }}
The produced JSON should be a single array of strings under the single key 'comments'. Do not include the tone in the comments.
You are an expert software engineer that is particularly good at writing inclusive, well-written, thoughtful code reviews. 
Give 10 examples of feedback in each of the following tones: {tones} you would give to a junior engineer on their code review.";

ChatCompletion response = chat.CompleteChat(prompt);
// for debugging
// Console.WriteLine($"openAI output: {response}");
var match = Regex.Match(response.ToString(), @"{.*}", RegexOptions.Singleline);

if (!match.Success)
	throw new Exception("No JSON found in completion");

var data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(match.Value);
var comments = data["comments"];
var modifiedComments = comments.Select(comment => comment + ",0,0.5").ToList();

string location = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
var path = Path.Combine(location, "..", "..", "..", "..", "..", "comments", "classified.csv");

// write generated comments to CSV sorted in alphabetical order
// ensure the first line remains unchanged as it contains the column names
var allLines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
string firstRow = allLines[0];
List<string> otherRows = allLines.Skip(1).ToList();
otherRows.AddRange(modifiedComments);
otherRows = otherRows.OrderBy(comment => comment).ToList();
using (StreamWriter writer = new StreamWriter(path))
{
	writer.WriteLine(firstRow);
	foreach (var comment in otherRows)
	{
		writer.WriteLine(comment);
	}
}
