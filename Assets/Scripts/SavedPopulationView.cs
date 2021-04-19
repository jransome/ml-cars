using UnityEngine;
using UnityEngine.UI;

public class SavedPopulationView : MonoBehaviour
{
    public Button Button;
    public Text BtnText;
    
    private PopulationData popData;
    private SpeciesEvolver evolver;

    public void Initialise(PopulationData populationData, SpeciesEvolver se)
    {
        evolver = se;
        popData = populationData;
        BtnText.text = populationData.SaveName;
    }

    private void Load()
    {
        evolver.LoadGeneration(popData);
    }

    private void Start()
    {
        Button.onClick.AddListener(Load);
    }
}
