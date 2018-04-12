using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Classification
{
    /// <summary>
    /// A classifier instance provides a wrapper around a trained classifier, retaining info about the identifiers and scaling or shifting used in training the classifier
    /// </summary>
    public class ClassifierInstance
    {
        [JsonProperty]
        protected Classifier classifier;
        [JsonProperty]
        protected double[] featureScaling;
        [JsonProperty]
        protected double[] featureShift;
        [JsonProperty]
        protected string[] trainedIdentifiers;

        public ClassifierInstance(Classifier classifier, double[] featureScaling, double[] featureShift, string[] trainedIdentifiers)
        {
            this.classifier = classifier;
            this.featureScaling = featureScaling;
            this.featureShift = featureShift;
            this.trainedIdentifiers = trainedIdentifiers;
        }

        public double Verify(double[] features, string identifier)
        {
            // trying to match on an identifier that hasn't been trained wouldn't make sense
            if (!trainedIdentifiers.Contains(identifier))
            {
                throw new ArgumentException("Provided identifier does not match any trained identifiers");
            }

            // scale/shift features to fit within standard range
            double[] standardizedFeatures = StandardizationHelpers.GenerateTransformedData(features, (value, i) => (value * featureScaling[i]) + featureShift[i]);
            return classifier.Verify(standardizedFeatures, identifier);
        }

        public string[] GetTrainedIdentifiers()
        {
            return trainedIdentifiers;
        }

        public void Save(string classifierFile)
        {
            FileHelpers.WriteFile<ClassifierInstance>(classifierFile, this);
        }

        public static ClassifierInstance Read(string classifierFile)
        {
            return FileHelpers.ReadFile<ClassifierInstance>(classifierFile);
        }
    }
}
