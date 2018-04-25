using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jemml.Data.Record;

namespace jemml.Utilities
{
    public class AssertionHelpers
    {
        public static void NotNull(Object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException("Object cannot be null");
            }
        }

        public static void WithEqualNumberOfDimensions(List<ISample> samples)
        {
            int[] dimensions = samples.Select(sample => sample.GetDimensionCount()).Distinct().ToArray();
            if (dimensions.Length != 1)
            {
                throw new ArgumentException("All samples must have equal dimensions");
            }
        }
    }
}
