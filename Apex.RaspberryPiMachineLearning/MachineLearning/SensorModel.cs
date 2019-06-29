using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Apex.RaspberryPiMachineLearning.Helpers;
using Microsoft.ML.AutoML;
using static Apex.RaspberryPiMachineLearning.Helpers.ConsoleHelpers;

namespace RaspberryPiMachineLearning.MachineLearning
{
    public static class SensorModel
    {
        private static ITransformer Model { get; set;}
        private static MLContext Context { get; set; } = new MLContext(seed: 1);

        private const string Temperature = nameof(SourceData.Temperature);
        private const string Luminosity = nameof(SourceData.Luminosity);
        private const string Infrared = nameof(SourceData.Infrared);
        //private const string Distance = nameof(SourceData.Distance);
        private const string Label = nameof(SourceData.Label);

        private const string Score = nameof(SourcePrediction.Score);
        private const string PredictedLabel = nameof(SourcePrediction.PredictedLabel);
        private const string Features = "Features";
        
        private const string trainingCsv = "MachineLearning/training.csv";
        private const string testingCsv = "MachineLearning/testing.csv";
        private const string modelPath = "MachineLearning/Model.zip";

        private const int ExperimentTime = 30;

        public static void AutoTrain()
        {
            // STEP 1: data loading
            IDataView trainingDataView = LoadDataFromCsv(trainingCsv);
            IDataView testingDataView = LoadDataFromCsv(testingCsv);

            // STEP 2: run an AutoML multiclass classification experiment
            WriteLineColor($"{Environment.NewLine}AutoML multiclass classification experiment for {ExperimentTime} seconds...", ConsoleColor.Yellow);
            var progressHandler = new MulticlassExperimentProgressHandler();
            ExperimentResult<MulticlassClassificationMetrics> experimentResult = Context.Auto()
                .CreateMulticlassClassificationExperiment(ExperimentTime)
                .Execute(trainingDataView, Label, progressHandler: progressHandler);

            // STEP 3: evaluate the model and print metrics
            RunDetail<MulticlassClassificationMetrics> bestRun = experimentResult.BestRun;
            WriteLineColor($"{Environment.NewLine}Top Trainer (by accuracy)", ConsoleColor.Yellow);
            PrintTopModels(experimentResult);
            WriteLineColor($"{Environment.NewLine}TRAINING USING: {bestRun.TrainerName}", ConsoleColor.Cyan);
            Model = bestRun.Model;
            var predictions = Model.Transform(testingDataView);
            var metrics = Context.MulticlassClassification.Evaluate(data: predictions, labelColumnName: Label, scoreColumnName: Score, predictedLabelColumnName: PredictedLabel);
            PrintMultiClassClassificationMetrics(bestRun.TrainerName, metrics);

            // STEP 4: save the model
            Context.Model.Save(Model, trainingDataView.Schema, modelPath);
        }

        public static void Train()
        {
            // STEP 1: data loading
            IDataView trainingDataView = LoadDataFromCsv(trainingCsv);
            IDataView testingDataView = LoadDataFromCsv(testingCsv);

            // STEP 2a: data process configuration with pipeline data transformations
            var dataProcessPipeline = Context.Transforms.Conversion.MapValueToKey(Label, Label)
                .Append(Context.Transforms.Concatenate("Features", Temperature, Luminosity, Infrared))
                .Append(Context.Transforms.NormalizeMinMax("Features", "Features"))
                .AppendCacheCheckpoint(Context);

            // STEP 2b: set the training algorithm
            IEstimator<ITransformer> trainer = Context.MulticlassClassification.Trainers
                ////.LightGbm(labelColumnName: Label, Features);
                ////.SdcaNonCalibrated(Label, Features);
                ////.SdcaMaximumEntropy(Label, Features);
                ////.LinearSvm(Label, Features);
                .OneVersusAll(Context.BinaryClassification.Trainers.AveragedPerceptron(Label, Features));
            ////var trainerName = "LightGbm";
            ////var trainerName = "SdcaNonCalibrated";
            ////var trainerName = "SdcaMaximumEntropy";
            ////var trainerName = "LinearSvm";
            var trainerName = "AveragedPerceptron (OneVersusAll)";
            WriteLineColor($"{Environment.NewLine}TRAINING USING: {trainerName}", ConsoleColor.Cyan);

            // STEP 2c: train the model fitting to the DataSet
            var trainingPipeline = dataProcessPipeline
                .Append(trainer)
                .Append(Context.Transforms.Conversion.MapKeyToValue(PredictedLabel, PredictedLabel));
            Model = trainingPipeline.Fit(trainingDataView);

            // STEP 3a: evaluate the model and print metrics
            var predictions = Model.Transform(testingDataView);
            var metrics = Context.MulticlassClassification.Evaluate(predictions, Label, Score, PredictedLabel);
            PrintMultiClassClassificationMetrics(trainerName, metrics);

            // STEP 4: save the model
            Context.Model.Save(Model, trainingDataView.Schema, modelPath); // Saves the model we trained to a zip file.
        }

        public static string Predict(float luminosity, float temperature, float infrared)
        {
            var sample = new SourceData {
                Luminosity = luminosity,
                Temperature = temperature,
                Infrared = infrared
            };

            if (Model != null)
            {
                var predictor = Context.Model.CreatePredictionEngine<SourceData, SourcePrediction>(Model);
                var predicted = predictor.Predict(sample);

                return predicted.PredictedLabel;
            }

            return string.Empty;
        }

        private static IDataView LoadDataFromCsv(string sampleCsv)
        {
            var textLoader = Context.Data.CreateTextLoader(
                columns: new TextLoader.Column[]
                {
                    new TextLoader.Column(Temperature, DataKind.Single, 0),
                    new TextLoader.Column(Luminosity, DataKind.Single, 1),
                    new TextLoader.Column(Infrared, DataKind.Single, 2),
                    new TextLoader.Column(Label, DataKind.String, 4)
                },
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true,
                allowSparse: false
            );
            var trainingDataView = textLoader.Load(sampleCsv);

            return trainingDataView;
        }
    }
}
