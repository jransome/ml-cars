using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    private List<Layer> layers;

    public NeuralNetwork(Dna dna)
    {
        ReplaceDna(dna);
    }

    public void ReplaceDna(Dna dna)
    {
        layers  = new List<Layer>();

        foreach (LayerGene layerGene in dna.LayerGenes)
            layers.Add(new Layer(layerGene));
    }

    public List<double> Calculate(List<double> inputs) => layers.Aggregate(inputs, (result, layer) => layer.FireNeurons(result));
}
