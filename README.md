# Inclusive Code Reviews: Machine Learning

Machine learning for code reviews! This is an MIT-licensed companion
to our [Inclusive Code Comments browser extension][browser].

The goal of this project is to produce a machine learning model for
classifying sentences for code reviews.

Some examples:

* "Looks good to me. Ship it!" -> OK!
* "There are test failures" -> OK!
* "You are a failure" -> BAD!
* "This code stinks" -> BAD!

We have the model in two formats, that is free to use in other
projects:

* `ml.net\InclusiveCodeReviews.Model\MLModel.zip` - model in ML.NET format
* `onnxjs\model.onnx` - model in ONNX format

See the `InclusiveCodeReviews.ConsoleApp` for a C# example, general
usage:

```csharp
var results = ConsumeModel.Predict(new ModelInput
{
    Text = "Your text here."
});
var result = result.Prediction;
var score = result.Score[result.Prediction == "1" ? 1 : 0];
```

See `onnxjs\tests\onnx.ts` for a TypeScript/JavaScript example,
general usage:

```typescript
const session = await ort.InferenceSession.create('./model.onnx');
const results = await session.run({
    text: new ort.Tensor(["Your text here."], [1,1]),
    isnegative: new ort.Tensor([''], [1,1]),
});
const result = results['PredictedLabel.output'].data[0];
const score = results['Score.output'].data[Number(result)];
```

## Repo layout / structure

Folder structure of this repo:

* `comments`: several `.csv` files of code review comments
* `Maui`: desktop app for classifying data, see [MLTrainer app](#mltrainer-app)
* `ml.net`: contains C# projects related to ML.NET usage, creating `.zip` or `.onnx` files
* `onnxjs`: JS test project for the `.onnx` model

[browser]: https://github.com/jonathanpeppers/inclusive-code-comments

## MLTrainer app

If you're here to update [`comments/classified.csv`](comments/classified.csv),
we have a CI archive of the app, so you don't have to build it from
source.

Find a commit on GitHub, and click a status for either Windows or Mac:

![GitHub Status](docs\MLTrainer-GH-Status.png)

Click on `Summary`:

![GitHub Summary](docs\MLTrainer-Summary.png)

Scroll to the bottom, and pick either a Windows or Mac build:

![GitHub Artifacts](docs\MLTrainer-Artifacts.png)

The Windows build contains an `MLTrainer.exe` inside, you can unzip
this somewhere and run it.

The Mac build contains an `MLTrainer-1.0.pkg` inside. You can simply
install it and run `/Applications/MLTrainer.app` afterward.

Note that you may need to bypass various signing prompts, as this app
is not digitally signed. Mac you will need to go to `System
Preferences > Security & Privacy` to run an unsigned `.pkg` installer.

## Notes about Inputs

The model is meant to be passed individual sentences. If you need to
classify paragraphs of text, it is recommended to split the text into
sentences and match each against the model.

The model is trained with all GitHub handles removed and replaced with
`@github`, you should apply this same replacement with your own input
text.

TODO: standardize replacement of punctuation.

## `mlnet` .NET Global Tool

Install with:

```dotnetcli
dotnet tool install --global mlnet
```

And you can train, such as:

```dotnetcli
mlnet classification --dataset comments/classified.csv --label-col 1 --has-header true --train-time 10
```

This outputs a folder named `SampleClassification` in the current
directory. It's not exactly the same output the ML.NET Model Builder
outputs, but this should also work on a Mac.

See the [ML.NET docs][mlnet] for more info.

[mlnet]: https://docs.microsoft.com/dotnet/machine-learning/automate-training-with-cli

## Generate Projects in `ml.net` folder

Run `train.ps1`.

Note there are additional manual edits we would lose if running this
again. You might discard some of the changes, or compare the diff.

## Updating the Model

Open `ml.net\InclusiveCodeReviews.sln` in VS, and run `InclusiveCodeReviews.Convert` project.

This will update `InclusiveCodeReviews.Model\MLModel.zip` and `onnxjs\model.onnx` in-place.

To test `model.onnx`, run `npm test` in the `onnxjs` directory.
