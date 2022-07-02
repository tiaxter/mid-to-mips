// See https://aka.ms/new-console-template for more information

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

// Get command line args
var commandLineArgs = Environment.GetCommandLineArgs();
commandLineHandler();

// Get input and output file
var inputPath = commandLineArgs[2];
var outputPath = commandLineArgs[3];
var midiFile = MidiFile.Read(inputPath);

// Prepare MIPS program
var mipsOutput = @".text
.globl main

main:
";

var tempoMap = midiFile.GetTempoMap();

// For each notes
foreach (var note in midiFile.GetNotes()) {
    // Get duration, note pitch and note channel
    var duration = note.LengthAs<MetricTimeSpan>(tempoMap);
    var noteNumber = note.NoteNumber;
    var channel = note.Channel;

    // Convert it to MIPS syscall
    mipsOutput += noteToMips(noteNumber, channel, duration);
}

// Append the exit syscall
mipsOutput += @"
        # exit
        li $v0 10
        syscall
";

File.WriteAllText(outputPath, mipsOutput);


// region: Functions
string noteToMips(SevenBitNumber noteNumber, FourBitNumber channel, MetricTimeSpan duration) {
    return @$"
        li $v0 33
        li $a0 {noteNumber}
        li $a1 {duration.Milliseconds}
        li $a3 127
        syscall

    ";
}

void commandLineHandler() {
    var helpMessage = "usage: mid-to-mips [options] input_file output_file\noptions:\n-i, --input     Specify input file";

    if (commandLineArgs.Length == 4 && (commandLineArgs[1] == "-i" || commandLineArgs[1] == "--input")) {
        return;
    }
    
    Console.WriteLine(helpMessage);
    Environment.Exit(0);
}
// endregion