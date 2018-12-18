using System.Collections.Generic;
using System.Linq;

public class Neuron
{
    private double[] weights;
    private double bias = 1;

    public Neuron(int nInputs)
    {
        weights = Enumerable.Repeat((double)1, nInputs).ToArray();
    }

    public void SetWeights(double[] inputWeights, double biasValue)
    {
        weights = inputWeights;
        bias = biasValue;
    }

    public double CalculateOutput(List<double> inputValues)
    {
        if (inputValues.Count != weights.Length)
            throw new System.ArgumentException("Neuron received the wrong number of inputs!");

        double dotProduct = inputValues.Zip(weights, (input, weight) => input * weight).Sum();
        return ActivationFunction(dotProduct + bias);
    }

    private double ActivationFunction(double dotProductBias) => dotProductBias > 0 ? 1 : 0;
}
