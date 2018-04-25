using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification.KNN
{
    public class KNNClassifier : IClassifier
    {
        [JsonProperty]
        public int K { get; protected set; }
        [JsonProperty]
        protected List<ISample> trainingSamples;

        [JsonConstructor]
        KNNClassifier() { /* for serialization */ }

        public KNNClassifier(int K, List<ISample> trainingSamples)
        {
            if (K <= 0 || K >= trainingSamples.Count)
            {
                throw new ArgumentException("K must be a value greater than 0 and less than the number of training samples");
            }
            this.K = K;
            this.trainingSamples = trainingSamples;
        }

        public double Verify(double[] features, string identifier)
        {
            Neighbour[] neighbours = FindNearestNeighbours(features);

            // estimate the posterior probability ki / k
            return FindIdentifierProbability(neighbours, identifier);
        }

        protected Neighbour[] FindNearestNeighbours(double[] features)
        {
            // return list of neighbours ordered by distance then take first k
            return trainingSamples.Select(sample => new Neighbour(sample.GetIdentifier(), sample.GetDimensions().Select((feature, i) => Math.Pow(feature - features[i], 2)).Sum()))
                .OrderBy(neighbour => neighbour.Distance)
                .Take(K).ToArray();
        }

        protected Dictionary<string, double> FindWeightedCountPerNeighbour(Neighbour[] neighbours)
        {
            // take a weighted count of the number of samples per identifier in the neighbours (add 0.0000001 to avoid division by 0)
            Dictionary<string, double> neighbourCounter = neighbours.GroupBy(neighbour => neighbour.Identifier, neighbour => (1 / (neighbour.Distance + 0.0000001)))
                .ToDictionary(neighbour => neighbour.Key, neighbour => neighbour.Sum());

            // get all users as a part of this (but any that weren't part of the first K will have 0 probability assigned)
            // include imposters because a match to an imposter should also be returned
            return SampleSetHelpers.GetIdentifiers(trainingSamples, true).Select(ident => new { identifier = ident, weightedCount = neighbourCounter.ContainsKey(ident) ? neighbourCounter[ident] : 0.0 })
                .ToDictionary(weightedCount => weightedCount.identifier, weightedCount => weightedCount.weightedCount);
        }

        protected double FindIdentifierProbability(Neighbour[] neighbours, string identifier)
        {
            Dictionary<string, double> neighbourCounter = FindWeightedCountPerNeighbour(neighbours);

            double pIdentifier = 0.0;
            if (neighbourCounter.ContainsKey(identifier))
            {
                // votes for ki
                double ki = neighbourCounter[identifier];

                // total votes
                double k = neighbourCounter.Sum(weightedCount => weightedCount.Value);

                pIdentifier = ki / k;
            }
            return pIdentifier;
        }

        protected class Neighbour
        {
            public string Identifier { get; protected set; }
            public double Distance { get; protected set; }

            public Neighbour(string identifier, double distance)
            {
                Identifier = identifier;
                Distance = distance;
            }

        }
    }
}
