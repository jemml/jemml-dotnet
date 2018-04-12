using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace jemml.Utilities
{
    /// <summary>
    /// A set of helper methods to assist with file IO
    /// </summary>
    public class FileHelpers
    {
        public static JsonSerializer Serializer = new JsonSerializer()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public static void WriteFile<T>(string file, T data)
        {
            WriteFile(file, (writer) =>
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jsonWriter, data);
                }
            });
        }

        public static void WriteFile(string file, Action<StreamWriter> transform)
        {
            using (FileStream fileStream = File.Open(file + ".gz", FileMode.Create))
            {
                using (GZipStream compress = new GZipStream(fileStream, CompressionMode.Compress))
                {
                    using (StreamWriter writer = new StreamWriter(compress))
                    {
                        transform.Invoke(writer);
                    }
                }
            }
        }

        public static T ReadFile<T>(string file)
        {
            return ReadFile(file, (reader) =>
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            });
        }

        public static T ReadFile<T>(string file, Func<StreamReader, T> transform)
        {
            string readFile = file.EndsWith(".gz") ? file : file + ".gz";

            using (FileStream fileStream = File.Open(readFile, FileMode.Open))
            {
                using (GZipStream decompress = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(decompress))
                    {
                        return transform.Invoke(reader);
                    }
                }
            }
        }
    }
}
