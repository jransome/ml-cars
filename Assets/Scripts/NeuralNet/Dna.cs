using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DnaHeritage
{
    IsNew,
    UnchangedFromLastGen,
    Bred,
    Mutated,
}

public class Dna
{
    public readonly int NumInputs;
    public readonly int NumOutputs;
    public readonly int NumHiddenLayers;
    public readonly int MaxNeuronsPerLayer;

    public List<LayerGene> LayerGenes { get; private set; } = new List<LayerGene>();
    public DnaHeritage Heritage { get; set; }
    public float Fitness { get; set; }
    public double WeightSumFingerprint { get { return LayerGenes.Aggregate(0.0, (acc, x) => acc += x.WeightSumFingerprint); } }

    public event System.Action SelectedForBreeding = delegate { };

    public Dna(int nInputs, int nOutputs, int nHiddenLayers, int maxNeuronsPerLayer, bool splicing = false)
    {
        Heritage = splicing ? DnaHeritage.Bred : DnaHeritage.IsNew;
        Fitness = 0f;
        NumInputs = nInputs;
        NumOutputs = nOutputs;
        MaxNeuronsPerLayer = maxNeuronsPerLayer;
        NumHiddenLayers = nHiddenLayers;
        if (splicing) return;

        // hidden layers
        for (int i = 0; i < NumHiddenLayers; i++)
        {
            int inputsPerNeuron = i == 0 ? nInputs : LayerGenes[i - 1].MaxNeurons;
            LayerGenes.Add(new LayerGene(maxNeuronsPerLayer, inputsPerNeuron));
        }

        // output layer
        LayerGenes.Add(new LayerGene(nOutputs, LayerGenes.Last().MaxNeurons, true));
    }

    public Dna Clone() 
    {
        Dna clone = (Dna)this.MemberwiseClone();
        clone.LayerGenes = LayerGenes.Select(layerGene => layerGene.Clone()).ToList();
        clone.Heritage = DnaHeritage.UnchangedFromLastGen;
        return clone;
    }

    public Dna[] Splice(Dna other)
    {
        if (NumHiddenLayers != other.NumHiddenLayers)
            throw new System.ArgumentException("Tried to splice two NNs with different numbers of hidden layers!");

        Dna[] children = new Dna[2] {
            new Dna(NumInputs, NumOutputs, NumHiddenLayers, MaxNeuronsPerLayer, true),
            new Dna(NumInputs, NumOutputs, NumHiddenLayers, MaxNeuronsPerLayer, true),
        };

        for (int i = 0; i < LayerGenes.Count; i++)
        {
            LayerGene[] layerOffspring = LayerGenes[i].Splice(other.LayerGenes[i]);
            children[0].LayerGenes.Add(layerOffspring[0]);
            children[1].LayerGenes.Add(layerOffspring[1]);
        }
        
        SelectedForBreeding();
        return children;
    }

    public void Mutate(float proportion)
    {
        proportion = Mathf.Clamp01(proportion); // proportion of neurons to be mutated
        int nLayersToMutate = Mathf.CeilToInt(LayerGenes.Count * proportion);
        if (nLayersToMutate == 0) return;
        for (int i = 0; i < nLayersToMutate; i++)
            LayerGenes[Random.Range(0, LayerGenes.Count)].Mutate(proportion);
        
        Heritage = DnaHeritage.Mutated;
    }

    public bool IsEqual(Dna other) => !LayerGenes.Zip(other.LayerGenes, (own, otherGene) => own.IsEqual(otherGene)).Contains(false);
}

public class LayerGene
{
    public readonly int MaxNeurons;

    public List<NeuronGene> NeuronGenes { get; set; } = new List<NeuronGene>();
    public double WeightSumFingerprint { get { return NeuronGenes.Aggregate(0.0, (acc, x) => acc += x.WeightSumFingerprint); } }

    public LayerGene(int nNeurons, int inputsPerNeuron, bool allNeuronsAlive = false)
    {
        MaxNeurons = nNeurons;
        int initialAliveNeuron = Random.Range(0, nNeurons);
        for (int i = 0; i < MaxNeurons; i++)
            NeuronGenes.Add(new NeuronGene(inputsPerNeuron, allNeuronsAlive || i == initialAliveNeuron));
    }

    private LayerGene(int nNeurons) => MaxNeurons = nNeurons;

    public LayerGene Clone() 
    {
        LayerGene clone = (LayerGene)this.MemberwiseClone();
        clone.NeuronGenes = NeuronGenes.Select(neuronGene => neuronGene.Clone()).ToList();
        return clone;
    }

