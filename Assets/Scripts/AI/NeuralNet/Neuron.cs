using System.Collections.Generic;
using System.Linq;
using System;

namespace RansomeCorp.AI.NeuralNet
{
    public interface INeuron
    {
        double Bias { get; }
        List<double> Weights { get; }
        Func<double, double> ActivationFunction { get; }
        double Compute(List<double> inputValues);
    }

    public class Neuron : INeuron
    {
        public double Bias { get; private set; }
        public List<double> Weights { get; private set; }
        public Func<double, double> ActivationFunction { get; private set; }

        public Neuron(List<double> weights, ActivationType activationType)
        {
            Bias = weights[0];
            Weights = weights.Skip(1).ToList();
            ActivationFunction = Activation.Functions[activationType];
        }

        public double Compute(List<double> inputValues)
        {
            if (inputValues.Count != Weights.Count)
                throw new System.ArgumentException("Neuron received the wrong number of inputs!");

            double dotProduct = inputValues.Zip(Weights, (input, weight) => input * weight).Sum();
            return ActivationFunction(dotProduct + Bias);
        }
    }
}