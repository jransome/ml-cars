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
    
    public static void Save(List<Dna> genePool)
    {
        string filePath = Path.Combine(saveDirectory, GenerateFileName());
        FileStream fs = File.OpenWrite(filePath);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(JsonUtility.ToJson(new PopulationData(genePool), true));
        writer.Flush();
        fs.Close();
    }

    private static PopulationData ReadFile(string path)
    {
        StreamReader reader = new StreamReader(File.OpenRead(path));
        return JsonUtility.FromJson<PopulationData>(reader.ReadToEnd());
    }

    private static string GenerateFileName() => "population_" + System.DateTime.Now.ToString("HHmmss_ddMMyyyy") + ".json";
}

[System.Serializable]
public struct PopulationData
{
    public readonly string Description;
    public readonly List<Dna> GenePool;
    public readonly DnaStructure DnaStructure;

    public PopulationData(List<Dna> genePool, string description = "")
    {
        Description = description;
        GenePool = genePool;
        DnaStructure = genePool[0].structure;
    }
}
