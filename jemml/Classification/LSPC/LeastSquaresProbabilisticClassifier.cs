using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification.LSPC
{
    public class LeastSquaresProbabilisticClassifier : Classifier
    {
        [JsonProperty]
        protected Dictionary<string, int> identifiersMap;
        [JsonProperty]
        protected Dictionary<int, double[,]> inputSubsets;
        [JsonProperty]
        protected double w;
        [JsonProperty]
        protected double[][] alpha;

        [JsonConstructor]
        LeastSquaresProbabilisticClassifier() { /* for serialization */ }

        public LeastSquaresProbabilisticClassifier(List<Sample> trainingSamples, string[] trainingIdentifiers, double r, double w)
        {
            Train(trainingSamples, trainingIdentifiers, r, w);
        }

        protected void Train(List<Sample> trainingSamples, string[] trainingIdentifiers, double r, double w)
        {
            // generate a numeric mapping of our string identifiers to unique numeric values
            identifiersMap = trainingIdentifiers.Select((identifier, index) => new { identifier, index }).ToDictionary(id => id.identifier, id => id.index);

            // generate a numeric mapping of our string identifiers to unique numeric values
            double[][] inputSamples = trainingSamples.Select(sample => sample.GetDimensions()).ToArray();
            double[,] input = DataHelpers.Convert(inputSamples);
            int[] ideal = trainingSamples.Select(sample => identifiersMap[sample.GetIdentifier()]).ToArray();

            inputSubsets = GetInputSubsets(input, ideal);
            alpha = SolveAlpha(input, ideal, r, w);

            this.w = w;
        }

        private Dictionary<int, double[,]> GetInputSubsets(double[,] input, int[] rowClasses)
        {
            return rowClasses.Distinct().ToDictionary(i => i, i => MatrixHelpers.Subset(input, rowClasses, i));
        }

        private double[][] SolveAlpha(double[,] input, int[] rowClasses, double r, double w)
        {
            int[] labels = rowClasses.Distinct();
            double[][] alpha = new double[labels.Length][];

            return labels.Select(i =>
            {
                // store features for each sample in this class in a class subset array
                double[,] xy = MatrixHelpers.Subset(input, rowClasses, i);

                double[,] Hy = GetHMatrix(input, xy, w);
                double[] hy = GetHVector(xy, input.GetLength(0), w);

                // solve linear equation (Hy + rIny)alphay = hy subject to alphay >= 0
                double[,] alphay = Hy.Add(MatrixHelpers.Identity(Hy.GetLength(0), Hy.GetLength(1)).Multiply(r)).Inverse().Dot(hy.Transpose());
                return alphay.Transpose().GetRow(0);
            }).ToArray();
        }

        private double[,] GetHMatrix(double[,] x, double[,] xy, double w)
        {
            int ny = xy.GetLength(0);
            int n = x.GetLength(0);

            double[,] Hy = new double[ny, ny];
            for (int i = 0; i < ny; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    double hij = Enumerable.Range(0, n).Select(k =>
                    {
                        // || x[k] - xy[i] ||^2 + || x[k] - xy[j] ||^2 / 2w^2
                        double hijn = Enumerable.Range(0, x.GetLength(1)).Select(l => Math.Pow(x[k, l] - xy[i, l], 2.0) + Math.Pow(x[k, l] - xy[j, l], 2.0)).Sum();
                        return Math.Exp(-hijn / 2.0 * Math.Pow(w, 2.0));
                    }).Sum();
                    Hy[i, j] = (1.0 / (double)n) * hij;
                }
            }
            return Hy;
        }

        private double[] GetHVector(double[,] xy, int n, double w)
        {
            return Enumerable.Range(0, xy.GetLength(0)).Select(i =>
            {
                double hi = 0;
                for (int j = 0; j < xy.GetLength(0); j++)
                {
                    hi += Enumerable.Range(0, xy.GetLength(1)).Select(l => Math.Pow(xy[i, l] - xy[j, l], 2.0)).Sum();
                    hi += Math.Exp(-hi / (2.0 * Math.Pow(w, 2.0)));
                }
                return (1.0 / (double)n) * hi;
            }).ToArray();
        }

        public double GetProbability(int label, double[] x, double[][] alpha, double w, double maximumExpValue, List<int> ignoredNegativeLabels, Dictionary<int, double[,]> inputSubsets)
        {
            if (ignoredNegativeLabels.Contains(label))
            {
                // don't bother to return a value that we already know will be negative => return Math.Max(0, px)
                return 0;
            }

            double px = 0.0;
            double[] pValues = new double[inputSubsets[label].GetLength(0)];
            double[] exps = new double[inputSubsets[label].GetLength(0)];
            for (int i = 0; i < inputSubsets[label].GetLength(0); i++)
            {
                double euxy = CalculateEuclidean(x, i, label, inputSubsets, x.Length);
                double exp = -euxy / (2.0 * Math.Pow(w, 2.0));
                px += alpha[label][i] * Math.Exp(exp - maximumExpValue);
                exps[i] = exp;
                pValues[i] = alpha[label][i] * Math.Exp(exp - maximumExpValue);
            }
            return px;
        }

        private double CalculateEuclidean(double[] x, int i, int label, Dictionary<int, double[,]> inputSubsets, int xLength)
        {
            // || x - xy ||^2
            return Enumerable.Range(0, xLength).Select(j => Math.Pow(x[j] - inputSubsets[label][i, j], 2.0)).Sum();
        }

        private Tuple<double, List<int>> GetMaximumTrainingExponent(double[] x, double[][] alpha, double w, int numLabels, Dictionary<int, double[,]> inputSubsets)
        {
            double maxExp = Double.MinValue;
            List<int> negativeLabelData = new List<int>();
            for (int label = 0; label < numLabels; label++)
            {
                // calculate the max label exp to determine whether the sum will be positive or negative (i.e. should we ignore it)
                double[] labelExps = Enumerable.Range(0, inputSubsets[label].GetLength(0)).Select(i => -CalculateEuclidean(x, i, label, inputSubsets, x.Length) / (2.0 * Math.Pow(w, 2.0))).ToArray();
                double maxLabelExp = labelExps.Max();
                double[] testExps = Enumerable.Range(0, inputSubsets[label].GetLength(0)).Select(i => alpha[label][i] * Math.Exp(labelExps[i] - maxLabelExp)).ToArray();
                double testSum = testExps.Sum();
                double testMaxExp = Math.Max(maxExp, Enumerable.Range(0, inputSubsets[label].GetLength(0)).Where(i => alpha[label][i] > 0).Select(i => testExps[i]).Max());

                // if test sum < 0 => this term will come out negative so don't include it as a max exponent
                if (testSum > 0)
                {
                    maxExp = testMaxExp;
                }
                else
                {
                    negativeLabelData.Add(label);
                }

                // if this is the last term and nothing has been set then nothing is positive - let it fail gracefully by assigning testSum to the last term
                if (maxExp == Double.MinValue && label == numLabels - 1)
                {
                    maxExp = testMaxExp;
                }
            }
            //return maxExp;
            return Tuple.Create(maxExp, negativeLabelData);
        }

        public double Verify(double[] features, string identifier)
        {
            Tuple<double, List<int>> modifiers = GetMaximumTrainingExponent(features, alpha, w, identifiersMap.Count, inputSubsets);
            double maxExp = modifiers.Item1;
            List<int> ignoredNegativeLabels = modifiers.Item2;

            double p = identifiersMap.Select(i => GetProbability(i.Value, features, alpha, w, maxExp, ignoredNegativeLabels, inputSubsets)).Sum();

            if (p == 0)
            {
                //Console.WriteLine("FOUND ONLY NEGATIVE PROBABILITY NUMBERS");
                return 0;
            }
            double pr = GetProbability(identifiersMap[identifier], features, alpha, w, maxExp, ignoredNegativeLabels, inputSubsets) / p;

            if (Double.IsNaN(pr))
            {
                Console.WriteLine("PR SHOULD NOT BE NAN");
            }
            return pr;
        }
    }
}
