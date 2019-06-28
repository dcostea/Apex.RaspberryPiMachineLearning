using Microsoft.ML.Data;

namespace RaspberryPiMachineLearning.MachineLearning
{
    public class SourcePrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel;

        [ColumnName("Score")]
        public float[] Score;
    }
}