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

        public Neuron(int nInputs, ActivationType activationType = ActivationType.TanH)
        {
            Bias = UnityEngine.Random.Range(-1f, 1f);
            ActivationFunction = Activation.Functions[activationType];
            Weights = new List<double>(new double[nInputs])
                .Select((_) => (double)UnityEngine.Random.Range(-1f, 1f)).ToList();
        }

        public Neuron(double bias, List<double> weights, ActivationType activationType = ActivationType.TanH) =>
            (Bias, Weights, ActivationFunction) = (bias, weights, Activation.Functions[activationType]);

        public double Compute(List<double> inputValues)
        {
            if (inputValues.Count != Weights.Count)
                throw new System.ArgumentException("Neuron received the wrong number of inputs!");

            double dotProduct = inputValues.Zip(Weights, (input, weight) => input * weight).Sum();
            return ActivationFunction(dotProduct + Bias);
        }
    }
}