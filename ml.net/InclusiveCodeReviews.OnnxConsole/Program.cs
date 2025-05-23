using InclusiveCodeReviews.OnnxConsole;
using System;

namespace InclusiveCodeReviews.OnnxConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Inclusive Code Reviews - ONNX Runtime Console App");
            Console.WriteLine("===============================================");
            
            try
            {
                // Load the ONNX model
                var model = new OnnxModel();
                
                if (args.Length > 0)
                {
                    // Process command line input
                    var text = string.Join(" ", args);
                    ProcessText(model, text);
                }
                else
                {
                    // Interactive mode
                    Console.WriteLine("\nEnter text to classify (empty line to quit):");
                    
                    while (true)
                    {
                        Console.Write("\n> ");
                        var input = Console.ReadLine();
                        
                        if (string.IsNullOrWhiteSpace(input))
                            break;
                            
                        ProcessText(model, input);
                    }
                }
                
                Console.WriteLine("\nThank you for using the Inclusive Code Reviews ONNX Console App!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        static void ProcessText(OnnxModel model, string text)
        {
            Console.WriteLine($"\nOriginal text: {text}");
            Console.WriteLine($"Preprocessed: {model.PreprocessText(text)}");
            
            var (prediction, confidence) = model.Predict(text);
            
            Console.WriteLine($"Prediction: {prediction} (IsNegative: {(prediction == "1" ? "Yes" : "No")})");
            Console.WriteLine($"Confidence: {confidence:P2}");
            
            if (prediction == "1")
            {
                Console.WriteLine("This text may contain non-inclusive language.");
            }
            else
            {
                Console.WriteLine("This text appears to be inclusive.");
            }
        }
    }
}
