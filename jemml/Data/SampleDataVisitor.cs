﻿using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data
{
    public class SampleDataVisitor : SampleVisitor<Sample>
    {
        private List<Tuple<double, double[]>> subsetRows;
        private bool recalculateDuration;

        public SampleDataVisitor(List<Tuple<double, double[]>> subsetRows, bool recalculateDuration)
        {
            this.subsetRows = subsetRows;
            this.recalculateDuration = recalculateDuration;
        }

        public Sample Accept(StandardSample sample)
        {
            if (recalculateDuration)
            {
                return new StandardSample(sample.GetIdentifier(), sample.IsImposter(), subsetRows, sample.GetOrder(), sample.GetDuration().HasValue);
            }
            else
            {
                return new StandardSample(sample.GetIdentifier(), sample.IsImposter(), subsetRows, sample.GetOrder(), sample.GetDuration());
            }
        }

        public Sample Accept(CrossValidatedSample sample)
        {
            if (recalculateDuration)
            {
                return new CrossValidatedSample(sample.GetIdentifier(), sample.IsImposter(), subsetRows, sample.GetOrder(), sample.GetCrossValidation(), sample.GetDuration().HasValue);
            }
            else
            {
                return new CrossValidatedSample(sample.GetIdentifier(), sample.IsImposter(), subsetRows, sample.GetOrder(), sample.GetCrossValidation(), sample.GetDuration());
            }
        }
    }
}
