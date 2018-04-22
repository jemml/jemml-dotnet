using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Classification;
using jemml.Data;
using jemml.Data.Record;
using jemml.Data.Transform.DimensionalityReduction;

namespace jemml.Evaluation
{
    public class FeatureOptimizer
    {
        public static OptimizationResult GetOptimalFeatures<T, C>(int bestNFeatures, SampleSet<T> sampleSet, C classifier, int trainingSize, int xValidationStart = 0, int xValidationLength = 1, double minInterval = Evaluator.DEFAULT_MIN_INTERVAL)
    where T : Sample
    where C : ClassifierFactory<T>
        {
            return GetOptimalFeatures<T>(bestNFeatures, sampleSet, (reducedSet, index) => {
                BiometricResult res = Evaluator.Evaluate(reducedSet, classifier.Train(reducedSet, trainingSize), trainingSize, xValidationStart, xValidationLength, minInterval);
                return new Tuple<int, double>(index, res.GetERR());
            });
        }

        private static OptimizationResult GetOptimalFeatures<T>(int bestNFeatures, SampleSet<T> samples, Func<SampleSet<T>, int, Tuple<int, double>> evaluator)
            where T : Sample
        {
            List<int> extractionIndices = new List<int>();
            List<double> errs = new List<double>();
            int dimensions = samples[0].GetDimensionCount();

            for (int i = 1; i <= bestNFeatures; i++)
            {
                Tuple<int, double> bestIndex = Enumerable.Range(0, dimensions).Where(d => !extractionIndices.Contains(d))
                    .Select(d => GetERR(extractionIndices, d, samples, evaluator)).OrderBy(d => d.Item2).First();
                extractionIndices.Add(bestIndex.Item1);
                errs.Add(bestIndex.Item2);
            }
            return new OptimizationResult(errs.ToArray(), extractionIndices.ToArray());
        }

        private static Tuple<int, double> GetERR<T>(List<int> extractionIndices, int tryIndex, SampleSet<T> samples, Func<SampleSet<T>, int, Tuple<int, double>> evaluator)
            where T : Sample
        {
            List<int> testIndices = new List<int>(extractionIndices);
            testIndices.Add(tryIndex);
            SampleSet<T> reducedSet = samples.ApplyDimensionalityReduction(new FeatureIndexReduction(testIndices.ToArray()));
            return evaluator.Invoke(reducedSet, tryIndex);
        }
    }
}
