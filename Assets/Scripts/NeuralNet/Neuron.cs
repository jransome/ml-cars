using System.Collections.Generic;
using System.Linq;

namespace RansomeCorp.NeuralNet
{
    public static class Neuron
    {
        public static double Compute(List<double> inputValues, NeuronGene gene)
        {
            if (inputValues.Count != gene.Weights.Count)
                throw new System.ArgumentException("Neuron received the wrong number of inputs!");

            double dotProduct = inputValues.Zip(gene.Weights, (input, weight) => input * weight).Sum();
            return gene.ActivationFunction(dotProduct + gene.Bias);
        }
    }
}