using System.Collections.Generic;
using System.Linq;

public class Neuron
{
    private bool isPassive;
    private double[] weights;
    private double bias;

    public Neuron(bool isPassiveNeuron = false)
    {
        isPassive = isPassiveNeuron;
    }

    public void SetWeights(double[] inputWeights, double biasValue)
    {
        weights = inputWeights;
        bias = biasValue;
    }

    public double CalculateOutput(List<double> inputValues)
    {
        if (isPassive) return inputValues[0];
        if (inputValues.Count != weights.Length)
            throw new System.ArgumentException("Neuron received the wrong number of inputs!");

        double dotProduct = inputValues.Zip(weights, (input, weight) => input * weight).Sum();
        return ActivationFunction(dotProduct + bias);
    }

    private double ActivationFunction(double dotProductBias) => dotProductBias > 0 ? 1 : 0;
}
