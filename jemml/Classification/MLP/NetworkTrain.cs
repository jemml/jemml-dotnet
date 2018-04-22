using System;
using System.Collections.Generic;
using System.Text;
using Encog.ML.Data;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;

namespace jemml.Classification.MLP
{
    public interface NetworkTrain
    {
        ITrain TrainNetwork(IContainsFlat network, IMLDataSet trainingSet);
    }
}
