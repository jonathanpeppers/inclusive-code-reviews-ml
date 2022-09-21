//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using InclusiveCodeReviews.Model;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;

namespace InclusiveCodeReviews.ConsoleApp
{
	public static class ModelBuilder
	{
		private static string TRAIN_DATA_FILEPATH = Path.Combine(Path.GetDirectoryName(typeof(ModelBuilder).Assembly.Location), "..", "..", "..", "..", "..", "comments", "classified.csv");
		private static string MODEL_FILE = ConsumeModel.MLNetModelPath;

		// Create MLContext to be shared across the model creation workflow objects 
		// Set a random seed for repeatable/deterministic results across multiple trainings.
		private static MLContext mlContext = new MLContext(seed: 209348038);

		public static void CreateModel()
		{
			// Load Data
			IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
											path: TRAIN_DATA_FILEPATH,
											hasHeader: true,
											separatorChar: ',',
											allowQuoting: true,
											allowSparse: false);

			// Build training pipeline
			IEstimator<ITransformer> trainingPipeline = BuildTrainingPipeline(mlContext);

			// Train Model
			ITransformer mlModel = TrainModel(mlContext, trainingDataView, trainingPipeline);

			// Evaluate quality of Model
			Evaluate(mlContext, trainingDataView, trainingPipeline);

			// Save model
			SaveModel(mlContext, trainingDataView, mlModel, MODEL_FILE, trainingDataView.Schema);
		}

		public static IEstimator<ITransformer> BuildTrainingPipeline(MLContext mlContext)
		{
			// Data process configuration with pipeline data transformations 
			var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("isnegative", "isnegative")
									  .Append(mlContext.Transforms.Text.FeaturizeText("text_tf", new TextFeaturizingEstimator.Options
									  {
										  CaseMode = TextNormalizingEstimator.CaseMode.Lower,
										  //NOTE: not exportable to ONNX
										  //KeepNumbers = false,
										  //KeepPunctuations = false,
										  KeepDiacritics = true,
									  }, "text"))
									  .Append(mlContext.Transforms.CopyColumns("Features", "text_tf"))
									  .Append(mlContext.Transforms.NormalizeMinMax("Features", "Features"))
									  .AppendCacheCheckpoint(mlContext);
			// Set the training algorithm 
			var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(new SdcaMaximumEntropyMulticlassTrainer.Options() { L2Regularization = 0.001f, L1Regularization = 0f, ConvergenceTolerance = 0.001f, MaximumNumberOfIterations = 10, Shuffle = false, BiasLearningRate = 0f, LabelColumnName = "isnegative", FeatureColumnName = "Features", ExampleWeightColumnName = "importance" })
						  .Append(mlContext.Transforms.Conversion.MapValueToKey("importance", "importance"))
						  .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

			var trainingPipeline = dataProcessPipeline.Append(trainer);

			return trainingPipeline;
		}

		public static ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
		{
			Console.WriteLine("=============== Training  model ===============");

			ITransformer model = trainingPipeline.Fit(trainingDataView);

			Console.WriteLine("=============== End of training process ===============");
			return model;
		}

