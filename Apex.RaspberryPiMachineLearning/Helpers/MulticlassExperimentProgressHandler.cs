using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using System;

namespace Apex.RaspberryPiMachineLearning.Helpers
{
    public class MulticlassExperimentProgressHandler : IProgress<RunDetail<MulticlassClassificationMetrics>>
    {
        private int _iterationIndex;

        public void Report(RunDetail<MulticlassClassificationMetrics> iterationResult)
        {
            if (_iterationIndex++ == 0)
            {
                ConsoleHelpers.PrintMulticlassClassificationMetricsHeader();
            }

            if (iterationResult.Exception != null)
            {
                ConsoleHelpers.PrintIterationException(iterationResult.Exception);
            }
            else
            {
                ConsoleHelpers.PrintIterationMetrics(_iterationIndex, iterationResult.TrainerName,
                    iterationResult.ValidationMetrics, iterationResult.RuntimeInSeconds);
            }
        }
    }
}
