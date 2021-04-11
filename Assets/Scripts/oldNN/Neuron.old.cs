using System.Collections.Generic;
using System.Linq;
using System;

public class Neuron
{
    private NeuronGene gene;

    public Neuron(NeuronGene neuronGene)
    {
        gene = neuronGene;
    }

    public double CalculateOutput(List<double> inputValues)
    {
        if (inputValues.Count != gene.Weights.Length)
            throw new System.ArgumentException("Neuron received the wrong number of inputs!");

        double dotProduct = inputValues.Zip(gene.Weights, (input, weight) => input * weight).Sum();
        return TanH(dotProduct + gene.Bias);
    }

    static double TanH(double input) => Math.Tanh(input);
}
