// See https://aka.ms/new-console-template for more information

using mid_to_mips;

// Get command line args
var commandLineArgs = Environment.GetCommandLineArgs();
CommandLineHandler();

// Get input and output file
var inputPath = commandLineArgs[2];
var outputPath = commandLineArgs[3];
// Convert midi to mips
var mipsOutput = new MidMips(inputPath).ToMips();
// Write the mips content
File.WriteAllText(outputPath, mipsOutput);

// region: Functions
void CommandLineHandler() {
    const string helpMessage = "usage: mid-to-mips [options] input_file output_file\noptions:\n-i, --input     Specify input file";

    if (commandLineArgs.Length == 4 && (commandLineArgs[1] == "-i" || commandLineArgs[1] == "--input")) {
        return;
    }
    
    Console.WriteLine(helpMessage);
    Environment.Exit(0);
}
// endregion