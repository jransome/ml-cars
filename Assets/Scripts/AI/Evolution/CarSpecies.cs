using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using UnityEngine;

// TODO: where does this file live/split into composable SOs?
[CreateAssetMenu]
public class CarSpecies : ScriptableObject
{
    public readonly static Dictionary<DnaHeritage, Color> LineageColours = new Dictionary<DnaHeritage, Color>()
    {
        { DnaHeritage.New, Color.yellow },
        { DnaHeritage.Elite, Color.grey },
        { DnaHeritage.Offspring, Color.blue },
        { DnaHeritage.MutatedOffspring, Color.green },
        { DnaHeritage.MutatedElite, Color.magenta },
    };

    [Header("General")]
    public GameObject PopulationPrefab = null;
    public Color SpeciesColour;

    [Header("Distance sensors")]
    public LayerMask SensorLayerMask = ~(1 << 9 | 1 << 2); // ignore layer 9 ('Car') and 2 ('ignore raycast' - for gates)
    public bool DrawSensors = false;
    public float[] SensorAngles = new float[] { 0f, 15f, -15f };
    public float SensorDistance = 150f;

    [Header("Physics sensors")] // values to normalise velocity sensors by, ie. when velocity value equals velocity normal, nn input = 1
    public float VelocityNormalX = 10f;
    public float VelocityNormalZ = 20f;

    [Header("Neural network")]
    // When the activation function is non-linear, then a two-layer neural network can be proven to be a universal function approximator
    public const int Outputs = 2;
    public int Inputs { get; private set; } // computed dynamically
    public int[] HiddenLayersNeuronCount = new int[] { 1 }; 
    public bool HeterogeneousHiddenActivation;
    public ActivationType OutputLayerActivation = ActivationType.TanH;

    [Header("Evolution hyperparameters")]
    public int GenerationSize = 30;
    public float ProportionUnchanged = 0.05f;
    public float ProportionMutatantsOfUnchanged = 0.15f;
    public float NewDnaRate = 0.05f;
    public float OffspringMutationProbability = 0.5f;
    public float MutationSeverity = 0.2f;
    public float ActivationMutationSeverity = 0.01f;
    public int CrossoverPasses = 5; // how many times to cross parent dna in offspring production. higher numbers = closer to uniform crossover
    public bool IncludeActivationCrossover = false;

    [Header("Fitness hyperparameters")]
    public float MaxTimeToReachNextGateSecs = 5f;
    public float MaxLifeSpanSecs = 0f;
    public int GateCrossedReward = 4;
    public int OptimalDirectionReward = 8;
    public int OptimalPositionReward = 6;
    public int MaxPositionDifferenceTolerance = 15;

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