    public LayerGene[] Splice(LayerGene other)
    {
        if (MaxNeurons != other.MaxNeurons)
            throw new System.ArgumentException("Tried to splice layer with another layer of a different size!");

        LayerGene[] children = new LayerGene[2] {
            new LayerGene(MaxNeurons),
            new LayerGene(MaxNeurons),
        };

        for (int i = 0; i < MaxNeurons; i++)
        {
            NeuronGene[] neuronOffspring = NeuronGenes[i].Splice(other.NeuronGenes[i]);
            children[0].NeuronGenes.Add(neuronOffspring[0]);
            children[1].NeuronGenes.Add(neuronOffspring[1]);
        }

        return children;
    }

    public void Mutate(float proportion)
    {
        int nNeuronsToMutate = Mathf.CeilToInt(MaxNeurons * proportion);
        if (nNeuronsToMutate == 0) return;
        for (int i = 0; i < nNeuronsToMutate; i++)
            NeuronGenes[Random.Range(0, MaxNeurons)].Mutate();
    }

    public bool IsEqual(LayerGene other) => !NeuronGenes.Zip(other.NeuronGenes, (own, otherGene) => own.IsEqual(otherGene)).Contains(false);
}

public class NeuronGene
{
    public double[] Weights { get; set; }
    public double Bias { get; set; }
    public double WeightSumFingerprint { get; private set; }

    public NeuronGene(int nWeights, bool initialiseAsAlive)
    {
        Weights = new double[nWeights];
        if (initialiseAsAlive)
            RandomiseWeights();
        else
            SetWeights(0);
    }

    private NeuronGene(int nWeights, double bias)
    {
        Weights = new double[nWeights];
        Bias = bias;
    }

    public NeuronGene Clone() => (NeuronGene)this.MemberwiseClone();

    public NeuronGene[] Splice(NeuronGene other)
    {
        if (Weights.Length != other.Weights.Length)
            throw new System.ArgumentException("Tried to splice neuron with another neuron with a different number of weights!");

        NeuronGene[] children = new NeuronGene[2] { 
            new NeuronGene(Weights.Length, Bias),
            new NeuronGene(Weights.Length, other.Bias),
        };

        for (int i = 0; i < Weights.Length; i++)
        {
            int random = Random.Range(0, 2);
            children[0].Weights[i] = random == 0 ? Weights[i] : other.Weights[i];
            children[1].Weights[i] = random == 0 ? other.Weights[i] : Weights[i];
        }

        return children;
    }

    public void Mutate()
    {
        int mutationType = Random.Range(0, 6);
        if (mutationType < 2 && (Bias + Weights.Sum() > 0))
        {
            // Mutate by scaling all weights by up to +/-50%
            float scale = Random.Range(-0.5f, 0.5f);
            ScaleWeights(1 + scale);
        }
        else if (mutationType < 4)
        {
            // Mutate by selecting a random weight and replacing it with a new random number
            RandomiseSingleWeight();
        }
        else
        {
            // Mutate by randomising all weights
            RandomiseWeights();
        }
        double newWeightSum = CalculateWeightSumFingerprint();
        if (WeightSumFingerprint == newWeightSum) Debug.LogError("Neuron mutation failed. Type: " + mutationType);
        WeightSumFingerprint = newWeightSum;
    }

    public void RandomiseWeights()
    {
        Bias = Random.Range(-1f, 1f);
        for (int i = 0; i < Weights.Length; i++)
            Weights[i] = Random.Range(-1f, 1f);
    }

    public bool IsEqual(NeuronGene other)
    {
        if (Bias != other.Bias) return false;
        return Weights.SequenceEqual(other.Weights);
    }

    private void RandomiseSingleWeight()
    {
        int weightIndex = Random.Range(-1, Weights.Length); // -1 for the bias
        if (weightIndex == -1) 
            Bias = Random.Range(-1f, 1f);
        else
            Weights[weightIndex] = Random.Range(-1f, 1f);
    }

    private void ScaleWeights(float factor)
    {
        Bias *= factor;
        Weights = Weights.Select(x => x *= factor).ToArray();
    }

    private void SetWeights(double value)
    {
        Bias = value;
        for (int i = 0; i < Weights.Length; i++)
            Weights[i] = value;
    }

    private double CalculateWeightSumFingerprint() => Weights.Select((w, i) => w + i).Sum() + Bias;

    // private static bool RandomBool() => Random.Range(0, 2) == 1 ? true : false; // Random.Range is max exclusive with ints
}
