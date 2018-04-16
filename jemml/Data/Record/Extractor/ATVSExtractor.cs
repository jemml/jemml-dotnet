using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Record.Extractor
{
    public class ATVSExtractor : SampleExtractor
    {
        protected ATVSExtractor(Preprocessor predecessor) : base(predecessor) { }

        public ATVSExtractor() : this(null) { }

        protected override List<Tuple<double, double[]>> ExtractRows(Sample sample)
        {
            List<Tuple<double, double[]>> atvsRows = new List<Tuple<double, double[]>>();
            foreach (Tuple<double, double[]> row in sample.GetDataRows())
            {
                if (row.Item2.Where(column => Math.Abs(column) > 0).Count() > 0)
                {
                    atvsRows.Add(row);
                }
                else if (atvsRows.Count > 0) // if we haven't recorded any rows the sample recording may have started too early
                {
                    break; // don't continue past the point where no rows have pressure readings
                }
            }
            return atvsRows;
        }

        protected override bool RecalculateDuration()
        {
            return true;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new ATVSExtractor(predecessor);
        }
    }
}
