using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.Neural.Flat;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Classification.MLP
{
    public class MultiMLPClassifier : MLPClassifier
    {
        [JsonProperty]
        private Dictionary<string, FlatNetwork> networks;

        [JsonConstructor]
        MultiMLPClassifier() { /* for serialization */ }

        public MultiMLPClassifier(int hiddenLayers, INetworkTrain networkTrainer, double maxIterations, double minError, List<ISample> trainingSamples, string[] trainingIdentifiers) :
            base(hiddenLayers, networkTrainer, maxIterations, minError, trainingSamples, trainingIdentifiers)
        {
            // training initiated by base class
        }

        protected override void Train(int hiddenLayers, INetworkTrain networkTrainer, double maxIterations, double minError, List<ISample> trainingSamples, string[] trainingIdentifiers)
        {
            networks = trainingIdentifiers.AsParallel().Select(identifier => new {
                identifier,
                network = TrainNetwork(hiddenLayers, networkTrainer, maxIterations, minError, trainingSamples, 1, (sample) => sample.GetIdentifier().Equals(identifier) ? new double[] { 1.0 } : new double[] { 0.0 })
            }).ToDictionary(net => net.identifier, net => net.network.Structure.Flat);
        }

        public override double Verify(double[] features, string identifier)
        {
            // compute the MLP results
            double[] TOUTPUT = new double[1];
            lock (networkLock)
            {
                networks[identifier].Compute(features, TOUTPUT);
            }

            // the output will be a value between 0-1 (find appropriate acceptance threshold)
            return TOUTPUT[0];
        }
    }
}
