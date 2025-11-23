using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;

public class LevelPatternLoader : MonoBehaviour
{
    public WaveformGenerator waveformGenerator;
    public int levelToLoad = 1;

    string jsonPath;

    void Awake()
    {
        jsonPath = Path.Combine(Application.streamingAssetsPath, "level.json");
    }

    void Start()
    {
        LoadAndApply(levelToLoad);
    }

    public void LoadAndApply(int level)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("JSON not found at: " + jsonPath);
            return;
        }

        string json = File.ReadAllText(jsonPath);
        var root = JObject.Parse(json);
        var levels = root["levels"];

        foreach (var lev in levels)
        {
            if ((int)lev["level"] == level)
            {
                List<string> list = new List<string>();
                foreach (var s in lev["pattern_symbols"])
                    list.Add((string)s);

                waveformGenerator.BuildWaveformTexture(list);

                Debug.Log("Loaded level " + level + ": " + string.Join(", ", list));
                return;
            }
        }

        Debug.LogWarning("Level not found: " + level);
    }
}
