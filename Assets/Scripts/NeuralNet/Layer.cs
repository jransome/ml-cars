using System.Collections.Generic;
using System.Linq;

public class Layer
{
    public List<Neuron> Neurons { get; private set; }

    public Layer(int noOfNeurons, int inputsPerNeuron)
    {
        Neurons = new List<Neuron>();
        for (int i = 0; i < noOfNeurons; i++)
            Neurons.Add(new Neuron(inputsPerNeuron));
    }

    public List<double> FireNeurons(List<double> inputs) => Neurons.Select(n => n.CalculateOutput(inputs)).ToList();
}
