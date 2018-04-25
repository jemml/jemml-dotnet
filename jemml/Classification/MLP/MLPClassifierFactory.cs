using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Classification.MLP
{
    public class MLPClassifierFactory<T> : ClassifierFactory<T> where T : ISample
    {
        protected int hiddenLayerNodes;
        protected INetworkTrain networkTrainer;
        protected double maxIterations;
        protected double minError;
        protected MLPNetworkConfig config;

        public MLPClassifierFactory(int hiddenLayerNodes, INetworkTrain networkTrainer, double maxIterations = 100000, double minError = 0.0000001, MLPNetworkConfig config = MLPNetworkConfig.ManyToOne)
            : base(-1, 1)
        {
            this.hiddenLayerNodes = hiddenLayerNodes;
            this.networkTrainer = networkTrainer;
            this.maxIterations = maxIterations;
            this.minError = minError;
            this.config = config;
        }

        public override ClassifierInstance CreateInstance(List<ISample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers)
        {
            if (config == MLPNetworkConfig.ManyToOne)
            {
                MLPClassifier classifier = new MLPClassifier(hiddenLayerNodes, networkTrainer, maxIterations, minError, trainingSamples, trainingIdentifiers);
                return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
            }
            else
            {
                MultiMLPClassifier classifier = new MultiMLPClassifier(hiddenLayerNodes, networkTrainer, maxIterations, minError, trainingSamples, trainingIdentifiers);
                return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
            }
        }
    }
}
