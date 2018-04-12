using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data;
using jemml.Data.Record;

namespace jemml.Utilities
{
    public class SampleSetHelpers
    {
        // count the number of distinct identifiers per sample
        public static int CountIdentifiers<T>(SampleSet<T> samples) where T : Sample
        {
            return samples.Select(x => x.GetIdentifier()).Distinct().Count();
        }

        public static List<Sample> GetCrossValidation(List<CrossValidatedSample> samples, int crossValidation)
        {
            return samples.Where(x => x.GetCrossValidation().Equals(crossValidation)).ToList().Cast<Sample>().ToList();
        }

        // get a list of all dinstinct identifiers (including imposter samples if requested)
        public static string[] GetIdentifiers(List<Sample> samples, bool includeImposters)
        {
            return samples.Where(x => includeImposters || !x.IsImposter()).Select(x => x.GetIdentifier()).Distinct().ToArray();
        }

        public static List<Sample> GetSamplesWithIdentifier(List<Sample> samples, string identifier)
        {
            List<Sample> identifierSamples = samples.Where(x => x.GetIdentifier().Equals(identifier)).ToList();
            identifierSamples.Sort(new Comparison<Sample>((s1, s2) => s1.GetOrder().CompareTo(s2.GetOrder()))); // order samples by identifier
            return identifierSamples;
        }

        public static List<Sample> GetSampleSetTrainingSamples<T>(SampleSet<T> sampleSet, int trainingSize, int x) where T : Sample
        {
            if (sampleSet.IsCrossValidatedSampleSet())
            {
                // if the list is cross validated then filter out this cross validation
                return GetTrainingSamples(GetCrossValidation(sampleSet.Cast<CrossValidatedSample>().ToList(), x), trainingSize, x);
            }
            else
            {
                // if the list is not cross validated then use it directly
                return GetTrainingSamples(sampleSet.AsSampleList(), trainingSize, x);
            }
        }

        private static List<Sample> GetTrainingSamples(List<Sample> sampleSet, int trainingSize, int x)
        {
            string[] identifiers = GetIdentifiers(sampleSet, false); // don't include imposter identifiers in cross validation generation
            List<Sample> trainingSamples = new List<Sample>();

            foreach (String identifier in identifiers)
            {
                List<Sample> identifierSamples = GetSamplesWithIdentifier(sampleSet, identifier);
                if (trainingSize >= identifierSamples.Count)
                {
                    throw new ArgumentException("Sample with identifier " + identifier + " contains fewer samples than the allowable training size");
                }
                // get all samples for each identifier within the provided training cross validation range
                for (int k = 0; k < trainingSize; k++)
                {
                    trainingSamples.Add(identifierSamples[(k + x) % identifierSamples.Count]);
                }
            }
            return trainingSamples;
        }


        public static List<Sample> GetSampleSetTestingSamples<T>(SampleSet<T> sampleSet, int trainingSize, int x) where T : Sample
        {
            if (sampleSet.IsCrossValidatedSampleSet())
            {
                // if the list is cross validated then filter out this cross validation
                return GetTestingSamples(GetCrossValidation(sampleSet.Cast<CrossValidatedSample>().ToList(), x), trainingSize, x);
            }
            else
            {
                // if the list is not cross validated then use it directly
                return GetTestingSamples(sampleSet.AsSampleList(), trainingSize, x);
            }
        }

        private static List<Sample> GetTestingSamples(List<Sample> sampleSet, int trainingSize, int x)
        {
            string[] identifiers = GetIdentifiers(sampleSet, false);
            List<Sample> testingSamples = new List<Sample>();

            foreach (String identifier in identifiers)
            {
                List<Sample> identifierSamples = GetSamplesWithIdentifier(sampleSet, identifier);
                // get all samples for each identifier within the provided testing cross validation range
                for (int k = trainingSize; k <= identifierSamples.Count; k++)
                {
                    testingSamples.Add(identifierSamples[(k + x) % identifierSamples.Count]);
                }
            }
            testingSamples.AddRange(sampleSet.Where(sample => sample.IsImposter())); // always include all imposters since they aren't trained
            return testingSamples;
        }

        public static double[] GetMinimumFeatureValues(List<Sample> sampleSet)
        {
            return Enumerable.Range(0, sampleSet.First().GetDimensionCount()).Select(i => sampleSet.Select(sample => sample.GetDimensions()[i]).Min()).ToArray();
        }

        public static double[] GetMaximumFeatureValues(List<Sample> sampleSet)
        {
            return Enumerable.Range(0, sampleSet.First().GetDimensionCount()).Select(i => sampleSet.Select(sample => sample.GetDimensions()[i]).Max()).ToArray();
        }
    }
}
