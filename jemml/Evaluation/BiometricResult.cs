using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Evaluation
{
    /// <summary>
    /// For tracking the result of a biometric analysis
    /// </summary>
    public class BiometricResult
    {
        private double threshold; // the total cross validated EER
        private List<ErrorRatePair> errorRates; // the FAR / FRR for each cross validation
        public BiometricResult(double threshold, List<ErrorRatePair> errorRates)
        {
            this.threshold = threshold;
            this.errorRates = errorRates;
        }

        public double GetThreshold()
        {
            return threshold;
        }

        public List<ErrorRatePair> GetErrorRates()
        {
            return errorRates;
        }

        public double GetERR()
        {
            double totalAcceptedTests = errorRates.Select(errorRate => errorRate.GetTotalAcceptedTestCount()).Sum();
            double totalRejectedTests = errorRates.Select(errorRate => errorRate.GetTotalRejectedTestCount()).Sum();
            double falseAccepted = errorRates.Select(errorRate => errorRate.GetFalseAcceptedCount()).Sum();
            double falseRejected = errorRates.Select(errorRate => errorRate.GetFalseRejectedCount()).Sum();
            return ((falseAccepted / totalAcceptedTests) + (falseRejected / totalRejectedTests)) / 2;
        }
    }
}
