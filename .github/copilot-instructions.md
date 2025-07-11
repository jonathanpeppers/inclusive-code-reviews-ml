# Instructions for AIs

This repository trains and serves ML models for classifying code review comments as inclusive/positive (0) or potentially problematic (1).

## Architecture Overview

The project has a dual-pipeline architecture:
- **Training Pipeline**: `.NET ML.NET` (ml.net/) → produces both MLModel.zip and model.onnx
- **Inference Pipeline**: `ONNX.js` (onnxjs/) → consumes model.onnx for cross-platform inference
- **Data Management**: CSV files (comments/) containing labeled training data
- **Classification Tool**: MAUI desktop app (Maui/) for manual data labeling

Key data flow: `comments/classified.csv` → ML.NET training → `bin/model.onnx` → ONNX.js tests

## Critical Development Workflow

To work on this repository, you will need to first build the model.

**Always build model before testing:**
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

**For model improvements (heuristics fixes):**
1. Add failing test cases to `onnxjs/tests/test_cases.json` using *verbatim* problematic text
2. Add training data to `comments/classified.csv` using *similar but not identical* text 
3. Rebuild model, verify tests pass

## Text Processing Conventions

The model expects preprocessed text following `Maui/MLTrainer/TextProcessor.cs` patterns:
- GitHub handles: `@username` → `@github`
- Code blocks: `` `code` `` → `#code`
- URLs: `https://...` → `#url`
- Remove trailing punctuation: `Hello!` → `Hello`
- Split paragraphs into individual sentences

Both training data and inference must use identical preprocessing.

## Data Format Requirements

**CSV structure** (`comments/classified.csv`):
```csv
text,isnegative,importance
"Quoted text with, commas",0,0.5
```
- Column 0: preprocessed text (quoted if contains commas)
- Column 1: label (0=good, 1=problematic) 
- Column 2: importance weight (typically 0.5)

**Test cases** (`onnxjs/tests/test_cases.json`):
```json
{"text": "raw unprocessed text", "isnegative": "0"}
```

## Project-Specific Rules

- **Never** add TypeScript test code - only add cases to `test_cases.json`
- **Never** pipe text to `dotnet run` (causes hanging)
- Use *verbatim* text in test cases, *similar* text in training data
- Keep git diffs minimal for easier human review
- Model outputs are at `bin/MLModel.zip` (ML.NET) and `bin/model.onnx` (ONNX)
