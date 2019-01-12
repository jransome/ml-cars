using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Persistence : MonoBehaviour
{
    string lastSaved;
    private void Start()
    {
        var d = new Dna(5, 5,5,5);
        Save(d);
        Read();


    }


    private void Read()
    {
        StreamReader reader = new StreamReader(File.OpenRead(lastSaved));
        string contents = reader.ReadToEnd();

        // Debug.Log(contents);
        Dna savedD = JsonUtility.FromJson<Dna>(contents);
        Debug.Log(savedD.Fitness);
    }

    private void Save(Dna d)
    {
        string data = JsonUtility.ToJson(d, true);
        string filePath = Path.Combine(Application.persistentDataPath, "population_" + System.DateTime.Now.ToString("HHmmss_ddMMyyyy") + ".json");
        lastSaved = filePath;
        FileStream fs = File.OpenWrite(filePath);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(data);
        writer.Flush();
        fs.Close();
    }
}

// public class PopulationData
// {
//     public float 
//     public List<Dna> GenePool;
// }
