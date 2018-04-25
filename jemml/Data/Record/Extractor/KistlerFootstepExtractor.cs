using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jemml.Data.Record.Extractor
{
    public class KistlerFootstepExtractor : SampleExtractor
    {
        private int forceStepStartThreshold;
        private int silentIntervalsToEndStep;
        private int minimumStepSize;
        private int keepStep;
        private int[] columnsToMonitor;

        protected KistlerFootstepExtractor(int forceStepStartThreshold, int silentIntervalsToEndStep, int minimumStepSize, int keepStep, Preprocessor predecessor, params int[] columnsToMonitor) : base(predecessor)
        {
            this.forceStepStartThreshold = forceStepStartThreshold;
            this.silentIntervalsToEndStep = silentIntervalsToEndStep;
            this.minimumStepSize = minimumStepSize;
            this.keepStep = keepStep;
            this.columnsToMonitor = columnsToMonitor;
        }

        /**
         * forceStepStartThreshold - if any force in the monitored sensors exceeds this (+ or -) then include then include this record and the records that follow
         * silenetIntervalsToEndStep - if this many intervals pass without a force exceeding the forceStepStartThreshold then end the footstep record
         * minimumStepSize - drop the step recording if it is found to be smaller than this value (to avoid including noise)
         * keepStep - if there are multiple steps then keep the step provided here
         * columnsToMonitor - the index (starting at 0) of the columns we wish to monitor for the start/end of the footstep (if not provided we monitor all rows)
         */
        public KistlerFootstepExtractor(int forceStepStartThreshold, int silentIntervalsToEndStep, int minimumStepSize, int keepStep, params int[] columnsToMonitor) : this(forceStepStartThreshold, silentIntervalsToEndStep, minimumStepSize, keepStep, null, columnsToMonitor) { }

        protected Boolean HasColumnsToMonitor()
        {
            return columnsToMonitor.Length > 0;
        }

        protected override List<Tuple<double, double[]>> ExtractRows(ISample sample)
        {
            List<Tuple<double, double[]>> extractedRows = new List<Tuple<double, double[]>>();
            Boolean shouldRecordValue = false; // true if we should record the value
            int silentIntervalCount = 0; // count how many records have passed without a notable value
            int extractedSampleCount = 0; // count how many samples have been recorded
            foreach (Tuple<double, double[]> row in sample.GetDataRows())
            {
                int colNum = 0;
                Boolean hasExceededThreshold = false;
                foreach (int column in row.Item2)
                {
                    // if there were no columns provided to monitor then monitor all - otherwise check if the current column is being monitored
                    if (!HasColumnsToMonitor() || columnsToMonitor.Contains(colNum))
                    {
                        if (Math.Abs(column) > forceStepStartThreshold)
                        {
                            shouldRecordValue = true;
                            hasExceededThreshold = true;
                            silentIntervalCount = 0; // reset to silent interval counter now we have encountered a notable sample
                        }
                    }
                    colNum++;
                }

                if (!hasExceededThreshold)
                {
                    silentIntervalCount++; // if nothing exceeded the threshold increment the silent interval counter
                    if (silentIntervalCount > silentIntervalsToEndStep)
                    {
                        // if we previously were recorded and now are not then apply appropriate cleanup
                        if (shouldRecordValue)
                        {
                            // ignore unusually short pressure spikes
                            if (extractedRows.Count < 100)
                            {
                                extractedRows.Clear();
                                continue; // don't increment sample count if rows extracted
                            }

                            extractedSampleCount++; // we just finished recording a sample so increment the counter

                            // only keep records from the step we've marked to keep
                            if (extractedSampleCount == keepStep)
                            {
                                // return the desired sample
                                return extractedRows;
                            }
                            else
                            {
                                // clear and start the next recording
                                extractedRows.Clear();
                            }
                        }
                        shouldRecordValue = false;
                    }
                }

                if (shouldRecordValue)
                {
                    // extract this row
                    extractedRows.Add(new Tuple<double, double[]>(row.Item1, row.Item2));
                }
            }
            // no desirable samples could be found
            throw new ArgumentException("Could not find appropriate sample");
        }

        protected override bool RecalculateDuration()
        {
            return true;
        }

        protected override Preprocessor Copy(Preprocessor predecessor)
        {
            return new KistlerFootstepExtractor(forceStepStartThreshold, silentIntervalsToEndStep, minimumStepSize, keepStep, predecessor, columnsToMonitor);
        }
    }
}
