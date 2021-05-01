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

    public static List<SaveData> GetSavedPopulations()
    {
        List<SaveData> saves = dirInfo.GetFiles()
            .Select(f => ReadFile(Path.Combine(saveDirectory, f.Name)))
            .ToList();
        Debug.Log($"Found {saves.Count} saved populations in {saveDirectory}");
        return saves;
    }

    public static void Save(List<Generation> generationHistory, string saveName)
    {
        SaveData saveData = new SaveData(generationHistory, saveName);
        SavePopulationData(saveData);
        SaveFitnessData(saveData);
    }

    private static void SavePopulationData(SaveData saveData)
    {
        WriteToFile($"{saveData.SaveName}.json", JsonUtility.ToJson(saveData, true));
        Debug.Log($"Population data saved at {saveDirectory}");
    }

    private static void SaveFitnessData(SaveData saveData)
    {
        string header = "Generation, Spawn Location Index, Total, Max, Average\n";
        StringBuilder csvString = new StringBuilder(header);

        foreach (Generation g in saveData.GenerationHistory)
        {
            csvString.AppendLine(string.Join(",", new string[5] {
                g.GenerationNumber.ToString(),
                g.SpawnLocationIndex.ToString(),
                g.PerformanceData.TotalFitness.ToString("F"),
                g.PerformanceData.BestFitness.ToString("F"),
                g.PerformanceData.AverageFitness.ToString("F"),
            }));
        }

        WriteToFile($"{saveData.SaveName}_fitness.csv", csvString.ToString());
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

    private static SaveData ReadFile(string path)
    {
        StreamReader reader = new StreamReader(File.OpenRead(path));
        SaveData loadedSave = JsonUtility.FromJson<SaveData>(reader.ReadToEnd());

        if (loadedSave.FormatVersion != SaveData.CurrentFormatVersion)
            Debug.LogError("Loaded incompatible save file!");

        return loadedSave;
    }
}

[System.Serializable]
public class SaveData
{
    public static string CurrentFormatVersion = "0.1";

    public string FormatVersion;
    public string SaveName;
    public List<Generation> GenerationHistory;
    public SaveData(List<Generation> generationHistory, string saveName = null)
    {
        saveName ??= $"evolution_run_{System.DateTime.Now.ToString("HHmmss_ddMMyyyy")}";

        FormatVersion = CurrentFormatVersion;
        SaveName = saveName;
        GenerationHistory = generationHistory;
    }
}
