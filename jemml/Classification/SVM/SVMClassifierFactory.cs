using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Classification.SVM
{
    public class SVMClassifierFactory<T> : ClassifierFactory<T> where T : Sample
    {
        protected double C;
        protected double gamma;

        public SVMClassifierFactory(double C, double gamma) : base(0, 1)
        {
            this.gamma = gamma;
            this.C = C;
        }

        public override ClassifierInstance CreateInstance(List<Sample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers)
        {
            SVMClassifier classifier = new SVMClassifier(C, gamma, trainingSamples, trainingIdentifiers);
            return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
        }
    }
}
