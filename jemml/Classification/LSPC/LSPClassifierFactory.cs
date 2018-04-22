using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Classification.LSPC
{
    public class LSPClassifierFactory<T> : ClassifierFactory<T> where T : Sample
    {
        protected double r;
        protected double w;

        public LSPClassifierFactory(double r, double w) : base(0, 1)
        {
            this.r = r;
            this.w = w;
        }

        public override ClassifierInstance CreateInstance(List<Sample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers)
        {
            LeastSquaresProbabilisticClassifier classifier = new LeastSquaresProbabilisticClassifier(trainingSamples, trainingIdentifiers, r, w);
            return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
        }
    }
}
