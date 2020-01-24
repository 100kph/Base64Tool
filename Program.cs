namespace Base64Tool
{
    using CommandLine;
    using log4net;
    using System;
    using System.IO;
    using System.Reflection;
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        class Options
        {
            [Option('i', "input", Required = true, HelpText = "the file to encode/decode.")]
            public string InputFile { get; set; }

            [Value(0, MetaName = "outputFilePath", Required = true, HelpText = "path where you want the file to created.")]

            [Option('d', "decode", Required = false, HelpText = "do base64 decoding instead of encoding.")]
            public bool Decode { get; set; }

            [Option('o', "output", Required = true, HelpText = "destination/target file")]
            public string OutputFilePath { get; set; }
        }
        static void Main(string[] args)
        {
            LogHelper.Setup();

            // encode -i Base64Tool.deps.json -o test.out
            // decode -i test.out -o test.decoded.txt --decode

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    string inputFile = o.InputFile;
                    string outputFilePath = o.OutputFilePath;

                    if (!File.Exists(inputFile))
                    {
                        log.ErrorFormat($@"Cannot open input file ""{inputFile}""");
                        Environment.Exit(-1);
                    }

                    bool displayProgress = !Console.IsInputRedirected || !Console.IsOutputRedirected;

                    Base64Helper base64Helper = new Base64Helper(displayProgress);

                    if (o.Decode)
                    {
                        if (!base64Helper.DecodeFromBase64(inputFile, outputFilePath, out long processedBytes, out long readBytes))
                        {
                            log.ErrorFormat($@"Cannot write to file ""{outputFilePath}""");
                            Environment.Exit(-1);
                        }
                        else
                        {
                            log.InfoFormat($@"Decoded {readBytes} bytes. Wrote {processedBytes} bytes");
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        if (!base64Helper.EncodeToBase64(inputFile, outputFilePath, out long inputSize, out long outputSize))
                        {
                            log.ErrorFormat($@"Cannot write to file ""{outputFilePath}""");
                            Environment.Exit(-1);
                        }
                        else
                        {
                            log.InfoFormat($@"Encoded {inputSize} bytes. Wrote {outputSize} bytes");
                            Environment.Exit(0);
                        }
                    }
                })
                .WithNotParsed(o =>
                {
                    log.InfoFormat("Arguments not specified. Please use the --help command argument to find usage.");
                });
        }
    }
}
