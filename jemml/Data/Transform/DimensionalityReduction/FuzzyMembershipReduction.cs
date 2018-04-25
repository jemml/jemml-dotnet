using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Data.Transform.DimensionalityReduction
{
    public class FuzzyMembershipReduction : DimensionalityReduction, ITrainable
    {
        [JsonProperty]
        protected int numberOfOutputs;
        [JsonProperty]
        protected int? trainingDimensions;
        [JsonProperty]
        protected int[] featureRankings;

        protected FuzzyMembershipReduction(int numberOfOutputs, int? trainingDimensions, int[] featureRankings, Preprocessor predecessor) : base(predecessor)
        {
            Init(numberOfOutputs, trainingDimensions, featureRankings);
        }

        public FuzzyMembershipReduction(int numberOfOutputs) : base(null)
        {
            Init(numberOfOutputs);
        }

        protected void Init(int numberOfOutputs, int? trainingDimensions = null, int[] featureRankings = null)
        {
            this.numberOfOutputs = numberOfOutputs;
            this.trainingDimensions = trainingDimensions;
            this.featureRankings = featureRankings;
        }

        public bool IsTrained()
        {
            return featureRankings != null;
        }

        public P Train<P>(List<ISample> trainingSamples) where P : Preprocessor
        {
            List<int> dimensionCounts = trainingSamples.Select(sample => sample.GetDimensionCount()).Distinct().ToList();
            if (dimensionCounts.Count != 1)
            {
                throw new ArgumentException("All samples must have the same number of dimensions"); // TODO - create parent or global method assertEqualDimensionality(samples)
            }
            trainingDimensions = trainingSamples[0].GetDimensionCount();

            // calculate the per feature means
            string[] identifiers = SampleSetHelpers.GetIdentifiers(trainingSamples, true).ToArray();
            Dictionary<string, double[]> means = CalculateDimensionMeans(trainingSamples, trainingDimensions.Value, identifiers);

            // calculate membership score for each feature then order by best associated indices
            featureRankings = Enumerable.Range(0, trainingDimensions.Value).AsParallel().Select(featureIndex => new { featureIndex, score = CalculateMembership(featureIndex, trainingSamples, means, identifiers) })
                .OrderBy(member => member.score)
                .Reverse()
                .Take(numberOfOutputs)
                .Select(member => member.featureIndex).ToArray();
            return this as P;
        }

        protected Dictionary<string, double[]> CalculateDimensionMeans(List<ISample> trainingSamples, int trainingDimensions, string[] identifiers)
        {
            return trainingSamples.SelectMany(sample => sample.GetDimensions().Select((value, index) => new { identifier = sample.GetIdentifier(), index, value }))
                .GroupBy(sm => sm.identifier, sm => new { sm.index, sm.value })
                .ToDictionary(sm => sm.Key, sm => sm.GroupBy(mean => mean.index, mean => mean.value).Select(mean => mean.Average()).ToArray());
        }

        protected double CalculateMembership(int featureIndex, List<ISample> trainingSamples, Dictionary<string, double[]> means, string[] identifiers)
        {
            // get and loop through each class (identifier)
            return identifiers.SelectMany(identifier =>
                trainingSamples.Where(sample => sample.GetIdentifier().Equals(identifier))
                .Select(sample => CalculateMembershipScore(featureIndex, identifier, sample.GetDimensions()[featureIndex], identifiers, means)))
                .Sum();
        }

        protected double CalculateMembershipScore(int j, string identifier, double xij, string[] identifiers, Dictionary<string, double[]> means)
        {
            double uikj = 0.0;
            double xkj_vij = Math.Pow(xij - means[identifier][j], 2);

            // if xkj = vij then uikj = 1
            if (xkj_vij != 0)
            {
                // for each identifier calculate uikj
                uikj = identifiers.Select(m => new { m, xkj_vmj = Math.Pow(xij - means[m][j], 2) })
                    .Where(mScore => !(mScore.xkj_vmj == 0 && identifier.Equals(mScore.m)))
                    .Select(mScore => Math.Pow(xkj_vij / mScore.xkj_vmj, 2))
                    .DefaultIfEmpty(0.0) // if xkj = vmj and i != m then uikj = 0
                    .Sum();

                if (uikj != 0)
                {
                    uikj = Math.Pow(uikj, -1);
                }
            }
            else
            {
                uikj = 1.0;
            }
            return uikj;
        }

        protected override double[] Reduce(ISample sample)
        {
            return sample.GetDimensions().Where((value, index) => featureRankings.Contains(index)).ToArray();
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new FuzzyMembershipReduction(numberOfOutputs, trainingDimensions, featureRankings, predecessor);
        }
    }
}
