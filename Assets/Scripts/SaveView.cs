using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaveView : MonoBehaviour
{
    public God EvolutionManager;
    public InputField SaveName;
    public Button SaveButton;
    public GameObject SavedPopPrefab;
    public Transform SavedPopsPanel;

    private List<GameObject> ActiveSavedPopViews;
    
    private void Save()
    {
        // for testing
        // DnaStructure structure = new DnaStructure(2, 2, 2, 2);
        // List<Dna> TestGenes = new List<Dna> {
        //     new Dna(structure),
        //     new Dna(structure),
        //     new Dna(structure),
        // };
        // PopulationData data = new PopulationData(TestGenes, 1, SaveName.text);
        // Persistence.Save(data);
        Persistence.Save(new PopulationData(EvolutionManager.GenePool, EvolutionManager.GenerationCount, SaveName.text));

        ClearSaves();
        PopulateSaves();
    }

    private void PopulateSaves()
    {
        ActiveSavedPopViews = Persistence.GetSavedPopulations().Select(popData => 
        {
            GameObject go = Instantiate(SavedPopPrefab, SavedPopsPanel);
            go.GetComponent<SavedPopulationView>().Initialise(popData, EvolutionManager);
            return go;
        }).ToList();
    }

    private void ClearSaves()
    {
        foreach (GameObject view in ActiveSavedPopViews) Destroy(view);
    }

    private void Start()
    {
        SaveButton.onClick.AddListener(Save);
        PopulateSaves();
    }
}
