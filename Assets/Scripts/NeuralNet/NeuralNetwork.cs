using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private List<Layer> layers = new List<Layer>();
    private int layerWidth;

    public NeuralNetwork(DNA dna)
    {
        layerWidth = dna.MaxNeuronsPerLayer;

        foreach (LayerGene layerGene in dna.LayerGenes)
            layers.Add(new Layer(layerGene));
    }

    public List<double> Calculate(List<double> inputs) => layers.Aggregate(inputs, (result, layer) => layer.FireNeurons(result));
}
