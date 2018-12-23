using System.Collections.Generic;
using System.Linq;

public class Layer
{
    public List<Neuron> Neurons { get; private set; } = new List<Neuron>();

    public Layer(LayerGene gene)
    {
        foreach (NeuronGene neuronGene in gene.NeuronGenes)
            Neurons.Add(new Neuron(neuronGene));
    }

    public List<double> FireNeurons(List<double> inputs) => Neurons.Select(n => n.CalculateOutput(inputs)).ToList();
}
