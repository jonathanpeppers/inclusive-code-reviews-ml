using System;
using InclusiveCodeReviews.Model;

Console.WriteLine("Ctrl+C to exit...");
Console.WriteLine();

while (true)
{
	Console.WriteLine("Enter some text:");

	var sampleData = new ModelInput()
	{
		Text = Console.ReadLine()!,
	};

	var result = ConsumeModel.Predict(sampleData);
	Console.WriteLine($"IsNegative: {result.Prediction}, Confidence: {result.Score[result.Prediction == "1" ? 1 : 0]}");
	Console.WriteLine();
}