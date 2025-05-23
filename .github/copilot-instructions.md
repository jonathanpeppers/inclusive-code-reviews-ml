# Instructions for AIs

See [README.md](../README.md) for information about this repository.

Make all git diffs as small as possible, for easier code review by humans.

To work on this repository, you will need to first build the model:

```bash
cd ml.net/InclusiveCodeReviews.Convert
dotnet run
```

This will create `bin/model.onnx` that is used by the `onnxjs` test project.

To run tests, you can run:

```bash
cd onnxjs
npm install
npm test
```

When fixing "heuristics" issues, see a past example here:

* https://github.com/jonathanpeppers/inclusive-code-reviews-ml/pull/177

To solve issues like this:

1. Add a test case to using the verbatim text in `onnxjs/tests/test_cases.json`. Add a few test cases with *similar* text.

2. Run the tests, and verify the test fails. If the test does not fail, add a few more similar cases.

3. Then add entries to the following file *not* using the verbatim text, but similar text: `comments/classified.csv`

When writing CSV files, verify the syntax is correct. Text containing commas should be enclosed in double quotes. For example:

```CSV
"Hello, World!",0,0.5
```

4. Run the tests again, and verify the test passes.
