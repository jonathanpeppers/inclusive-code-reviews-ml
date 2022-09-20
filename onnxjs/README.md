# JavaScript ONNX Example

Before running the tests, you need to have produced `bin\model.onnx`
by running:

```bash
dotnet run --project ml.net/InclusiveCodeReviews.Convert/InclusiveCodeReviews.Convert.csproj
...
=============== Saving the model  ===============
The model is saved to inclusive-code-reviews-ml/bin/MLModel.zip
The model is saved to inclusive-code-reviews-ml/bin/model.onnx
```

`npm install` then `npm test` to run the tests. You can also open this
folder in VS Code.

If you get errors, it's worth trying to update Node.js to latest at:

https://nodejs.org/en/download/current/

We found there might be issues running Node.js older than 17.x.

Links:

* https://onnxruntime.ai/docs/get-started/with-javascript.html
* https://github.com/microsoft/onnxruntime-inference-examples
* https://github.com/microsoft/onnxruntime

## Viewing an `.onnx` file

Try Netron: https://github.com/lutzroeder/netron
