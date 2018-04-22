using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Evaluation
{
    public class OptimizationResult
    {
        public double[] FeaturesToERR { private set; get; }
        public int[] BestFeatureIndices { private set; get; }

        public OptimizationResult(double[] featuresToERR, int[] bestFeatures)
        {
            FeaturesToERR = featuresToERR;
            BestFeatureIndices = bestFeatures;
        }
    }
}
