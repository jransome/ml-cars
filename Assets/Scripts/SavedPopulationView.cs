using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavedPopulationView : MonoBehaviour
{
    public Button Button;
    public Text BtnText;
    
    private PopulationData PopData;
    private God EvolutionManager;

    public void Initialise(PopulationData popData, God em)
    {
        EvolutionManager = em;
        PopData = popData;
        BtnText.text = popData.SaveName;
    }

    private void Load()
    {
        EvolutionManager.LoadGeneration(PopData);
    }

    private void Start()
    {
        Button.onClick.AddListener(Load);
    }
}
