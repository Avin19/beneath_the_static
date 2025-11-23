using System;
using System.Collections.Generic;

[Serializable]
public class PulsePatternLibrary
{
    public PulseLibraryMeta meta;
    public List<PulseLevelData> levels;
}

[Serializable]
public class PulseLibraryMeta
{
    public string game;
    public string description;

    public PulseUnits units;
}

[Serializable]
public class PulseUnits
{
    public int dot_ms;
    public int dash_ms;
    public int inter_element_gap_ms;
    public int inter_pattern_gap_ms;
    public int recommended_input_window_ms;
}

[Serializable]
public class PulseLevelData
{
    public int level;
    public string name;
    public List<string> pattern_symbols;
    public List<int> pattern_timing_ms_sequence;
    public string notes;

    public string example_image; // optional
}
