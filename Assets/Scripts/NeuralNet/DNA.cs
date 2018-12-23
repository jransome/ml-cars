using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DNA
{
    public readonly int NumInputs;
    public readonly int NumOutputs;
    public readonly int NumHiddenLayers;
    public readonly int MaxNeuronsPerLayer;

    public List<LayerGene> LayerGenes { get; private set; } = new List<LayerGene>();

    public DNA(int nInputs, int nOutputs, int nHiddenLayers, int maxNeuronsPerLayer)
    {
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

    public DNA Splice(DNA other)
    {
        if (NumHiddenLayers != other.NumHiddenLayers)
            throw new System.ArgumentException("Tried to splice two NNs with different numbers of hidden layers!");

        LayerGenes = LayerGenes.Zip(other.LayerGenes, (otherL, ownL) => ownL.Splice(otherL)).ToList();
        return this;
    }

    public void Mutate(float proportion)
    {
        proportion = Mathf.Clamp01(proportion); // proportion of neurons to be mutated
        foreach (LayerGene layerGene in LayerGenes)
            layerGene.Mutate(proportion);
    }
}

public class LayerGene
{
    public readonly int MaxNeurons;

    public List<NeuronGene> NeuronGenes { get; private set; } = new List<NeuronGene>();

    public LayerGene(int nNeurons, int inputsPerNeuron, bool isOutputLayer = false)
    {
        MaxNeurons = nNeurons;
        for (int i = 0; i < MaxNeurons; i++)
            NeuronGenes.Add(new NeuronGene(inputsPerNeuron, isOutputLayer));

        if (isOutputLayer == false) NeuronGenes[Random.Range(0, nNeurons)].RandomiseWeights(); // set 1 neuron as alive
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

    public NeuronGene Splice(NeuronGene other)
    {
        bias = RandomBool() ? other.bias : bias;
        weights = weights.Zip(other.weights, (otherW, ownW) => RandomBool() ? otherW : ownW).ToArray();
        return this;
    }

    public void Mutate()
    {
        int mutationType = Random.Range(0, 3);

        if (mutationType == 0)
        {
            // Mutate by selecting a random weight and scaling it by +/-25%
            List<double> allWeights = new List<double>(weights);
            allWeights.Add(bias);
            int randomWeightIndex = Random.Range(0, weights.Length + 1);
            allWeights[randomWeightIndex] = allWeights[randomWeightIndex] * 1.25f * Random.Range(0, 2) * 2 - 1;
        }
        else if (mutationType == 1)
        {
            // Mutate by selecting a random weight and replacing it with a new random number
            List<double> allWeights = new List<double>(weights);
            allWeights.Add(bias);
            allWeights[Random.Range(0, weights.Length + 1)] = Random.Range(-1f, 1f);
        }
        else
        {
            // Mutate by randomising all weights
            RandomiseWeights();
        }
    }

    public void RandomiseWeights()
    {
        bias = Random.Range(-1f, 1f);
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);
    }

    private void SetWeights(double value)
    {
        bias = value;
        for (int i = 0; i < weights.Length; i++)
            weights[i] = value;
    }

    private bool RandomBool() => Random.Range(0, 2) == 1 ? true : false; // Random.Range is max exclusive with ints
}
