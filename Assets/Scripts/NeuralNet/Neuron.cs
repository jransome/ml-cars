using System.Collections.Generic;
using System.Linq;

public class Neuron
{
    private NeuronGene gene;

    public Neuron(NeuronGene neuronGene)
    {
        gene = neuronGene;
    }

    public double CalculateOutput(List<double> inputValues)
    {
        if (inputValues.Count != gene.weights.Length)
            throw new System.ArgumentException("Neuron received the wrong number of inputs!");

        double dotProduct = inputValues.Zip(gene.weights, (input, weight) => input * weight).Sum();
        return ActivationFunction(dotProduct + gene.bias);
    }

    private double ActivationFunction(double dotProductBias) => Activation.TanH(dotProductBias);
}
