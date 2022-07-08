using System;
using System.Text.RegularExpressions;
using InclusiveCodeReviews.Model;

Console.WriteLine("Ctrl+C to exit...");
Console.WriteLine();

var githubHandleRegex = new Regex(@"\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

while (true)
{
	Console.WriteLine("Enter some text:");

	var text = Console.ReadLine();
	if (string.IsNullOrEmpty(text))
		continue;

	var sampleData = new ModelInput()
	{
		Text = githubHandleRegex.Replace(text, "@github"),
	};

	var result = ConsumeModel.Predict(sampleData);
	Console.WriteLine($"IsNegative: {result.Prediction}, Confidence: {result.Score[result.Prediction == "1" ? 1 : 0]}");
	Console.WriteLine();
}