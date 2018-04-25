using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Classification.KNN
{
    public class KNNClassifierFactory<T> : ClassifierFactory<T> where T : ISample
    {
        public int K { get; protected set; }

        public KNNClassifierFactory(int K) : base(0, 1)
        {
            this.K = K;
        }

        public override ClassifierInstance CreateInstance(List<ISample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers)
        {
            KNNClassifier classifier = new KNNClassifier(K, trainingSamples);
            return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
        }
    }
}
