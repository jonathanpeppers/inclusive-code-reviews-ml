using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace InclusiveCodeReviews.OnnxConsole
{
    public class OnnxModel
    {
        private readonly InferenceSession _session;

        // Regex patterns for text preprocessing
        private static readonly Regex GithubHandleRegex = new Regex(@"\B@([a-z0-9](?:-(?=[a-z0-9])|[a-z0-9]){0,38}(?<=[a-z0-9]))", RegexOptions.IgnoreCase);
        private static readonly Regex BacktickRegex = new Regex(@"`+[^`]+`+", RegexOptions.IgnoreCase);
        private static readonly Regex UrlRegex = new Regex(@"\b(https?|ftp|file):\/\/[\-A-Za-z0-9+&@#\/%?=~_|!:,.;]*[\-A-Za-z0-9+&@#\/%=~_|]", RegexOptions.IgnoreCase);
        private static readonly Regex PunctuationRegex = new Regex(@"(\.|!|\?|;|:)+$");

        public static string ModelPath => Path.Combine(
            Path.GetDirectoryName(typeof(OnnxModel).Assembly.Location) ?? "",
            "..", "..", "..", "..", "..", "bin", "model.onnx");

        public OnnxModel(string? modelPath = null)
        {
            var path = modelPath ?? ModelPath;
            Console.WriteLine($"Loading ONNX model from: {Path.GetFullPath(path)}");
            _session = new InferenceSession(path);
        }

        public (string PredictedLabel, float Score) Predict(string text)
        {
            // Preprocess the text similar to the JS implementation
            var preprocessedText = PreprocessText(text);
            
            // Create input tensor
            var inputTextTensor = new DenseTensor<string>(new[] { preprocessedText }, new[] { 1, 1 });
            var inputIsNegativeTensor = new DenseTensor<string>(new[] { "" }, new[] { 1, 1 });
            var inputImportanceTensor = new DenseTensor<float>(new[] { 0.5f }, new[] { 1, 1 });

            // Setup inputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("text", inputTextTensor),
                NamedOnnxValue.CreateFromTensor("isnegative", inputIsNegativeTensor),
                NamedOnnxValue.CreateFromTensor("importance", inputImportanceTensor)
            };

            // Run inference
            using var results = _session.Run(inputs);

            // Process the results
            var predictedLabelValue = results.First(x => x.Name == "PredictedLabel.output").Value as Tensor<string>;
            var scoresValue = results.First(x => x.Name == "Score.output").Value as Tensor<float>;
            
            if (predictedLabelValue == null || scoresValue == null)
                throw new InvalidOperationException("Could not retrieve prediction results from the model");
                
            var predictedLabel = predictedLabelValue.GetValue(0);
            var scores = scoresValue.ToArray();
            var score = scores[int.Parse(predictedLabel)];

            return (predictedLabel, score);
        }

        public string PreprocessText(string text)
        {
            var github_replaced = GithubHandleRegex.Replace(text, "@github");
            var backtick_replaced = BacktickRegex.Replace(github_replaced, "#code");
            var urls_replaced = UrlRegex.Replace(backtick_replaced, "#url");
            var punctuation_replaced = PunctuationRegex.Replace(urls_replaced, "");
            return punctuation_replaced.Trim();
        }
    }
}