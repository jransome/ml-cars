using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Persistence
{
    private static readonly string saveDirectory = Application.persistentDataPath;
    private static readonly DirectoryInfo dirInfo = new DirectoryInfo(saveDirectory);

    public static List<PopulationData> GetSavedPopulations()
    {
        return dirInfo.GetFiles()
            .Select(f => ReadFile(Path.Combine(saveDirectory, f.Name)))
            .ToList();
    }

    public static void Save(PopulationData popData)
    {
        string filePath = Path.Combine(saveDirectory, popData.SaveName + ".json");
        FileStream fs = File.OpenWrite(filePath);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(JsonUtility.ToJson(popData, true));
        writer.Flush();
        fs.Close();
        Debug.Log("Population " + popData.SaveName + " saved to " + saveDirectory);
    }

    private static PopulationData ReadFile(string path)
    {
        StreamReader reader = new StreamReader(File.OpenRead(path));
        return JsonUtility.FromJson<PopulationData>(reader.ReadToEnd());
    }
}

[System.Serializable]
public struct PopulationData
{
    public string SaveName;
    public int GenerationNumber;
    public List<Dna> GenePool;
    public PopulationData(List<Dna> genePool, int generationNumber, string saveName = null)
    {
        SaveName = saveName == null ? "population_" + System.DateTime.Now.ToString("HHmmss_ddMMyyyy") : saveName;
        GenerationNumber = generationNumber;
        GenePool = genePool;
    }
}
