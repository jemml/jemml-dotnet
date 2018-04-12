using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Classification;
using jemml.Data;
using jemml.Data.Record;
using jemml.Utilities;

namespace jemml.Evaluation
{
    /// <summary>
    /// Tools to evaluate biometric classifiers against sample sets
    /// </summary>
    public class Evaluator
    {
        public const double DEFAULT_MIN_INTERVAL = 0.01;

        // perform cross validated EER evaluation
        public static BiometricResult Evaluate<T, C>(SampleSet<T> sampleSet, C classifier, int trainingSize, int xValidationStart = 0, int xValidationLength = 1, double minInterval = DEFAULT_MIN_INTERVAL)
            where T : Sample
            where C : ClassifierFactory<T>
        {
            if (minInterval >= 0.5 || minInterval <= 0)
            {
                throw new ArgumentException("Invalid Min Interval: must be a number between 0 and 0.5");
            }
            return Evaluate(sampleSet, classifier, trainingSize, minInterval, 0.5, new BiometricResult(0.5, null), xValidationStart, xValidationLength);
        }

        private static BiometricResult Evaluate<T>(SampleSet<T> sampleSet, ClassifierFactory<T> classifier, int trainingSize, double minInterval, double interval, BiometricResult result, int xValidationStart, int xValidationLength) where T : Sample
        {
            if (interval < minInterval)
            {
                // the threshold found for the smallest interval represents the best guess at the EER
                return result;
            }
            else
            {
                // recurse until the smallest allowable interval is found
                Tuple<ErrorRatePair, List<ErrorRatePair>> delta1 = CalculateErrorRate(sampleSet, classifier, trainingSize, (result.GetThreshold() - (interval / 2.0)), xValidationStart, xValidationLength);
                Tuple<ErrorRatePair, List<ErrorRatePair>> delta2 = CalculateErrorRate(sampleSet, classifier, trainingSize, (result.GetThreshold() + (interval / 2.0)), xValidationStart, xValidationLength);

                if (delta1.Item1.getErrorDelta() < delta2.Item1.getErrorDelta())
                {
                    return Evaluate(sampleSet, classifier, trainingSize, minInterval, (interval / 2.0), new BiometricResult((result.GetThreshold() - (interval / 2.0)), delta1.Item2), xValidationStart, xValidationLength);
                }
                else
                {
                    return Evaluate(sampleSet, classifier, trainingSize, minInterval, (interval / 2.0), new BiometricResult((result.GetThreshold() + (interval / 2.0)), delta2.Item2), xValidationStart, xValidationLength);
                }
            }
        }

        public static BiometricResult Evaluate(ClassifierFactory<Sample> classifier, List<Sample> testingSamples)
        {
            return Evaluate(classifier.GetInstance(0), testingSamples);
        }

        // perform EER evaluation on provided testing samples
        public static BiometricResult Evaluate(ClassifierInstance classifier, List<Sample> testingSamples, double minInterval = DEFAULT_MIN_INTERVAL)
        {
            return Evaluate(classifier, testingSamples, minInterval, 0.5, new BiometricResult(0.5, null));
        }

        private static BiometricResult Evaluate(ClassifierInstance classifier, List<Sample> testingSamples, double minInterval, double interval, BiometricResult result)
        {
            if (interval < minInterval)
            {
                // the threshold found for the smallest interval represents the best guess at the EER
                return result;
            }
            else
            {
                // recurse until the smallest allowable interval is hit
                ErrorRatePair delta1 = CalculateErrorRate(classifier, testingSamples, (result.GetThreshold() - (interval / 2.0)));
                ErrorRatePair delta2 = CalculateErrorRate(classifier, testingSamples, (result.GetThreshold() + (interval / 2.0)));

                Console.WriteLine("delta1 : " + delta1.getFAR() + " " + delta1.getFRR() + " \n " + delta2.getFAR() + " " + delta2.getFRR());
                Console.WriteLine("threshold: " + result.GetThreshold());
                Console.WriteLine("errorDelta1: " + delta1.getErrorDelta() + " errorDelta2: " + delta2.getErrorDelta());

                if (delta1.getErrorDelta() < delta2.getErrorDelta())
                {
                    Console.WriteLine("Choose delta1");
                    return Evaluate(classifier, testingSamples, minInterval, (interval / 2.0), new BiometricResult((result.GetThreshold() - (interval / 2.0)), new List<ErrorRatePair> { delta1 }));
                }
                else
                {
                    Console.WriteLine("Choose delta2");
                    return Evaluate(classifier, testingSamples, minInterval, (interval / 2.0), new BiometricResult((result.GetThreshold() + (interval / 2.0)), new List<ErrorRatePair> { delta2 }));
                }
            }
        }

