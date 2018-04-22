using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace jemml.Utilities
{
    public class DataHelpers
    {
        public static double[,] Convert(double[][] source)
        {
            double[,] result = new double[source.Length, source[0].Length];
            for (int i = 0; i < source.Length; i++)
            {
                for (int k = 0; k < source[0].Length; k++)
                {
                    result[i, k] = source[i][k];
                }
            }
            return result;
        }

        public static T GetFieldValue<T>(object obj, string field)
        {
            return (T)obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }

        public static void SetFieldValue<T>(object obj, string field, T value)
        {
            obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        }
    }
}
