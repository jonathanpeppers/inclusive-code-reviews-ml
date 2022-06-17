using Microsoft.ML;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: convert-model foo.zip foo.onnx");
    return -1;
}

using var inStream = File.OpenRead(args[0]);
var context = new MLContext();
var transformer = context.Model.Load(inStream, out var schema);

using var outStream = File.Create(args[1]);
context.Model.ConvertToOnnx(transformer, null /* what do I put here???? */, outStream);
return 0;
