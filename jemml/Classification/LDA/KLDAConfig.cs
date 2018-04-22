using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;
using Accord.Statistics.Kernels;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification.LDA
{
    public class KLDAConfig : LDAConfig
    {
        [JsonProperty]
        protected double gamma;
        [JsonProperty]
        protected double[,] trainingInput;

        [JsonConstructor]
        KLDAConfig() { /* for serialization */ }

        public KLDAConfig(double gamma, List<Sample> trainingSamples, LDAType type = LDAType.ULDA)
            : base(type)
        {
            this.gamma = gamma;
            this.trainingInput = DataHelpers.Convert(trainingSamples.Select(sample => sample.GetDimensions()).ToArray());
        }

        public override double[,] FormatInput(double[,] input)
        {
            // Create the Gram Kernel matrix
            int dimensions = input.GetLength(0);
            double[,] K = new double[dimensions, dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                double[] row = input.GetRow(i);
                for (int j = i; j < dimensions; j++)
                {
                    double k = GetKernel().Distance(row, input.GetRow(j));
                    K[i, j] = k;
                    K[j, i] = k;
                }
            }
            return K;
        }

        public override double[] FormatRow(double[] row)
        {
            return Enumerable.Range(0, trainingInput.GetLength(0)).Select(i => GetKernel().Distance(trainingInput.GetRow(i), row)).ToArray();
        }

        public Gaussian GetKernel()
        {
            return Gaussian.FromGamma(gamma);
        }
    }
}
