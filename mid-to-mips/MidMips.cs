using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace mid_to_mips;

public class MidMips {
    private readonly MidiFile midiFile;

    public MidMips(string filename) {
        midiFile = MidiFile.Read(filename);
    }

    public string ToMips() {
        // Get midi tempo map
        var tempoMap = midiFile.GetTempoMap();
        
        // Prepare MIPS program
        var mipsOutput = ".text\n.globl main\nmain:";
        
        // Get "Program Change Events" from Midi File
        var programChangeEvents = midiFile.GetTimedEvents()
            .Where(e => e.Event is ProgramChangeEvent);
        
        // Get notes of track
        foreach (var note in midiFile.GetNotes()) {
            // Get note info
            var noteDuration = note.LengthAs<MetricTimeSpan>(tempoMap).Milliseconds;
            var notePitch = note.NoteNumber;
            var instrument = 0;
            var absoluteTime = note.TimeAs<MetricTimeSpan>(tempoMap).Milliseconds;
            
            // Get instrument
            foreach (var timedEvent in programChangeEvents.Where(timedEvent => timedEvent is TimedEvent)) {
                var programChangeEventTime = timedEvent.TimeAs<MetricTimeSpan>(tempoMap).Milliseconds;
                var programChangeEvent = timedEvent.Event as ProgramChangeEvent;

                if (absoluteTime < programChangeEventTime) {
                    continue;
                };
                
                // If the note is after the iterated event then
                instrument = programChangeEvent!.ProgramNumber;
                break;
            }
            
            mipsOutput += $"\n\tli $v0 33\n\tli $a0 {notePitch}\n\tli $a1 {noteDuration}\n\tli $a2 {instrument}\n\tli $a3 127\n\tsyscall\n";
        }
        
        // Append the exit syscall
        mipsOutput += "\n\tli $v0 10\n\tsyscall\n";

        // Return the output
        return mipsOutput;
    }
}