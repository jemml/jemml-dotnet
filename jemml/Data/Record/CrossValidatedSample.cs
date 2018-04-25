using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Data.Record
{
    public class CrossValidatedSample : StandardSample
    {
        [JsonProperty]
        private int crossValidation;

        protected CrossValidatedSample()
        {
            // for serialization only
        }

        public CrossValidatedSample(string identifier, bool isImposter, List<Tuple<double, double[]>> dataRows, int order, int crossValidation, bool hasDuration) :
            this(identifier, isImposter, dataRows, order, crossValidation, CalculateDuration(dataRows, hasDuration))
        {

        }

        public CrossValidatedSample(string identifier, bool isImposter, List<Tuple<double, double[]>> dataRows, int order, int crossValidation, double? duration)
            : base(identifier, isImposter, dataRows, order, duration)
        {
            if (crossValidation < 0)
            {
                throw new ArgumentException("Cross Validation must be a number greater than 0");
            }
            this.crossValidation = crossValidation;
        }

        public int GetCrossValidation()
        {
            return crossValidation;
        }

        public override T AcceptVisitor<T>(ISampleVisitor<T> visitor)
        {
            return visitor.Accept(this);
        }
    }
}
