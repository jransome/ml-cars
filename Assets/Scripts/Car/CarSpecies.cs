using RansomeCorp.AI.NeuralNet;
using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarSpecies : ScriptableObject
{
    public readonly static Dictionary<DnaHeritage, Color> LineageColours = new Dictionary<DnaHeritage, Color>()
    {
        { DnaHeritage.New, Color.yellow },
        { DnaHeritage.Unchanged, Color.grey },
        { DnaHeritage.Bred, Color.blue },
        { DnaHeritage.BredAndMutated, Color.green },
        { DnaHeritage.Mutated, Color.green },
    };

    [Header("General")]
    public GameObject PopulationPrefab = null;
    public Color SpeciesColour;

    [Header("Distance sensors")]
    public LayerMask SensorLayerMask = ~(1 << 9 | 1 << 2); // ignore layer 9 ('Car') and 2 ('ignore raycast' - for gates)
    public bool DrawSensors = false;
    public float[] SensorAngles = new float[] { 0f, 15f, -15f};
    public float SensorDistance = 150f;

    [Header("Neural network")]
    public const int Outputs = 3;
    public int Inputs { get; private set; } // computed dynamically
    public int[] HiddenLayersNeuronCount = new int[] { 1 };
    public bool HeterogeneousHiddenActivation;
    public ActivationType OutputLayerActivation = ActivationType.TanH;

    [Header("Evolution hyperparameters")]
    public int GenerationSize = 30;
    public float ProportionUnchanged = 0.05f;
    public float NewDnaRate = 0.05f;
    public float MutationProbability = 0.05f;
    public float MutationSeverity = 0.05f;
    public float ActivationMutationSeverity = 0.01f;
    public float CrossoverSeverity = 0.4f;
    public float ActivationCrossoverSeverity = 0.05f;

    [Header("Fitness hyperparameters")]
    public float MaxTimeToReachNextGateSecs = 5f;
    public float MaxLifeSpanSecs = 0f;

    private void OnEnable()
    {
        if (LayerMask.NameToLayer("Car") == -1) 
            Debug.LogError("The 'Car' layer wasn't set in the inspector");
        
        if (LayerMask.NameToLayer("Ignore Raycast") == -1) 
            Debug.LogError("The 'Ignore Raycast' layer wasn't set in the inspector");
        
        if (HiddenLayersNeuronCount.Length == 0)
            Debug.LogWarning("No hidden layers specified for species:" + this.name);

        if (SensorAngles.Length == 0)
            Debug.LogWarning("Sensor config specified for species:" + this.name);

        Inputs = SensorAngles.Length + 3; // +3 for physics sensors
    }
}
