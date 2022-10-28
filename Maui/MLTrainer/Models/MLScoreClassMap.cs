using CsvHelper.Configuration;

namespace MLTrainer.Models
{
	public class MLScoreClassMap : ClassMap<MLScore>
	{
		public MLScoreClassMap()
		{
			Map(s => s.Text).Name("text");
			Map(s => s.IsNegative).Name("isnegative");
			Map(s => s.Importance).Name("importance");
		}
	}
}
