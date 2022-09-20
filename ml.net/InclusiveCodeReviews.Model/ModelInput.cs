//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace InclusiveCodeReviews.Model
{
    public class ModelInput
    {
        [ColumnName("text"), LoadColumn(0)]
        public string Text { get; set; }


        [ColumnName("isnegative"), LoadColumn(1)]
        public string Isnegative { get; set; }

        [ColumnName("importance"), LoadColumn(2)]
        public float Importance { get; set; } = 0.5f;

        public override string ToString() => Text;
    }
}
