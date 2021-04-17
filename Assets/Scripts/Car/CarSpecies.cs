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
    public LayerMask IgnoredLayers = 1 << 9; // 9 should be the value of the 'Car' layer in the inspector
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
    [SerializeField] private int generationSize = 30; // workaround for immutability
    public int GenerationSize { get; private set; }
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
        
        if (HiddenLayersNeuronCount.Length == 0)
            Debug.LogWarning("No hidden layers specified!");

        GenerationSize = generationSize;
        Inputs = SensorAngles.Length + 3; // +3 for physics sensors
    }
}
