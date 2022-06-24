using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLTrainer.Models
{
    public class MLScoreClassMap : ClassMap<MLScore>
    {
        public MLScoreClassMap()
        {
            Map(s => s.Text).Name("text");
            Map(s => s.IsNegative).Name("isnegative");
        }
    }
}
