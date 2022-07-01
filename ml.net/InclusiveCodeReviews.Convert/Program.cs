using InclusiveCodeReviews.ConsoleApp;
using InclusiveCodeReviews.Model;

ModelBuilder.CreateModel();

// Copy to the correct place in source control
var root = Path.GetFullPath("../../../..");
Copy(ConsumeModel.MLNetModelPath, Path.Combine(root, "InclusiveCodeReviews.Model", Path.GetFileName(ConsumeModel.MLNetModelPath)));
var onnx = Path.ChangeExtension(ConsumeModel.MLNetModelPath, ".onnx");
Copy(onnx, Path.Combine(root, "..", "onnxjs", "model.onnx"));

static void Copy (string source, string dest)
{
	Console.WriteLine($"Copying {source} to {dest}");
	File.Copy(source, dest, overwrite: true);
}
