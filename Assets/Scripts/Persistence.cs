using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Persistence
{
    private static readonly string saveDirectory = Application.persistentDataPath;
    private static readonly DirectoryInfo dirInfo = new DirectoryInfo(saveDirectory);

    public static List<GenerationSaveData> GetSavedPopulations()
    {
        List<GenerationSaveData> saves = dirInfo.GetFiles()
            .Where(f => f.Extension == ".json")
            .Select(f => ReadFile(Path.Combine(saveDirectory, f.Name)))
            .ToList();
        Debug.Log($"Found {saves.Count} saved populations in {saveDirectory}");
        return saves;
    }

    public static void Save(List<Generation> generationHistory, string saveName)
    {
        SaveCurrentGenerationData(new GenerationSaveData(generationHistory.Last(), saveName));
        SaveFitnessData(generationHistory, saveName);
    }

    private static void SaveCurrentGenerationData(GenerationSaveData saveData)
    {
        WriteToFile($"{saveData.SaveName}.json", JsonUtility.ToJson(saveData, true));
        Debug.Log($"Population data saved at {saveDirectory}");
    }

    private static void SaveFitnessData(List<Generation> generationHistory, string saveName)
    {
        string header = "Generation, Spawn Location Index, Total, Max, Average\n";
        StringBuilder csvString = new StringBuilder(header);

        foreach (Generation g in generationHistory)
        {
            csvString.AppendLine(string.Join(",", new string[5] {
                g.GenerationNumber.ToString(),
                g.SpawnLocationIndex.ToString(),
                g.PerformanceData.BestFitness.ToString("F"),
                g.PerformanceData.AverageFitness.ToString("F"),
                g.PerformanceData.TotalFitness.ToString("F"),
            }));
        }

        WriteToFile($"{saveName}_fitness.csv", csvString.ToString());
        Debug.Log($"Fitness data saved at {saveDirectory}");
    }

    private static void WriteToFile(string filename, string content)
    {
        string filePath = Path.Combine(saveDirectory, filename);
        using (StreamWriter writer = File.CreateText(filePath))
        {
            writer.Write(content);
        } // the streamwriter WILL be closed and flushed here, even if an exception is thrown.
    }

    private static GenerationSaveData ReadFile(string path)
    {
        StreamReader reader = new StreamReader(File.OpenRead(path));
        GenerationSaveData loadedSave = JsonUtility.FromJson<GenerationSaveData>(reader.ReadToEnd());

        if (loadedSave.FormatVersion != GenerationSaveData.CurrentFormatVersion)
            Debug.LogError("Loaded incompatible save file!");

        return loadedSave;
    }
}

[System.Serializable]
public class GenerationSaveData
{
    public static string CurrentFormatVersion = "0.1";

    public string FormatVersion;
    public string SaveName;
    public Generation Generation;
    public GenerationSaveData(Generation generation, string saveName)
    {
        FormatVersion = CurrentFormatVersion;
        SaveName = string.IsNullOrEmpty(saveName) ? $"evolution_run_{System.DateTime.Now.ToString("HHmmss_ddMMyyyy")}" : saveName;
        Generation = generation;
    }
}
