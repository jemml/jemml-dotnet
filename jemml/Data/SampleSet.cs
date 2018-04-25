using System;
using System.Collections.Generic;
using System.Linq;
using jemml.Data.Record;
using jemml.Data.Subset;
using jemml.Data.Transform;
using jemml.Data.Transform.DimensionalityReduction;
using jemml.Utilities;
using Newtonsoft.Json;

namespace jemml.Data
{
    /// <summary>
    /// A sample set represents a processed dataset and the preprocessors used to arrive at its current state, as well as providing a means to process it further.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SampleSet<T> : List<T> where T : ISample
    {
        // cross validated preprocessor head values
        protected Dictionary<int, Preprocessor> preprocessorHeads;
        protected int? trainingSize;
        public const int DEFAULT_PREPROCESSOR_KEY = 0;

        protected SampleSet(List<T> samples, Dictionary<int, Preprocessor> preprocessorHeads = null, int? trainingSize = null) : base(samples)
        {
            this.preprocessorHeads = preprocessorHeads ?? new Dictionary<int, Preprocessor>();
            this.trainingSize = trainingSize;
        }

        public SampleSet()
        {
            preprocessorHeads = new Dictionary<int, Preprocessor>();
            trainingSize = null;
        }

        protected SampleSet(Preprocessor preprocessorHead)
        {
            this.preprocessorHeads = new Dictionary<int, Preprocessor>(); // default to a single cross validation
            preprocessorHeads.Add(DEFAULT_PREPROCESSOR_KEY, preprocessorHead);
        }

        protected SampleSet(Dictionary<int, Preprocessor> preprocessorHeads, int? trainingSize)
        {
            this.preprocessorHeads = preprocessorHeads;
            this.trainingSize = trainingSize;
        }

        public bool IsCrossValidatedSampleSet()
        {
            return this is SampleSet<CrossValidatedSample>;
        }

        public SampleSet<CrossValidatedSample> AsCrossValidatedSet(int trainingSize, int xValidationStart, int xValidationLength)
        {
            // don't allow an already cross validated set to be re-cross validated
            if (IsCrossValidatedSampleSet())
            {
                throw new Exception("Error: you cannot re-apply cross validation to an already cross-validated sample set");
            }
            // split current preprocessor into a per cross validation preprocessor
            Dictionary<int, Preprocessor> preprocessorCrossValidations = Enumerable.Range(xValidationStart, xValidationLength)
                .Select(x => new { x, preprocessor = (preprocessorHeads.ContainsKey(DEFAULT_PREPROCESSOR_KEY)) ? preprocessorHeads[DEFAULT_PREPROCESSOR_KEY].Copy() : null }).ToDictionary(validation => validation.x, validation => validation.preprocessor);
            // create a copy of the current SampleSet for each cross validation with an assigned cross validation
            SampleSet<CrossValidatedSample> xValidatedSampleSet = new SampleSet<CrossValidatedSample>(preprocessorCrossValidations, trainingSize);
            for (int x = xValidationStart; x < (xValidationStart + xValidationLength); x++)
            {
                xValidatedSampleSet.AddRange(this.Select(sample => new CrossValidatedSample(sample.GetIdentifier(), sample.IsImposter(), sample.GetDataRows(), sample.GetOrder(), x, sample.GetDuration())).ToList());
            }
            return xValidatedSampleSet;
        }

        public SampleSet<T> ApplySampleExtractor(SampleExtractor extractor)
        {
            // return transformed sample set
            return Apply<SampleExtractor>(extractor, (sample, trainedPreprocessor) => (T)trainedPreprocessor.Extract(sample));
        }

        /**
         * Get a a subset of the raw sample set with a smaller grouping of extracted columns (i.e. a column for average, minimum contour, etc)
         */
        public SampleSet<T> GenerateSampleSubsets(params ISubsetExtractor[] subsetExtractors)
        {
            if (subsetExtractors.Length < 1)
            {
                throw new ArgumentException("No subset extractors provided");
            }
            SampleSubsetExtractor subsetExtractor = new SampleSubsetExtractor(subsetExtractors);
            return Apply<SampleSubsetExtractor>(subsetExtractor, (sample, trainedPreprocessor) => (T)trainedPreprocessor.Extract(sample));
        }

        public SampleSet<T> ApplyTransformation(SampleTransform transform)
        {
            return Apply<SampleTransform>(transform, (sample, trainedPreprocessor) => (T)trainedPreprocessor.ApplyTransform(sample));
        }

        public SampleSet<T> ApplyDimensionalityReduction(DimensionalityReduction reduction)
        {
            return Apply<DimensionalityReduction>(reduction, (sample, trainedPreprocessor) => (T)trainedPreprocessor.ApplyReduction(sample));
        }

        /**
         * Use if you wish to apply the current preprocessor as well as any associated parent preprocessors otherwise use ApplyTransformation/ApplyDimensionalityReduction/...
         */
        public SampleSet<T> ApplyPreprocessor(Preprocessor preprocessor)
        {
            return Apply<Preprocessor>(preprocessor, (sample, trainedPreprocessor) => (T)trainedPreprocessor.Evaluate(sample));
        }

        protected SampleSet<T> Apply<P>(P preprocessor, Func<T, P, T> sampleProcessing) where P : Preprocessor
        {
            int[] xValidations = preprocessorHeads.Select(preprocessorHead => preprocessorHead.Key).ToArray();
            int xValidationStart = xValidations.Length > 0 ? xValidations.Min() : 0;
            int xValidationLength = xValidations.Length > 0 ? xValidations.Length : 1;
            return ApplyCrossValidated(preprocessor, sampleProcessing, xValidationStart, xValidationLength);
        }

        protected SampleSet<T> ApplyCrossValidated<P>(P preprocessor, Func<T, P, T> sampleProcessing, int xValidationStart, int xValidationLength) where P : Preprocessor
        {
            // you need to train your Trainable preprocessor externally if you aren't using cross validation (cross validation will apply its own training)
            if (!IsCrossValidatedSampleSet() && preprocessor is ITrainable && !((ITrainable)preprocessor).IsTrained())
            {
                throw new ArgumentException("Trainable preprocessor must be trained for non-cross validated sets");
            }

            Preprocessor copy = preprocessor.Copy();
            // perform preprocessor training in parallel
            // TODO - include imposterTrainingSize
            ParallelQuery<int> crossValidationRange = Enumerable.Range(xValidationStart, xValidationLength).AsParallel();
            Dictionary<int, Preprocessor> trainedPreprocessors = crossValidationRange.Select(x => new { x, preprocessorHead = preprocessor.Copy() })
                .Select(p => new { p.x, trainablePreprocessor = p.preprocessorHead as ITrainable, p.preprocessorHead })
                .Select(t => new { t.x, preprocessorHead = t.trainablePreprocessor != null && IsCrossValidatedSampleSet() ? t.trainablePreprocessor.Train<P>(SampleSetHelpers.GetSampleSetTrainingSamples(this, trainingSize.Value, t.x)) : t.preprocessorHead }) // only do training when cross validated
                .Select(trained => new { trained.x, preprocessorHead = trained.preprocessorHead.SetPredecessor(preprocessorHeads.ContainsKey(trained.x) ? preprocessorHeads[trained.x] : null) }) // set the current preprocessor as the predecessor to the newly applied one for initializing the new SampleSet
                .ToDictionary(trained => trained.x, trained => trained.preprocessorHead);

            // apply preprocessor head to each cross validation (or total sample set if this is not a cross validated set)
            SampleSet<T> preprocessedSampleSet = new SampleSet<T>(trainedPreprocessors, trainingSize);
            if (IsCrossValidatedSampleSet())
            {
                preprocessedSampleSet.AddRange(crossValidationRange
                    .SelectMany(x => SampleSetHelpers.GetCrossValidation(this as SampleSet<CrossValidatedSample>, x).Cast<T>().Select(sample => sampleProcessing.Invoke(sample, (P)trainedPreprocessors[x]))));
            }
            else
            {
                preprocessedSampleSet.AddRange(this.Select(sample => sampleProcessing.Invoke(sample, (P)trainedPreprocessors[DEFAULT_PREPROCESSOR_KEY])));
            }
            return preprocessedSampleSet;
        }

        public Dictionary<int, Preprocessor> GetPreprocessors()
        {
            return preprocessorHeads; // all preprocessors applied in the generation of this sample set (use recursive GetPredecessor() to get back to head)
        }

        public List<ISample> AsSampleList()
        {
            return new List<ISample>(this.Cast<ISample>());
        }

        public void WriteDataToCSV(string sampleFile)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sampleFile))
            {
                foreach (T sample in this)
                {
                    file.WriteLine(String.Join(",", sample.GetDimensions()));
                }
            }
        }

        public void SaveSampleSet(string sampleFile)
        {
            FileHelpers.WriteFile(sampleFile, (writer) =>
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    // Add meta to first line
                    FileHelpers.Serializer.Serialize(jsonWriter, new SampleSetMeta(trainingSize, preprocessorHeads));

                    // add each sample in this collection (per line)
                    FileHelpers.Serializer.Serialize(jsonWriter, this);
                }
            });
        }

        public void SavePreprocessor(string preprocessorFile, int crossValidation = DEFAULT_PREPROCESSOR_KEY)
        {
            FileHelpers.WriteFile<Preprocessor>(preprocessorFile, preprocessorHeads[crossValidation]);
        }

        public void SavePreprocessors(string preprocessorFile)
        {
            FileHelpers.WriteFile<Dictionary<int, Preprocessor>>(preprocessorFile, preprocessorHeads);
        }

        public static Preprocessor ReadPreprocessor(string preprocessorFile)
        {
            return ReadJson<Preprocessor>(preprocessorFile);
        }

        public static Dictionary<int, Preprocessor> ReadPreprocessors(string preprocessorFile)
        {
            return ReadJson<Dictionary<int, Preprocessor>>(preprocessorFile);
        }

        protected static P ReadJson<P>(string preprocessorFile)
        {
            return FileHelpers.ReadFile<P>(preprocessorFile);
        }

        public static SampleSet<T> ReadSampleSet(string sampleFile, bool includePreprocessors = true)
        {
            return FileHelpers.ReadFile<SampleSet<T>>(sampleFile, (reader) =>
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.SupportMultipleContent = true;
                    jsonReader.Read();
                    SampleSetMeta meta = FileHelpers.Serializer.Deserialize<SampleSetMeta>(jsonReader);

                    jsonReader.Read();
                    List<T> samples = FileHelpers.Serializer.Deserialize<List<T>>(jsonReader);

                    if (includePreprocessors)
                    {
                        return new SampleSet<T>(samples, meta.PreprocessorHeads, meta.TrainingSize);
                    }
                    else
                    {
                        return new SampleSet<T>(samples, trainingSize: meta.TrainingSize);
                    }
                }
            });
        }

        protected class SampleSetMeta
        {
            public int? TrainingSize { get; private set; }
            public Dictionary<int, Preprocessor> PreprocessorHeads { get; private set; }

            public SampleSetMeta(int? trainingSize, Dictionary<int, Preprocessor> preprocessorHeads)
            {
                this.TrainingSize = trainingSize;
                this.PreprocessorHeads = preprocessorHeads;
            }
        }
    }
}
