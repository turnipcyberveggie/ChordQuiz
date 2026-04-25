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

    // Semitone distance above C for each natural note
    private static readonly Dictionary<char, int> NaturalPitches = new()
    {
        ['C'] = 0, ['D'] = 2, ['E'] = 4, ['F'] = 5,
        ['G'] = 7, ['A'] = 9, ['B'] = 11
    };

    private static readonly char[] NoteLetterChars = ['C', 'D', 'E', 'F', 'G', 'A', 'B'];

    // Intervals (semitones from root) for each mode, indexed to match Modes[]
    private static readonly int[][] ModeIntervalPatterns =
    [
        [0, 2, 4, 5, 7, 9, 11], // Ionian
        [0, 2, 3, 5, 7, 9, 10], // Dorian
        [0, 1, 3, 5, 7, 8, 10], // Phrygian
        [0, 2, 4, 6, 7, 9, 11], // Lydian
        [0, 2, 4, 5, 7, 9, 10], // Mixolydian
        [0, 2, 3, 5, 7, 8, 10], // Aeolian
        [0, 1, 3, 5, 6, 8, 10], // Locrian
    ];

    /// <summary>
    /// Derives mode notes purely from interval arithmetic so any standard root works,
    /// including flat keys whose parent keys aren't in the 12-key map.
    /// May return double-flat notes (e.g. "Bbb") for the flattest modes in flat keys.
    /// </summary>
    public static string[] GetModeNotesByInterval(string root, int modeIndex)
    {
        char rootLetter = root[0];
        int rootPitch = NaturalPitches[rootLetter];
        for (int i = 1; i < root.Length; i++)
        {
            if (root[i] == '#') rootPitch++;
            else if (root[i] == 'b') rootPitch--;
        }
        rootPitch = ((rootPitch % 12) + 12) % 12;

        int rootLetterIdx = Array.IndexOf(NoteLetterChars, rootLetter);
        var intervals = ModeIntervalPatterns[modeIndex];
        var result = new string[7];

        for (int degree = 0; degree < 7; degree++)
        {
            int targetPitch = (rootPitch + intervals[degree]) % 12;
            char letter = NoteLetterChars[(rootLetterIdx + degree) % 7];
            int naturalPitch = NaturalPitches[letter];
            int diff = (targetPitch - naturalPitch + 12) % 12;
            string accidental = diff switch
            {
                0  => "",
                1  => "#",
                2  => "##",
                11 => "b",
                10 => "bb",
                _  => "?"
            };
            result[degree] = letter + accidental;
        }
        return result;
    }

    /// <summary>Returns the 7 correct triads for a mode whose root is the given note.</summary>
    public static Triad[] GetModeTriadsFromRoot(string root, int modeIndex)
    {
        var notes = GetModeNotesFromRoot(root, modeIndex);
        var qualities = ModeTriadPatterns[modeIndex];
        return [.. Enumerable.Range(0, 7).Select(i => new Triad(notes[i], qualities[i]))];
    }
}
