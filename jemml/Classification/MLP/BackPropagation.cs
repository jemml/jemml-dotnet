using System;
using System.Collections.Generic;
using System.Text;
using Encog.ML.Data;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Back;

namespace jemml.Classification.MLP
{
    public class BackPropagation : NetworkTrain
    {
        private double learningRate;
        private double momentum;

        public BackPropagation(double learningRate, double momentum)
        {
            this.learningRate = learningRate;
            this.momentum = momentum;
        }

        public ITrain TrainNetwork(IContainsFlat network, IMLDataSet trainingSet)
        {
            return new Backpropagation(network, trainingSet, learningRate, momentum);
        }
    }
}
