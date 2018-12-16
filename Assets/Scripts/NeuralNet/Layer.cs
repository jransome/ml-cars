using System.Collections.Generic;
using System.Linq;

public class Layer
{
    public List<Neuron> Neurons { get; private set; }
    private int neuronInputs;

    public Layer(bool isPassiveLayer, int noOfNeurons, int inputsPerNeuron)
    {
        Neurons = new List<Neuron>(noOfNeurons);
        Neurons.ForEach(n => new Neuron(isPassiveLayer));
        neuronInputs = inputsPerNeuron;
    }

    public List<double> FireNeurons(List<double> inputs) => Neurons.Select(n => n.CalculateOutput(inputs)).ToList();

    public void SetNeuronWeights()
    {
        //Neurons.ForEach(n => n.SetWeights());
    }
}
