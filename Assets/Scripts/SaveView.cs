using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaveView : MonoBehaviour
{
    public SpeciesEvolver SpeciesEvolver;
    public InputField SaveName;
    public Button SaveButton;
    public GameObject SavedPopPrefab;
    public Transform SavedPopsPanel;

    private List<GameObject> ActiveSavedPopViews;

    private void Save()
    {
        PopulationData data = new PopulationData(
            SpeciesEvolver.GenerationPool.ConvertAll(b => b.Dna),
            SpeciesEvolver.GenerationCount,
            SaveName.text
        );
        Persistence.Save(data);

        ClearSaveViews();
        PopulateSaveViews();
    }

    private void PopulateSaveViews()
    {
        ActiveSavedPopViews = Persistence.GetSavedPopulations().Select(popData =>
        {
            GameObject go = Instantiate(SavedPopPrefab, SavedPopsPanel);
            go.GetComponent<SavedPopulationView>().Initialise(popData, SpeciesEvolver);
            return go;
        }).ToList();
    }

    private void ClearSaveViews()
    {
        foreach (GameObject view in ActiveSavedPopViews) Destroy(view);
    }

    private void Start()
    {
        SaveButton.onClick.AddListener(Save);
        PopulateSaveViews();
    }
}
