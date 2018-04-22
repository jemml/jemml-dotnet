using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Classification.LDA
{
    public class LDAClassifierFactory<T> : ClassifierFactory<T> where T : Sample
    {
        protected double? gamma;
        protected LDAType type;

        public LDAClassifierFactory(double? gamma = null, LDAType type = LDAType.ULDA) : base(0, 1)
        {
            this.gamma = gamma;
            this.type = type;
        }

        public override ClassifierInstance CreateInstance(List<Sample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers)
        {
            LDAConfig config = gamma.HasValue ? new KLDAConfig(gamma.Value, trainingSamples, type) : new LDAConfig(type);
            Classifier classifier = new LinearDiscriminantAnalysisClassifier(config, trainingSamples, trainingIdentifiers);
            return new ClassifierInstance(classifier, featureScaling, featureShift, trainingIdentifiers);
        }
    }
}
