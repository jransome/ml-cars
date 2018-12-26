using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private List<Layer> layers;
    private int layerWidth;

    public NeuralNetwork(DNA dna)
    {
        ReplaceDna(dna);
    }

    public void ReplaceDna(DNA dna)
    {
        layerWidth = dna.MaxNeuronsPerLayer;
        layers  = new List<Layer>();

        foreach (LayerGene layerGene in dna.LayerGenes)
            layers.Add(new Layer(layerGene));
    }

    public List<double> Calculate(List<double> inputs) => layers.Aggregate(inputs, (result, layer) => layer.FireNeurons(result));
}
