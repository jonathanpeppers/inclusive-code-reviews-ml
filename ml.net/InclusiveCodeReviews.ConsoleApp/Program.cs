using System;
using System.Text.RegularExpressions;
using InclusiveCodeReviews.Model;

Console.WriteLine("Ctrl+C to exit...");
Console.WriteLine();

var githubHandleRegex = new Regex(@"\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))", RegexOptions.IgnoreCase);
var backtickRegex = new Regex("`+[^`]+`+", RegexOptions.IgnoreCase);
var urlRegex = new Regex(@"\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]", RegexOptions.IgnoreCase);
var punctuationRegex = new Regex("(\\.|!|\\?|;|:)+$", RegexOptions.IgnoreCase);

while (true)
{
	Console.WriteLine("Enter some text:");

	var text = Console.ReadLine();
	if (string.IsNullOrEmpty(text))
		continue;

	var replaced = githubHandleRegex.Replace(text, "@github");
	replaced = backtickRegex.Replace(replaced, "#code");
	replaced = urlRegex.Replace(replaced, "#code");
	replaced = punctuationRegex.Replace(replaced, "#code");

	var sampleData = new ModelInput()
	{
		Text = replaced,
	};

	var result = ConsumeModel.Predict(sampleData);
	Console.WriteLine($"IsNegative: {result.Prediction}, Confidence: {result.Score[result.Prediction == "1" ? 1 : 0]}");
	Console.WriteLine();
}