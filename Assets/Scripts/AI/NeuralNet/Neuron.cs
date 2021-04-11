using System.Collections.Generic;
using System.Linq;
using System;

namespace RansomeCorp.AI.NeuralNet
{
    public class Neuron
    {
        public readonly double Bias;
        public readonly List<double> Weights;
        public readonly Func<double, double> ActivationFunction;

        public Neuron(int nInputs, ActivationType activationType = ActivationType.TanH)
        {
            Bias = UnityEngine.Random.Range(-1f, 1f);
            ActivationFunction = Activation.Functions[activationType];
            Weights = new List<double>(new double[nInputs])
                .Select((_) => (double)UnityEngine.Random.Range(-1f, 1f)).ToList();
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