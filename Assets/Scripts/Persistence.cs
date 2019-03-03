using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Persistence
{
    private static string saveDirectory = Application.persistentDataPath;
    private static DirectoryInfo dirInfo = new DirectoryInfo(saveDirectory);

    public static List<PopulationData> GetSavedPopulations() => dirInfo.GetFiles().Select(f => ReadFile(Path.Combine(saveDirectory, f.Name))).ToList();

    public static void Save(PopulationData popData)
    {
        string filePath = Path.Combine(saveDirectory, popData.SaveName + ".json");
        FileStream fs = File.OpenWrite(filePath);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(JsonUtility.ToJson(popData, true));
        writer.Flush();
        fs.Close();
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
    public DnaStructure DnaStructure;

    public PopulationData(List<Dna> genePool, int generationNumber = 1, string saveName = null)
    {
        SaveName = saveName == null ? "population_" + System.DateTime.Now.ToString("HHmmss_ddMMyyyy") : saveName;
        GenerationNumber = generationNumber;
        GenePool = genePool;
        DnaStructure = genePool[0].structure;
    }
}
