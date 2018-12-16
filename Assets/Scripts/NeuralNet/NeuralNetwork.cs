using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private List<Layer> layers;

    public NeuralNetwork(int nInputs, int nOutputs, int nHiddenLayers)
    {
        // initialise layers list
        int totalLayers = nHiddenLayers + 2;
        layers = new List<Layer>(totalLayers);

        // input layer
        layers.Add(new Layer(true, nInputs, 1));

        for (int i = 1; i < nHiddenLayers + 1; i++)
        {
            int neuronsInPreviousLayer = layers[i - 1].Neurons.Count;
            layers.Add(new Layer(false, 5, neuronsInPreviousLayer));
        }

        // output layer
        layers[totalLayers] = new Layer(false, nOutputs, layers[totalLayers - 1].Neurons.Count); 
    }

    public List<double> Calculate(List<double> inputs) => layers.Aggregate(inputs, (result, layer) => layer.FireNeurons(result));
}
