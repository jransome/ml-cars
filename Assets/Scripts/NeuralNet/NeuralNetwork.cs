using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private List<Layer> layers;

    public NeuralNetwork(int nInputs, int nOutputs, int nHiddenLayers)
    {
        // initialise layers list
        layers = new List<Layer>();

        // hidden layers
        for (int i = 0; i < nHiddenLayers; i++)
        {
            int inputsPerNeuron = i == 0 ? nInputs : layers[i - 1].Neurons.Count;
            layers.Add(new Layer(5, inputsPerNeuron));
        }

        // output layer
        layers.Add(new Layer(nOutputs, layers.Last().Neurons.Count)); 
    }

    public List<double> Calculate(List<double> inputs) => layers.Aggregate(inputs, (result, layer) => layer.FireNeurons(result));
}
