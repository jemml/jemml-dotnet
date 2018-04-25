using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.Engine.Network.Activation;
using Encog.MathUtil.Randomize;
using Encog.Neural.Data.Basic;
using Encog.Neural.Flat;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training;
using Encog.Neural.NeuralData;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Classification.MLP
{
    public class MLPClassifier : IClassifier
    {
        [JsonProperty]
        private Dictionary<string, int> identifiersMap;
        [JsonProperty]
        private FlatNetwork network;
        protected Object networkLock = new Object(); // this shouldn't be needed but the network compute method doesn't appear to be threadsafe (see https://github.com/encog/encog-dotnet-core/issues/112)

        [JsonConstructor]
        protected MLPClassifier() { /* for serialization */ }

        public MLPClassifier(int hiddenLayerNodes, INetworkTrain networkTrainer, double maxIterations, double minError, List<ISample> trainingSamples, string[] trainingIdentifiers)
        {
            Train(hiddenLayerNodes, networkTrainer, maxIterations, minError, trainingSamples, trainingIdentifiers);
        }

        protected virtual void Train(int hiddenLayerNodes, INetworkTrain networkTrainer, double maxIterations, double minError, List<ISample> trainingSamples, string[] trainingIdentifiers)
        {
            // generate a numeric mapping of our string identifiers to unique numeric values
            identifiersMap = trainingIdentifiers.Select((identifier, index) => new { identifier, index }).ToDictionary(id => id.identifier, id => id.index);

            // account for imposter samples (identifier not in identifiersMap)
            network = TrainNetwork(hiddenLayerNodes, networkTrainer, maxIterations, minError, trainingSamples, identifiersMap.Count,
                (sample) => Enumerable.Range(0, identifiersMap.Count).Select(i => i == identifiersMap[sample.GetIdentifier()] ? 1.0 : 0.0).ToArray()).Structure.Flat;
        }

        protected BasicNetwork TrainNetwork(int hiddenLayerNodes, INetworkTrain networkTrainer, double maxIterations, double minError, List<ISample> trainingSamples, int outputs, Func<ISample, double[]> idealFunction)
        {
            // Construct the Neural Network
            BasicNetwork network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, trainingSamples[0].GetDimensionCount()));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, hiddenLayerNodes));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, outputs));
            network.Structure.FinalizeStructure();
            network.Reset();
            new ConsistentRandomizer(-1.0, 1.0, 500).Randomize(network);

            // get the input and ideal output for training (1.0 = correct for this identifier, 0.0 = incorrect)
            double[][] INPUT = trainingSamples.Select(sample => sample.GetDimensions()).ToArray();
            double[][] IDEAL = trainingSamples.Select(sample => idealFunction.Invoke(sample)).ToArray();

            // train the neural network
            INeuralDataSet trainingSet = new BasicNeuralDataSet(INPUT, IDEAL);
            ITrain train = networkTrainer.TrainNetwork(network, trainingSet);

            int epoch = 1;
            do
            {
                train.Iteration();
                epoch++;
            } while ((epoch < maxIterations) && (train.Error > minError));

            return network;
        }

        public virtual double Verify(double[] features, string identifier)
        {
            // compute the MLP results
            double[] TOUTPUT = new double[identifiersMap.Count];
            lock (networkLock)
            {
                network.Compute(features, TOUTPUT);
            }

            // the output will be a value between 0-1 (find appropriate acceptance threshold)
            return TOUTPUT[identifiersMap[identifier]];
        }
    }
}
