using MLConsoleApp;

Console.WriteLine("Ctrl+C to exit...");

while (true)
{
    Console.WriteLine("Enter some text:");

    var sampleData = new MLSentiment.ModelInput()
    {
        Text = Console.ReadLine()!,
    };

    var result = MLSentiment.Predict(sampleData);
    var text = result.Prediction == 1 ? "negative" : "ok";
    Console.WriteLine($"Result: {text}, Confidence: {result.Score[(int)result.Prediction]}");
    Console.WriteLine();
}
