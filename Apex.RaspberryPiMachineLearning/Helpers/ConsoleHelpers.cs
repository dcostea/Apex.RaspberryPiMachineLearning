using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Apex.RaspberryPiMachineLearning.Helpers
{
    public static class ConsoleHelpers
    {
        private const int Width = 114;
        private const ConsoleColor color = ConsoleColor.Yellow;

        public static void PrintMultiClassClassificationMetrics(string name, MulticlassClassificationMetrics metrics)
        {
            WriteLineColor($"**********************************************************************************", color);
            WriteLineColor($"  Metrics for {name} multi-class classification model", color);
            WriteLineColor($"**********************************************************************************", color);
            WriteLineColor($"  MicroAccuracy = {metrics.MicroAccuracy:0.000} (the closer to 1, the better)", color);
            WriteLineColor($"  MacroAccuracy = {metrics.MacroAccuracy:0.000} (the closer to 1, the better)", color);
            WriteLineColor($"  LogLoss       = {metrics.LogLoss:0.000} (the closer to 0, the better)", color);
            WriteLineColor($"**********************************************************************************", color);
        }

        public static void PrintMulticlassClassificationFoldsAverageMetrics(IEnumerable<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> crossValResults)
        {
            var metricsInMultipleFolds = crossValResults.Select(r => r.Metrics);

            var microAccuracyValues = metricsInMultipleFolds.Select(m => m.MicroAccuracy);
            var microAccuracyAverage = microAccuracyValues.Average();
            var microAccuraciesStdDeviation = ModelHelpers.CalculateStandardDeviation(microAccuracyValues);
            var microAccuraciesConfidenceInterval95 = ModelHelpers.CalculateConfidenceInterval95(microAccuracyValues);

            var macroAccuracyValues = metricsInMultipleFolds.Select(m => m.MacroAccuracy);
            var macroAccuracyAverage = macroAccuracyValues.Average();
            var macroAccuraciesStdDeviation = ModelHelpers.CalculateStandardDeviation(macroAccuracyValues);
            var macroAccuraciesConfidenceInterval95 = ModelHelpers.CalculateConfidenceInterval95(macroAccuracyValues);

            var logLossValues = metricsInMultipleFolds.Select(m => m.LogLoss);
            var logLossAverage = logLossValues.Average();
            var logLossStdDeviation = ModelHelpers.CalculateStandardDeviation(logLossValues);
            var logLossConfidenceInterval95 = ModelHelpers.CalculateConfidenceInterval95(logLossValues);

            var logLossReductionValues = metricsInMultipleFolds.Select(m => m.LogLossReduction);
            var logLossReductionAverage = logLossReductionValues.Average();
            var logLossReductionStdDeviation = ModelHelpers.CalculateStandardDeviation(logLossReductionValues);
            var logLossReductionConfidenceInterval95 = ModelHelpers.CalculateConfidenceInterval95(logLossReductionValues);

            WriteLineColor($"**********************************************************************************", color);
            WriteLineColor($"  Metrics for multi-class Classification model using cross validation", color);
            WriteLineColor($"**********************************************************************************", color);
            WriteLineColor($"  Average MicroAccuracy:    {microAccuracyAverage:0.000}  - Standard deviation: {microAccuraciesStdDeviation:0.000}  - Confidence Interval 95%: {microAccuraciesConfidenceInterval95:0.000}", color);
            WriteLineColor($"  Average MacroAccuracy:    {macroAccuracyAverage:0.000}  - Standard deviation: {macroAccuraciesStdDeviation:0.000}  - Confidence Interval 95%: {macroAccuraciesConfidenceInterval95:0.000}", color);
            WriteLineColor($"  Average LogLoss:          {logLossAverage:0.000}  - Standard deviation: {logLossStdDeviation:0.000}  - Confidence Interval 95%: {logLossConfidenceInterval95:0.000}", color);
            WriteLineColor($"  Average LogLossReduction: {logLossReductionAverage:0.000}  - Standard deviation: {logLossReductionStdDeviation:0.000}  - Confidence Interval 95%: {logLossReductionConfidenceInterval95:0.000}", color);
            WriteLineColor($"**********************************************************************************", color);
        }

        public static void PrintMulticlassClassificationMetricsHeader()
        {
            CreateRow($"  {"No",-4} {"Trainer",-35} {"MicroAccuracy",14} {"MacroAccuracy",14} {"Duration",9}", Width);
        }

        public static void PrintIterationMetrics(int iteration, string trainerName, BinaryClassificationMetrics metrics, double? runtimeInSeconds)
        {
            CreateRow($"  {iteration,-4} {trainerName,-35} {metrics?.Accuracy ?? double.NaN,9:F4} {metrics?.AreaUnderRocCurve ?? double.NaN,8:F4} {metrics?.AreaUnderPrecisionRecallCurve ?? double.NaN,8:F4} {metrics?.F1Score ?? double.NaN,9:F4} {runtimeInSeconds.Value,9:F1}", Width);
        }

        public static void PrintIterationMetrics(int iteration, string trainerName, MulticlassClassificationMetrics metrics, double? runtimeInSeconds)
        {
            CreateRow($"  {iteration,-4} {trainerName,-35} {metrics?.MicroAccuracy ?? double.NaN,14:F4} {metrics?.MacroAccuracy ?? double.NaN,14:F4} {runtimeInSeconds.Value,9:F1}", Width);
        }

        public static void PrintIterationMetrics(int iteration, string trainerName, RegressionMetrics metrics, double? runtimeInSeconds)
        {
            CreateRow($"  {iteration,-4} {trainerName,-35} {metrics?.RSquared ?? double.NaN,8:F4} {metrics?.MeanAbsoluteError ?? double.NaN,13:F2} {metrics?.MeanSquaredError ?? double.NaN,12:F2} {metrics?.RootMeanSquaredError ?? double.NaN,8:F2} {runtimeInSeconds.Value,9:F1}", Width);
        }

        internal static void PrintIterationException(Exception ex)
        {
            WriteLine($"Exception during AutoML iteration: {ex}");
        }

        private static void CreateRow(string message, int width)
        {
            WriteLineColor(message.PadRight(width - 2), color);
        }

        public static void WriteLineColor(string textLine, ConsoleColor color)
        {
            ForegroundColor = color;
            WriteLine(textLine);
            ResetColor();
        }

        public static void PrintCancellationReason(string reason)
        {
                WriteLine($"CANCELED: Reason={reason}");
        }

        public static void PrintCancellationError(string errorCode, string errorDetails)
        {
            WriteLine($"CANCELED: ErrorCode={errorCode}");
            WriteLine($"CANCELED: ErrorDetails=[{errorDetails}]");
            WriteLine($"CANCELED: Did you update the subscription info?");
        }

        /// <summary>
        /// Print top models from AutoML experiment.
        /// </summary>
        public static void PrintTopModels(ExperimentResult<MulticlassClassificationMetrics> experimentResult)
        {
            // Get top few runs ranked by accuracy
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.MicroAccuracy))
                .OrderByDescending(r => r.ValidationMetrics.MicroAccuracy).Take(3);

            PrintMulticlassClassificationMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }
    }
}
