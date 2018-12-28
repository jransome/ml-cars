using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DnaOrigin
{
    IsNew,
    UnchangedFromLastGen,
    Bred,
    Mutated,
}

public class Dna
{
    public double WeightSum = 0;
    public readonly int NumInputs;
    public readonly int NumOutputs;
    public readonly int NumHiddenLayers;
    public readonly int MaxNeuronsPerLayer;

    public List<LayerGene> LayerGenes { get; private set; } = new List<LayerGene>();
    public DnaOrigin Origin { get; set; }

    public Dna(int nInputs, int nOutputs, int nHiddenLayers, int maxNeuronsPerLayer)
    {
        Origin = DnaOrigin.IsNew;
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
        return clone;
    }

    public void Splice(Dna other)
    {
        if (NumHiddenLayers != other.NumHiddenLayers)
            throw new System.ArgumentException("Tried to splice two NNs with different numbers of hidden layers!");

        LayerGenes = LayerGenes.Zip(other.LayerGenes, (otherL, ownL) => ownL.Splice(otherL)).ToList();
        Origin = DnaOrigin.Bred;
    }

    public void Mutate(float proportion)
    {
        proportion = Mathf.Clamp01(proportion); // proportion of neurons to be mutated
        foreach (LayerGene layerGene in LayerGenes)
            layerGene.Mutate(proportion);
        
        Origin = DnaOrigin.Mutated;
    }
}

public class LayerGene
{
    public readonly int MaxNeurons;

    public List<NeuronGene> NeuronGenes { get; set; } = new List<NeuronGene>();

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
}

public class NeuronGene
{
    public double[] weights { get; set; }
    public double bias { get; set; }

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
        double sum = weights.Aggregate((acc, x) => acc += x) + bias;
        if (mutationType <= 2)
        {
            // Mutate by selecting a random (non zero) weight and scaling it by +/-50%
            // If all weights were zero, then randomise one of them instead
            bool success = ScaleRandomWeight(1.5f);
            if (!success) RandomiseSingleWeight();
        }
        else if (mutationType <= 4)
        {
            // Mutate by selecting a random weight and replacing it with a new random number
            RandomiseSingleWeight();
        }
        else
        {
            // Mutate by randomising all weights
            RandomiseWeights();
        }
        if (sum == weights.Aggregate((acc, x) => acc += x) + bias) Debug.LogError("Mutation failed");
    }

    public void RandomiseWeights()
    {
        bias = Random.Range(-1f, 1f);
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);
    }

    private void RandomiseSingleWeight()
    {
        int weightIndex = Random.Range(-1, weights.Length); // -1 for the bias
        if (weightIndex == -1) 
            bias = Random.Range(-1f, 1f);
        else
            weights[weightIndex] = Random.Range(-1f, 1f);
    }

    private bool ScaleRandomWeight(float scalar)
    {
        List<int> nonZeroWeightIndicies = new List<int>();
        if (bias != 0) nonZeroWeightIndicies.Add(-1);
        for (int i = 0; i < weights.Length; i++)
            if (weights[i] != 0) nonZeroWeightIndicies.Add(i);

        if (nonZeroWeightIndicies.Count == 0) return false;
        else
        {
            float scale = scalar * Random.Range(0, 2) * 2 - 1;
            int randomNonZeroWeightIndex = nonZeroWeightIndicies[Random.Range(0, nonZeroWeightIndicies.Count)];
            if (randomNonZeroWeightIndex == -1) bias *= scale;
            else weights[randomNonZeroWeightIndex] *= scale;
            
            return true;
        }
    }

    private void SetWeights(double value)
    {
        bias = value;
        for (int i = 0; i < weights.Length; i++)
            weights[i] = value;
    }

    private static bool RandomBool() => Random.Range(0, 2) == 1 ? true : false; // Random.Range is max exclusive with ints
}
