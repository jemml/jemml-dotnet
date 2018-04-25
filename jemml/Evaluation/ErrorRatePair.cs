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

        public int GetFalseAcceptedCount()
        {
            return falseAcceptedCount;
        }

        public int GetTotalAcceptedTestCount()
        {
            return totalAcceptedTestCount;
        }

        public int GetFalseRejectedCount()
        {
            return falseRejectedCount;
        }

        public int GetTotalRejectedTestCount()
        {
            return totalRejectedTestCount;
        }

        public double GetFAR()
        {
            return (double)falseAcceptedCount / totalAcceptedTestCount;
        }

        public double GetFRR()
        {
            return (double)falseRejectedCount / totalRejectedTestCount;
        }

        public double GetErrorDelta()
        {
            return Math.Abs(GetFAR() - GetFRR());
        }

        public static ErrorRatePair operator +(ErrorRatePair e1, ErrorRatePair e2)
        {
            return new ErrorRatePair(e1.GetFalseAcceptedCount() + e2.GetFalseAcceptedCount(), e1.GetTotalAcceptedTestCount() + e2.GetTotalAcceptedTestCount(), e1.GetFalseRejectedCount() + e2.GetFalseRejectedCount(), e1.GetTotalRejectedTestCount() + e2.GetTotalRejectedTestCount());
        }
    }
}
