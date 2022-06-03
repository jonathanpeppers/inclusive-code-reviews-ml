# inclusive-code-reviews-ml

Machine learning for code reviews!

`ml.net` folder is an example using ML.NET Model Builder in Visual Studio.

`tfjs` folder is TensorFlowJS setup to run tests. (Not really working)

## ML.NET Model Builder

To retrain the model, install the `ML.NET Model Builder` in Visual Studio:

![Visual Studio Installer](docs/vsinstaller.png)

TBD if you can do this command-line or on a Mac.

## `mlnet` .NET Global Tool

Install with:

```dotnetcli
dotnet tool install --global mlnet
```

And you can train, such as:

```dotnetcli
mlnet classification --dataset .\MLConsoleApp\test.csv --label-col 1 --has-header true --train-time 10
```

This outputs a folder named `SampleClassification` in the current
directory. It's not exactly the same output the ML.NET Model Builder
outputs, but this should also work on a Mac.

See the [ML.NET docs][mlnet] for more info.

[mlnet]: https://docs.microsoft.com/dotnet/machine-learning/automate-training-with-cli

## Code Review Examples

These are just some links to "heated" conversations:

* https://lkml.iu.edu/hypermail/linux/kernel/1510.3/02866.html
