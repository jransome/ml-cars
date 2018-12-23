using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DNA
{
    public readonly int NumHiddenLayers;
    public List<LayerGene> LayerGenes { get; set; }

    public DNA(int nInputs, int nOutputs, int nHiddenLayers, int maxNeuronsPerLayer)
    {
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

    // add mutate
}

public class LayerGene
{
    public List<NeuronGene> NeuronGenes = new List<NeuronGene>();
    public int MaxNeurons { get; set; }

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

    // add mutate
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
        List<double> allWeights = new List<double>(weights);
        allWeights.Add(bias);
        allWeights[Random.Range(0, weights.Length + 1)] = Random.Range(-1f, 1f);
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
