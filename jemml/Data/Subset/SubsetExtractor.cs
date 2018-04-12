using System;
using System.Collections.Generic;
using System.Text;
using jemml.Data.Record;

namespace jemml.Data.Subset
{
    public interface SubsetExtractor
    {
        List<double> Extract(Sample sample);
    }
}