        private static Tuple<ErrorRatePair, List<ErrorRatePair>> CalculateErrorRate<T>(SampleSet<T> sampleSet, ClassifierFactory<T> classifier, int trainingSize, double threshold, int xValidationStart, int xValidationLength) where T : Sample
        {
            // test classifier instances with testing samples and threshold asynchronously to get cross validated result
            List<ErrorRatePair> errorRates = Enumerable.Range(xValidationStart, xValidationLength).AsParallel()
                .Select(x => CalculateErrorRate(classifier.GetInstance(x), SampleSetHelpers.GetSampleSetTestingSamples(sampleSet, trainingSize, x), threshold))
                .ToList();

            ErrorRatePair totalError = errorRates[0];
            for (int i = 1; i < errorRates.Count; i++)
            {
                totalError = totalError + errorRates[i];
            }
            return new Tuple<ErrorRatePair, List<ErrorRatePair>>(totalError, errorRates);
        }

        public static ErrorRatePair CalculateErrorRate(ClassifierInstance classifier, List<Sample> testingSamples, double threshold)
        {
            Tuple<int, int> farResults = CalculateFAR(classifier, testingSamples, threshold);
            Tuple<int, int> frrResults = CalculateFRR(classifier, testingSamples, threshold);
            return new ErrorRatePair(farResults.Item1, farResults.Item2, frrResults.Item1, frrResults.Item2);
        }

        private static Tuple<int, int> CalculateFAR(ClassifierInstance classifier, List<Sample> testingSamples, double threshold)
        {
            string[] identifiers = classifier.GetTrainedIdentifiers();
            // for each identifier find the samples that correctly match the identifier given the threshold
            VerificationResult result = identifiers.AsParallel().SelectMany(identifier =>
                testingSamples.Where(sample => !sample.GetIdentifier().Equals(identifier)).Select(sample =>
                {
                    if (classifier.Verify(sample.GetDimensions(), identifier) > threshold)
                    {
                        return new VerificationResult(1, 0); // falsely accepted sample for a different identifier
                    }
                    else
                    {
                        return new VerificationResult(0, 1); // correctly rejected sample for a different identifier
                    }
                })
            ).Aggregate((current, next) => current + next);

            return new Tuple<int, int>(result.Accepted, (result.Accepted + result.Rejected));
        }

        private static Tuple<int, int> CalculateFRR(ClassifierInstance classifier, List<Sample> testingSamples, double threshold)
        {
            string[] identifiers = classifier.GetTrainedIdentifiers();
            // for each identifier find the samples taht incorrectly match the identifier given the threshold
            VerificationResult result = identifiers.AsParallel().SelectMany(identifier =>
                testingSamples.Where(sample => sample.GetIdentifier().Equals(identifier)).Select(sample =>
                {
                    if (classifier.Verify(sample.GetDimensions(), identifier) <= threshold)
                    {
                        return new VerificationResult(0, 1); // falsely rejected sample for the correct identifier
                    }
                    else
                    {
                        return new VerificationResult(1, 0); // correctly accepted sample for the identifier
                    }
                })
            ).Aggregate((current, next) => current + next);
            return new Tuple<int, int>(result.Rejected, (result.Accepted + result.Rejected));
        }

        private class VerificationResult
        {
            public int Accepted { get; private set; }
            public int Rejected { get; private set; }

            public VerificationResult(int accepted, int rejected)
            {
                this.Accepted = accepted;
                this.Rejected = rejected;
            }

            public static VerificationResult operator +(VerificationResult e1, VerificationResult e2)
            {
                return new VerificationResult(e1.Accepted + e2.Accepted, e1.Rejected + e2.Rejected);
            }
        }
    }
}
