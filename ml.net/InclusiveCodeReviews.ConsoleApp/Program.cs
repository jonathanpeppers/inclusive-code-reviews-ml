using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using InclusiveCodeReviews.ConsoleApp;
using InclusiveCodeReviews.Model;

Console.WriteLine("Ctrl+C to exit...");
Console.WriteLine();

var githubHandleRegex = new Regex(@"\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))", RegexOptions.IgnoreCase);
var backtickRegex = new Regex("`+[^`]+`+", RegexOptions.IgnoreCase);
var urlRegex = new Regex(@"\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]", RegexOptions.IgnoreCase);
var punctuationRegex = new Regex("(\\.|!|\\?|;|:)+$", RegexOptions.IgnoreCase);

while (true)
{
	Console.WriteLine("Enter 1 if you want Line prediction or 2 if you want File prediction:");

	var lineOrFile = Console.ReadLine();
	if (string.IsNullOrEmpty(lineOrFile))
		continue;
	if (lineOrFile == "1")
	{
		Console.WriteLine("its 1 for Line Prediction");
		Console.WriteLine("Enter text");
		var text = Console.ReadLine();
		if (string.IsNullOrEmpty(text))
			continue;
		var result = GetLinePrediction(text);
		Console.WriteLine($"IsNegative: {result.Prediction}, Confidence: {result.Score[result.Prediction == "1" ? 1 : 0]}");
		Console.WriteLine();
	}
	else if (lineOrFile == "2")
	{
		Console.WriteLine("Enter File name not path"); // e.g. sample.txt, good_sampe.txt in comments/ folder
		var filePath = Console.ReadLine();
		if (string.IsNullOrEmpty(filePath))
			continue;
		var result = GetFilePrediction(filePath);
		Console.WriteLine($" if score is towards 0 is positive, if score is towards 1 is negative. File Score is : {result}");
	}
	else
	{
		Console.WriteLine("Not supported Try again");
	}
}

string GetFilePrediction(string fileName)
{
	var filePrediction = 0;
	var numLines = 0;
	string sampleFilePath = Path.Combine(Path.GetDirectoryName(typeof(ModelBuilder).Assembly.Location), "..", "..", "..", "..", "..", "comments", fileName);
	IEnumerable<string> lines = File.ReadLines(@sampleFilePath);
	foreach (var line in lines)
	{
		if (string.IsNullOrEmpty(line))
			continue;
		var lineOut = GetLinePrediction(line);
		filePrediction = filePrediction + int.Parse(lineOut.Prediction); // cumulative line prediction
		numLines++;
	}
	if (numLines == 0)
		return null; // empty file
	decimal res = Decimal.Divide(filePrediction, numLines); // avg of all line predictions in file to get final score, we can modify scale as needed

	return res.ToString();
}

ModelOutput GetLinePrediction(string text)
{
	if (string.IsNullOrEmpty(text))
		return null;

	var replaced = githubHandleRegex.Replace(text, "@github");
	replaced = backtickRegex.Replace(replaced, "#code");
	replaced = urlRegex.Replace(replaced, "#code");
	replaced = punctuationRegex.Replace(replaced, "#code");

	var sampleData = new ModelInput()
	{
		Text = replaced,
	};

	var result = ConsumeModel.Predict(sampleData);
	return result;
}