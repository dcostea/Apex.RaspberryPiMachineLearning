using Microsoft.ML.Data;

namespace RaspberryPiMachineLearning.MachineLearning
{
    public class SourceData
    {
        [ColumnName("Temperature"), LoadColumn(0)]
        public float Temperature { get; set; }

        [ColumnName("Luminosity"), LoadColumn(1)]
        public float Luminosity { get; set; }

        [ColumnName("Infrared"), LoadColumn(2)]
        public float Infrared { get; set; }

        //[ColumnName("Distance"), LoadColumn(3)]
        //public float Distance { get; set; }

        [ColumnName("Label"), LoadColumn(4)]
        public string Label { get; set; }
    }
}