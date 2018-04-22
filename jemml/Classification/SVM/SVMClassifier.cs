using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Encog.MathUtil.LIBSVM;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.SVM.Training;
using jemml.Data.Record;
using Newtonsoft.Json;

namespace jemml.Classification.SVM
{
    public class SVMClassifier : Classifier
    {
        static SVMClassifier()
        {
            SupportClass.Random = new Random(0); // always use the same random seed so outputs are predictable
        }

        [JsonProperty]
        private Dictionary<string, int> identifiersMap;
        [JsonProperty]
        [JsonConverter(typeof(SVMModelConverter))]
        private svm_model model;
        private ProbabilitySupportVectorMachine svmNetwork;

        [JsonConstructor]
        SVMClassifier(Dictionary<string, int> identifiersMap, svm_model model)
        {
            /* for serialization */
            svmNetwork = new ProbabilitySupportVectorMachine(model);
            this.identifiersMap = identifiersMap;
            this.model = model;
        }

        public SVMClassifier(double C, double gamma, List<Sample> trainingSamples, string[] trainingIdentifiers)
        {
            Train(C, gamma, trainingSamples, trainingIdentifiers);
        }

        protected void Train(double C, double gamma, List<Sample> trainingSamples, string[] trainingIdentifiers)
        {
            // generate a numeric mapping of our string identifiers to unique numeric values
            identifiersMap = trainingIdentifiers.Select((identifier, index) => new { identifier, index }).ToDictionary(id => id.identifier, id => id.index);

            // don't include imposter samples
            svmNetwork = TrainSVM(C, gamma, trainingSamples, (sample) => identifiersMap[sample.GetIdentifier()]);
            model = svmNetwork.Model;
        }

        protected ProbabilitySupportVectorMachine TrainSVM(double C, double gamma, List<Sample> trainingSamples, Func<Sample, double> idealFunction)
        {
            // duplicate the training dataset for better cross validation by LIBSVM probability generator (see LIBSVM documentation)
            List<double[]> inputSamples = trainingSamples.Select(sample => sample.GetDimensions()).ToList();
            inputSamples.AddRange(trainingSamples.Select(sample => sample.GetDimensions()));

            // account for imposter samples (identifier not in identifierMap)
            List<double[]> outputSamples = trainingSamples.Select(sample => new double[] { idealFunction.Invoke(sample) }).ToList();
            outputSamples.AddRange(trainingSamples.Select(sample => new double[] { idealFunction.Invoke(sample) }));

            double[][] INPUT = inputSamples.ToArray();
            double[][] IDEAL = outputSamples.ToArray();

            // train the SVM classifier with the provided data
            IMLDataSet trainingData = new BasicMLDataSet(INPUT, IDEAL);

            ProbabilitySupportVectorMachine svmNetwork = new ProbabilitySupportVectorMachine(trainingSamples[0].GetDimensionCount(), false, 0.00000001);

            // train the SVM classifier with the provided C and gamma
            SVMTrain trainedSVM = new SVMTrain(svmNetwork, trainingData);
            trainedSVM.Fold = 0;
            trainedSVM.Gamma = gamma;
            trainedSVM.C = C;

            trainedSVM.Iteration();

            Console.WriteLine("SVM training error: " + trainedSVM.Error);
            return svmNetwork;
        }

        public double Verify(double[] features, string identifier)
        {
            // find the probability that the given identifier matches the provided sample
            return svmNetwork.Verify(new BasicMLData(features), identifiersMap[identifier], identifiersMap.Count);
        }
    }
}
