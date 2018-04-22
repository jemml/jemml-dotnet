using System;
using System.Collections.Generic;
using System.Text;
using Encog.ML.Data;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;

namespace jemml.Classification.MLP
{
    public class ResilientPropagation : NetworkTrain
    {
        public ITrain TrainNetwork(IContainsFlat network, IMLDataSet trainingSet)
        {
            return new Encog.Neural.Networks.Training.Propagation.Resilient.ResilientPropagation(network, trainingSet);
        }
    }
}
