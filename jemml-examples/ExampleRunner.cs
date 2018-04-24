using System;
using McMaster.Extensions.CommandLineUtils;

namespace jemml_examples
{
    public class ExampleRunner
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<ExampleRunner>(args);

        [Option(Description = "The input file", ShortName = "i", LongName = "inputFile")]
        public string InputFile { get; }

        [Option(Description = "The output file", ShortName = "o", LongName = "outputFile")]
        public string OutputFile { get; }

        private void OnExecute()
        {
            Console.WriteLine("input file {0}, output file {1}", InputFile, OutputFile);
            Console.ReadLine();
        }

        //{
        //    console.writeline("hello world!");
        //    console.writeline("number of arguments: {0}", args.length);
        //    console.readline();
        //}
    }
}