		private static void Evaluate(MLContext mlContext, IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
		{
			// Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
			// in order to evaluate and get the model's accuracy metrics
			Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
			var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(trainingDataView, trainingPipeline, numberOfFolds: 5, labelColumnName: "isnegative");
			PrintMulticlassClassificationFoldsAverageMetrics(crossValidationResults);
		}

		private static void SaveModel(MLContext mlContext, IDataView dataView, ITransformer mlModel, string modelRelativePath, DataViewSchema modelInputSchema)
		{
			// Save/persist the trained model to a .ZIP file
			Console.WriteLine($"=============== Saving the model  ===============");
			var outputDirectory = Path.GetFullPath(Path.GetDirectoryName(modelRelativePath));
			Directory.CreateDirectory(outputDirectory);
			mlContext.Model.Save(mlModel, modelInputSchema, modelRelativePath);
			var onnxFile = Path.Combine(outputDirectory, "model.onnx");
			using var fileStream = File.Create(onnxFile);
			mlContext.Model.ConvertToOnnx(mlModel, dataView, fileStream);
			Console.WriteLine("The model is saved to {0}", Path.GetFullPath(modelRelativePath));
			Console.WriteLine("The model is saved to {0}", Path.GetFullPath(onnxFile));
		}

		public static void PrintMulticlassClassificationMetrics(MulticlassClassificationMetrics metrics)
		{
			Console.WriteLine($"************************************************************");
			Console.WriteLine($"*    Metrics for multi-class classification model   ");
			Console.WriteLine($"*-----------------------------------------------------------");
			Console.WriteLine($"    MacroAccuracy = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
			Console.WriteLine($"    MicroAccuracy = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
			Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
			for (int i = 0; i < metrics.PerClassLogLoss.Count; i++)
			{
				Console.WriteLine($"    LogLoss for class {i + 1} = {metrics.PerClassLogLoss[i]:0.####}, the closer to 0, the better");
			}
			Console.WriteLine($"************************************************************");
		}

		public static void PrintMulticlassClassificationFoldsAverageMetrics(IEnumerable<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> crossValResults)
		{
			var metricsInMultipleFolds = crossValResults.Select(r => r.Metrics);

			var microAccuracyValues = metricsInMultipleFolds.Select(m => m.MicroAccuracy);
			var microAccuracyAverage = microAccuracyValues.Average();
			var microAccuraciesStdDeviation = CalculateStandardDeviation(microAccuracyValues);
			var microAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(microAccuracyValues);

			var macroAccuracyValues = metricsInMultipleFolds.Select(m => m.MacroAccuracy);
			var macroAccuracyAverage = macroAccuracyValues.Average();
			var macroAccuraciesStdDeviation = CalculateStandardDeviation(macroAccuracyValues);
			var macroAccuraciesConfidenceInterval95 = CalculateConfidenceInterval95(macroAccuracyValues);

			var logLossValues = metricsInMultipleFolds.Select(m => m.LogLoss);
			var logLossAverage = logLossValues.Average();
			var logLossStdDeviation = CalculateStandardDeviation(logLossValues);
			var logLossConfidenceInterval95 = CalculateConfidenceInterval95(logLossValues);

			var logLossReductionValues = metricsInMultipleFolds.Select(m => m.LogLossReduction);
			var logLossReductionAverage = logLossReductionValues.Average();
			var logLossReductionStdDeviation = CalculateStandardDeviation(logLossReductionValues);
			var logLossReductionConfidenceInterval95 = CalculateConfidenceInterval95(logLossReductionValues);

			if (metricsInMultipleFolds.Any(m => m.ConfusionMatrix.PerClassPrecision.Count != 2))
			{
				throw new Exception("We should only have two classes in this model!");
			}

			var class0Values = metricsInMultipleFolds.Select(m => m.ConfusionMatrix.PerClassPrecision[0]);
			var class0Average = class0Values.Average();
			var class0StdDev = CalculateStandardDeviation(class0Values);
			var class0Confidence = CalculateConfidenceInterval95(class0Values);
			var class1Values = metricsInMultipleFolds.Select(m => m.ConfusionMatrix.PerClassPrecision[1]);
			var class1Average = class1Values.Average();
			var class1StdDev = CalculateStandardDeviation(class1Values);
			var class1Confidence = CalculateConfidenceInterval95(class1Values);

			Console.WriteLine($"*************************************************************************************************************");
			Console.WriteLine($"*       Metrics for Multi-class Classification model      ");
			Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
			Console.WriteLine($"*       Average MicroAccuracy:     {microAccuracyAverage:0.###}  - Standard deviation: ({microAccuraciesStdDeviation:#.###})  - Confidence Interval 95%: ({microAccuraciesConfidenceInterval95:#.###})");
			Console.WriteLine($"*       Average MacroAccuracy:     {macroAccuracyAverage:0.###}  - Standard deviation: ({macroAccuraciesStdDeviation:#.###})  - Confidence Interval 95%: ({macroAccuraciesConfidenceInterval95:#.###})");
			Console.WriteLine($"*       Average LogLoss:           {logLossAverage:#.###}  - Standard deviation: ({logLossStdDeviation:#.###})  - Confidence Interval 95%: ({logLossConfidenceInterval95:#.###})");
			Console.WriteLine($"*       Average LogLossReduction:  {logLossReductionAverage:#.###}  - Standard deviation: ({logLossReductionStdDeviation:#.###})  - Confidence Interval 95%: ({logLossReductionConfidenceInterval95:#.###})");
			Console.WriteLine($"*       Average Class 0 Precision: {class0Average:0.###}  - Standard deviation: ({class0StdDev:#.###})  - Confidence Interval 95%: ({class0Confidence:#.###})");
			Console.WriteLine($"*       Average Class 1 Precision: {class1Average:0.###}  - Standard deviation: ({class1StdDev:#.###})  - Confidence Interval 95%: ({class1Confidence:#.###})");
			Console.WriteLine($"*************************************************************************************************************");

			// Fail if detecting IsNegative=1 is less than a threshold
			const double threshold = 0.65;
			if (class1Average < threshold)
			{
				throw new Exception($"Class 1 Precision must be higher than {threshold}!");
			}
		}

		public static double CalculateStandardDeviation(IEnumerable<double> values)
		{
			double average = values.Average();
			double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
			double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (values.Count() - 1));
			return standardDeviation;
		}

		public static double CalculateConfidenceInterval95(IEnumerable<double> values)
		{
			double confidenceInterval95 = 1.96 * CalculateStandardDeviation(values) / Math.Sqrt((values.Count() - 1));
			return confidenceInterval95;
		}
	}
}
