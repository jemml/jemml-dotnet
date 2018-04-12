using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Record
{
    /// <summary>
    /// A sample represents a single record of data in a larger dataset, also see SampleSet
    /// </summary>
    public interface Sample
    {
        string GetIdentifier();

        bool IsImposter();

        int GetOrder();

        List<Tuple<double, double[]>> GetDataRows();

        int GetColumnCount();

        double[] GetDataRows(int column);

        double[] GetDimensions();

        int GetDimensionCount();

        double[] GetIntervals();

        double? GetDuration();

        T AcceptVisitor<T>(SampleVisitor<T> visitor);

        double[] GetAllValuesForColumns(int[] columns);
    }
}
