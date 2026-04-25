namespace ChordQuiz.Models;

public enum TriadQuality { Major, Minor, Diminished }

public record ModeInfo(string Name, int ParentKeyDegree);

public record Triad(string Note, TriadQuality Quality)
{
    public override string ToString() => $"{Note} {Quality}";
}

public static class MusicTheory
{
    // Keys in circle-of-fifths order (sharps then flats)
    public static readonly string[] Keys =
        ["C", "G", "D", "A", "E", "B", "F#", "Db", "Ab", "Eb", "Bb", "F"];

    // The seven modes derived from the major scale
    public static readonly ModeInfo[] Modes =
    [
        new("Ionian",     0),
        new("Dorian",     1),
        new("Phrygian",   2),
        new("Lydian",     3),
        new("Mixolydian", 4),
        new("Aeolian",    5),
        new("Locrian",    6),
    ];

    // The 7 diatonic notes of each major key's scale
    private static readonly Dictionary<string, string[]> KeyNoteMap = new()
    {
        ["C"]  = ["C",  "D",  "E",  "F",  "G",  "A",  "B" ],
        ["G"]  = ["G",  "A",  "B",  "C",  "D",  "E",  "F#"],
        ["D"]  = ["D",  "E",  "F#", "G",  "A",  "B",  "C#"],
        ["A"]  = ["A",  "B",  "C#", "D",  "E",  "F#", "G#"],
        ["E"]  = ["E",  "F#", "G#", "A",  "B",  "C#", "D#"],
        ["B"]  = ["B",  "C#", "D#", "E",  "F#", "G#", "A#"],
        ["F#"] = ["F#", "G#", "A#", "B",  "C#", "D#", "E#"],
        ["Db"] = ["Db", "Eb", "F",  "Gb", "Ab", "Bb", "C" ],
        ["Ab"] = ["Ab", "Bb", "C",  "Db", "Eb", "F",  "G" ],
        ["Eb"] = ["Eb", "F",  "G",  "Ab", "Bb", "C",  "D" ],
        ["Bb"] = ["Bb", "C",  "D",  "Eb", "F",  "G",  "A" ],
        ["F"]  = ["F",  "G",  "A",  "Bb", "C",  "D",  "E" ],
    };

    // Triad quality for each of the 7 degrees in each mode.
    // The pattern is the same 7 qualities from the parent major scale,
    // just rotated to start at the mode's root degree.
    //
    // Parent major scale pattern: Maj, min, min, Maj, Maj, min, dim
    //
    // Mode[0] Ionian     (starts on degree 1): Maj min min Maj Maj min dim
    // Mode[1] Dorian     (starts on degree 2): min min Maj Maj min dim Maj
    // Mode[2] Phrygian   (starts on degree 3): min Maj Maj min dim Maj min
    // Mode[3] Lydian     (starts on degree 4): Maj Maj min dim Maj min min
    // Mode[4] Mixolydian (starts on degree 5): Maj min dim Maj min min Maj
    // Mode[5] Aeolian    (starts on degree 6): min dim Maj min min Maj Maj
    // Mode[6] Locrian    (starts on degree 7): dim Maj min min Maj Maj min
    private static readonly TriadQuality[][] ModeTriadPatterns =
    [
        [TriadQuality.Major,      TriadQuality.Minor,      TriadQuality.Minor,  TriadQuality.Major,      TriadQuality.Major, TriadQuality.Minor,      TriadQuality.Diminished],
        [TriadQuality.Minor,      TriadQuality.Minor,      TriadQuality.Major,  TriadQuality.Major,      TriadQuality.Minor, TriadQuality.Diminished, TriadQuality.Major     ],
        [TriadQuality.Minor,      TriadQuality.Major,      TriadQuality.Major,  TriadQuality.Minor,      TriadQuality.Diminished, TriadQuality.Major,  TriadQuality.Minor    ],
        [TriadQuality.Major,      TriadQuality.Major,      TriadQuality.Minor,  TriadQuality.Diminished, TriadQuality.Major, TriadQuality.Minor,      TriadQuality.Minor     ],
        [TriadQuality.Major,      TriadQuality.Minor,      TriadQuality.Diminished, TriadQuality.Major,  TriadQuality.Minor, TriadQuality.Minor,      TriadQuality.Major     ],
        [TriadQuality.Minor,      TriadQuality.Diminished, TriadQuality.Major,  TriadQuality.Minor,      TriadQuality.Minor, TriadQuality.Major,      TriadQuality.Major     ],
        [TriadQuality.Diminished, TriadQuality.Major,      TriadQuality.Minor,  TriadQuality.Minor,      TriadQuality.Major, TriadQuality.Major,      TriadQuality.Minor     ],
    ];

    /// <summary>Returns the 7 notes of the major scale for the given key.</summary>
    public static string[] GetKeyNotes(string key) => KeyNoteMap[key];

    /// <summary>
    /// Returns the 7 note names that make up the given mode in the given key.
    /// Ionian starts on degree 1, Dorian on degree 2, etc.
    /// </summary>
    public static string[] GetModeNotes(string key, int modeIndex)
    {
        var keyNotes = KeyNoteMap[key];
        return [.. Enumerable.Range(0, 7).Select(i => keyNotes[(modeIndex + i) % 7])];
    }

    /// <summary>Returns the 7 correct triads for the given mode in the given key.</summary>
    public static Triad[] GetModeTriads(string key, int modeIndex)
    {
        var notes = GetModeNotes(key, modeIndex);
        var qualities = ModeTriadPatterns[modeIndex];
        return [.. Enumerable.Range(0, 7).Select(i => new Triad(notes[i], qualities[i]))];
    }

    /// <summary>Returns the 12 valid modal roots for a given mode index, derived from the 12 parent keys.</summary>
    public static string[] GetAvailableRoots(int modeIndex) =>
        [.. Keys.Select(k => KeyNoteMap[k][modeIndex])];

    /// <summary>Returns the 7 notes for a mode whose root is the given note.</summary>
    public static string[] GetModeNotesFromRoot(string root, int modeIndex)
    {
        var parentKey = Keys.FirstOrDefault(k => KeyNoteMap[k][modeIndex] == root)
            ?? throw new ArgumentException($"No parent key contains '{root}' as mode degree {modeIndex}");
        return GetModeNotes(parentKey, modeIndex);
    }

    /// <summary>Returns roots that are valid for all 7 modes given the 12-key system.</summary>
    public static string[] GetValidRootsForAllModes() =>
        [.. Keys.Where(root =>
            Enumerable.Range(0, 7).All(mi =>
                Keys.Any(k => KeyNoteMap[k][mi] == root)))];

    /// <summary>Returns the 7 correct triads for a mode whose root is the given note.</summary>
    public static Triad[] GetModeTriadsFromRoot(string root, int modeIndex)
    {
        var notes = GetModeNotesFromRoot(root, modeIndex);
        var qualities = ModeTriadPatterns[modeIndex];
        return [.. Enumerable.Range(0, 7).Select(i => new Triad(notes[i], qualities[i]))];
    }
}
