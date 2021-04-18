using UnityEngine;
using UnityEngine.UI;

public class SavedPopulationView : MonoBehaviour
{
    public Button Button;
    public Text BtnText;
    
    private PopulationData PopData;
    private SpeciesEvolver EvolutionManager;

    public void Initialise(PopulationData popData, SpeciesEvolver em)
    {
        EvolutionManager = em;
        PopData = popData;
        BtnText.text = popData.SaveName;
    }

    private void Load()
    {
        // EvolutionManager.LoadGeneration(PopData);
    }

    private void Start()
    {
        Button.onClick.AddListener(Load);
    }
}
