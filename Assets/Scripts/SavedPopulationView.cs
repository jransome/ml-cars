using UnityEngine;
using UnityEngine.UI;
using System;

public class SavedPopulationView : MonoBehaviour
{
    public Button DeleteButton;
    public Button LoadButton;
    public Text LoadButtonText;

    private GenerationSaveData saveData;
    private SpeciesEvolver evolver;
    private Action onDeleted; // used to refresh parent

    public void Initialise(GenerationSaveData populationData, SpeciesEvolver se, Action onDeleteSave)
    {
        evolver = se;
        saveData = populationData;
        LoadButtonText.text = populationData.SaveName;
        onDeleted = onDeleteSave;
    }

    private void Load()
    {
        evolver.LoadGeneration(saveData.Generation);
    }

    private void Delete()
    {
        Persistence.Delete(saveData.SaveName);
        onDeleted();
    }

    private void Start()
    {
        LoadButton.onClick.AddListener(Load);
        DeleteButton.onClick.AddListener(Delete);
    }
}
