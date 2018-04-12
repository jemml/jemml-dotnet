using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Evaluation
{
    /// <summary>
    /// The class is used to keep track of individual FAR / FRR pairs that would be recovered during cross validation
    /// </summary>
    public class ErrorRatePair
    {
        private int falseAcceptedCount;
        private int totalAcceptedTestCount;
        private int falseRejectedCount;
        private int totalRejectedTestCount;

        public ErrorRatePair(int falseAcceptedCount, int totalAcceptedTestCount, int falseRejectedCount, int totalRejectedTestCount)
        {
            this.falseAcceptedCount = falseAcceptedCount;
            this.totalAcceptedTestCount = totalAcceptedTestCount;
            this.falseRejectedCount = falseRejectedCount;
            this.totalRejectedTestCount = totalRejectedTestCount;
        }

        public int getFalseAcceptedCount()
        {
            return falseAcceptedCount;
        }

        public int getTotalAcceptedTestCount()
        {
            return totalAcceptedTestCount;
        }

        public int getFalseRejectedCount()
        {
            return falseRejectedCount;
        }

        public int getTotalRejectedTestCount()
        {
            return totalRejectedTestCount;
        }

        public double getFAR()
        {
            return (double)falseAcceptedCount / totalAcceptedTestCount;
        }

        public double getFRR()
        {
            return (double)falseRejectedCount / totalRejectedTestCount;
        }

        public double getErrorDelta()
        {
            return Math.Abs(getFAR() - getFRR());
        }

        public static ErrorRatePair operator +(ErrorRatePair e1, ErrorRatePair e2)
        {
            return new ErrorRatePair(e1.getFalseAcceptedCount() + e2.getFalseAcceptedCount(), e1.getTotalAcceptedTestCount() + e2.getTotalAcceptedTestCount(), e1.getFalseRejectedCount() + e2.getFalseRejectedCount(), e1.getTotalRejectedTestCount() + e2.getTotalRejectedTestCount());
        }
    }
}
