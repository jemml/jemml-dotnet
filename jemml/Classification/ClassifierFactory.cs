using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data;
using jemml.Data.Record;
using jemml.Data.Transform.DimensionalityReduction;
using jemml.Utilities;

namespace jemml.Classification
{
    /// <summary>
    /// Factory that generates and trains classifier instances to be used in verification
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ClassifierFactory<T> where T : Sample
    {
        protected Dictionary<int, ClassifierInstance> xClassifierInstances = null;
        protected double standardMin;
        protected double standardMax;

        public ClassifierFactory(double standardMin, double standardMax)
        {
            this.standardMin = standardMin;
            this.standardMax = standardMax;
        }

        public ClassifierFactory<T> Train(List<Sample> trainingSamples)
        {
            // train against all provided samples (i.e. none of these are needed for verification)
            xClassifierInstances = new Dictionary<int, ClassifierInstance>();
            xClassifierInstances.Add(0, CreateInstance(trainingSamples.Cast<Sample>().ToList(), standardMin, standardMax));
            return this;
        }

        public ClassifierFactory<T> Train(SampleSet<T> trainingSamples, int trainingSize, int xValidationStart = 0, int xValidationLength = 1)
        {
            // train with a dataset that may require additional cross validation classifier instances
            xClassifierInstances = Enumerable.Range(xValidationStart, xValidationLength).AsParallel().Select(x => new { x, instance = CreateInstance(SampleSetHelpers.GetSampleSetTrainingSamples(trainingSamples, trainingSize, x), standardMin, standardMax) })
                .ToDictionary(validation => validation.x, validation => validation.instance);
            return this;
        }

        public bool IsTrained()
        {
            return xClassifierInstances != null;
        }

        protected void ValidateTrainingData(List<Sample> trainingSamples)
        {
            // check all dimensions are equal
            AssertionHelpers.WithEqualNumberOfDimensions(trainingSamples);
        }

        public ClassifierInstance CreateInstance(List<Sample> trainingSamples, double standardMin, double standardMax)
        {
            // don't train imposter samples (if you know the sample's identifier they are not an "imposter" as far as classifier training is concerned)
            List<Sample> samplesToTrain = trainingSamples.Where(sample => !sample.IsImposter()).ToList();

            // validate training data
            ValidateTrainingData(samplesToTrain);

            // generate scaling/shift per feature to acheive a standard min/max for the training data
            double range = standardMax - standardMin;
            double[] featureMin = SampleSetHelpers.GetMinimumFeatureValues(samplesToTrain);
            double[] featureMax = SampleSetHelpers.GetMaximumFeatureValues(samplesToTrain);

            double[] featureScaling = featureMax.Select((max, i) => range / (max - featureMin[i])).ToArray();
            double[] featureShift = featureScaling.Select((scale, i) => standardMin - (featureMin[i] * scale)).ToArray();

            // apply scaling/shift to the features in each training sample
            List<Sample> standardizedSamples = samplesToTrain.Select(sample => sample.AcceptVisitor(
                new SampleScaledDimensionVisitor(featureScaling, featureShift))).ToList();

            string[] trainingIdentifiers = SampleSetHelpers.GetIdentifiers(samplesToTrain, false);
            return CreateInstance(standardizedSamples, featureScaling, featureShift, trainingIdentifiers);
        }

        public abstract ClassifierInstance CreateInstance(List<Sample> trainingSamples, double[] featureScaling, double[] featureShift, string[] trainingIdentifiers);

        public ClassifierInstance GetInstance(int crossValidation = 0)
        {
            if (!IsTrained() || !xClassifierInstances.ContainsKey(crossValidation))
            {
                throw new ArgumentException("Attempted cross validation " + crossValidation + " that has not been trained");
            }
            return xClassifierInstances[crossValidation];
        }
    }
}
