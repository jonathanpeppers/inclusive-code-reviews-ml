# Instructions for AIs

Make all git diffs as small as possible, for easier code review by humans.

When fixing "heuristics" issues, see a past example here:

* https://github.com/jonathanpeppers/inclusive-code-reviews-ml/pull/177

To solve this, add a test case to using the verbatim text:

* `onnxjs/tests/test_cases.json`

Then add entries to the following file *not* using the verbatim text, but similar text:

* `comments/classified.csv`

That makes the test case pass, but make sure the text isn't exactly verbatim as the test.

When writing CSV files, verify the syntax is correct. Text containing commas should be enclosed in double quotes. For example:

```CSV
"Hello, World!",0,0.5
```
