using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveView : MonoBehaviour
{
    public InputField SaveName;
    public Button SaveButton;
    public God EvolutionManager;
    
    private void Save()
    {
        // for testing
        // DnaStructure structure = new DnaStructure(2, 2, 2, 2);
        // List<Dna> TestGenes = new List<Dna> {
        //     new Dna(structure),
        //     new Dna(structure),
        //     new Dna(structure),
        // };
        // PopulationData data = new PopulationData(TestGenes, SaveName.text);
        // Persistence.Save(data);
        
        Persistence.Save(new PopulationData(EvolutionManager.GenePool, SaveName.text));
    }

    private void Start()
    {
        SaveButton.onClick.AddListener(Save);
    }
}
