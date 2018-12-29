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
    public double WeightSumFingerprint { get { return LayerGenes.Aggregate(0.0, (acc, x) => acc += x.WeightSumFingerprint); } }

    public Dna(int nInputs, int nOutputs, int nHiddenLayers, int maxNeuronsPerLayer)
    {
        Heritage = DnaHeritage.IsNew;
        NumInputs = nInputs;
        NumOutputs = nOutputs;
        MaxNeuronsPerLayer = maxNeuronsPerLayer;

        // hidden layers
        NumHiddenLayers = nHiddenLayers;
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

    public Dna Splice(Dna other)
    {
        if (NumHiddenLayers != other.NumHiddenLayers)
            throw new System.ArgumentException("Tried to splice two NNs with different numbers of hidden layers!");

        LayerGenes = LayerGenes.Zip(other.LayerGenes, (otherL, ownL) => ownL.Splice(otherL)).ToList();
        Heritage = DnaHeritage.Bred;
        return this;
    }

    public void Mutate(float proportion)
    {
        proportion = Mathf.Clamp01(proportion); // proportion of neurons to be mutated
        foreach (LayerGene layerGene in LayerGenes)
            layerGene.Mutate(proportion);
        
        Heritage = DnaHeritage.Mutated;
    }

    public bool IsEqual(Dna other) => !LayerGenes.Zip(other.LayerGenes, (own, otherGene) => own.IsEqual(otherGene)).Contains(false);
}

public class LayerGene
{
    public readonly int MaxNeurons;

    public List<NeuronGene> NeuronGenes { get; set; } = new List<NeuronGene>();
    public double WeightSumFingerprint { get { return NeuronGenes.Aggregate(0.0, (acc, x) => acc += x.WeightSumFingerprint); } }

    public LayerGene(int nNeurons, int inputsPerNeuron, bool isOutputLayer = false)
    {
        MaxNeurons = nNeurons;
        for (int i = 0; i < MaxNeurons; i++)
            NeuronGenes.Add(new NeuronGene(inputsPerNeuron, isOutputLayer));

        if (isOutputLayer == false) NeuronGenes[Random.Range(0, nNeurons)].RandomiseWeights(); // set 1 neuron as alive
    }

    public LayerGene Clone() 
    {
        LayerGene clone = (LayerGene)this.MemberwiseClone();
        clone.NeuronGenes = NeuronGenes.Select(neuronGene => neuronGene.Clone()).ToList();
        return clone;
    }

    public LayerGene Splice(LayerGene other)
    {
        if (MaxNeurons != other.MaxNeurons)
            throw new System.ArgumentException("Tried to splice layer with another layer of a different size!");

        NeuronGenes = NeuronGenes.Zip(other.NeuronGenes, (otherN, ownN) => ownN.Splice(otherN)).ToList();
        return this;
    }

    public void Mutate(float proportion)
    {
        foreach (NeuronGene neuronGene in NeuronGenes)
            if (Random.Range(0f, 1f) < proportion) neuronGene.Mutate();
    }

    public bool IsEqual(LayerGene other) => !NeuronGenes.Zip(other.NeuronGenes, (own, otherGene) => own.IsEqual(otherGene)).Contains(false);
}

public class NeuronGene
{
    public double[] weights { get; set; }
    public double bias { get; set; }
    public double WeightSumFingerprint { get; private set; }

    public NeuronGene(int nWeights, bool initialiseAsAlive)
    {
        weights = new double[nWeights];
        if (initialiseAsAlive)
            RandomiseWeights();
        else
            SetWeights(0);
    }

    public NeuronGene Clone() => (NeuronGene)this.MemberwiseClone();

    public NeuronGene Splice(NeuronGene other)
    {
        bias = RandomBool() ? other.bias : bias;
        weights = weights.Zip(other.weights, (otherW, ownW) => RandomBool() ? otherW : ownW).ToArray();
        return this;
    }

    public void Mutate()
    {
        int mutationType = Random.Range(0, 6);
        if (mutationType < 2 && (bias + weights.Sum() > 0))
        {
            // Mutate by scaling all weights by +/-50%
            ScaleWeights(1.5f);
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
        if (WeightSumFingerprint == newWeightSum) Debug.LogError("Mutation failed. Type: " + mutationType);
        WeightSumFingerprint = newWeightSum;
    }

    public void RandomiseWeights()
    {
        bias = Random.Range(-1f, 1f);
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);
    }

    public bool IsEqual(NeuronGene other)
    {
        if (bias != other.bias) return false;
        return weights.SequenceEqual(other.weights);
    }

    private void RandomiseSingleWeight()
    {
        int weightIndex = Random.Range(-1, weights.Length); // -1 for the bias
        if (weightIndex == -1) 
            bias = Random.Range(-1f, 1f);
        else
            weights[weightIndex] = Random.Range(-1f, 1f);
    }

    private void ScaleWeights(float scalar)
    {
        bias *= scalar;
        weights = weights.Select(x => x *= scalar).ToArray();
    }

    private void SetWeights(double value)
    {
        bias = value;
        for (int i = 0; i < weights.Length; i++)
            weights[i] = value;
    }

    private double CalculateWeightSumFingerprint() => weights.Select((w, i) => w + i).Sum() + bias;

    private static bool RandomBool() => Random.Range(0, 2) == 1 ? true : false; // Random.Range is max exclusive with ints
}
