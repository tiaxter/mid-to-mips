using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace mid_to_mips;

public class MidMips {
    private readonly MidiFile _midiFile;

    public MidMips(string filename) {
        _midiFile = MidiFile.Read(filename);
    }

    public string ToMips() {
        // Get midi tempo map
        var tempoMap = _midiFile.GetTempoMap();
        
        // Prepare MIPS program
        var mipsOutput = ".text\n.globl main\nmain:";
        
        // Get notes
        var midiNotes = _midiFile.GetNotes();
        
        // Last played notes time
        int? lastPlayedNote = null;
        
        foreach (var note in midiNotes) {
            // Get note info
            var noteDuration = (int) note.LengthAs<MetricTimeSpan>(tempoMap).TotalMilliseconds;
            var notePitch = note.NoteNumber;
            var noteName = note.NoteName.ToString().Replace("Sharp", "#");
            var startTime = (int) note.GetTimedNoteOnEvent().TimeAs<MetricTimeSpan>(tempoMap).TotalMilliseconds;
            var endTime = (int) note.GetTimedNoteOffEvent().TimeAs<MetricTimeSpan>(tempoMap).TotalMilliseconds;

            int? delay;
            if (lastPlayedNote != null && (delay = startTime - lastPlayedNote) > 0) {
                var delayToSeconds = TimeSpan.FromMilliseconds((double) delay).TotalSeconds;
                mipsOutput += $"\n\t# Sleep for {delayToSeconds} seconds\n\tli $v0 32\n\tli $a0 {delay}\n";
            }

            lastPlayedNote = endTime;
            
            mipsOutput += $"\n\t# Note {noteName}\n\tli $v0 33\n\tli $a0 {notePitch}\n\tli $a1 {noteDuration}\n\tli $a3 127\n\tsyscall\n";
        }

        // Append the exit syscall
        mipsOutput += "\n\tli $v0 10\n\tsyscall\n";

        // Return the output
        return mipsOutput;
    }
}