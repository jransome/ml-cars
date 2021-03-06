using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RansomeCorp.AI.NeuralNet
{
    public class NeuralNetwork
    {
        public event Action<List<double>, List<INeuron>, int> OnLayerFeedForward = delegate { };

        public readonly Dna Dna;
        public readonly List<List<INeuron>> Layers = new List<List<INeuron>>();

        public NeuralNetwork(Dna dna, Func<List<double>, ActivationType, INeuron> neuronFactory = null)
        {
            neuronFactory ??= (List<double> a, ActivationType b) => new Neuron(a, b);
            Dna = dna;

            int neuronCounter = 0;
            int weightIndex = 0;
            for (int outputsIndex = 1; outputsIndex < dna.OutputsPerLayer.Count; outputsIndex++)
            {
                List<INeuron> layer = new List<INeuron>();
                int nNeurons = dna.OutputsPerLayer[outputsIndex];
                int weightsPerNeuron = dna.OutputsPerLayer[outputsIndex - 1] + 1; // +1 for bias
                for (int neuronIndex = 0; neuronIndex < nNeurons; neuronIndex++)
                {
                    List<double> weights = dna.WeightsAndBiases.Skip(weightIndex).Take(weightsPerNeuron).ToList();
                    ActivationType activation = (ActivationType)dna.ActivationIndexes[neuronCounter];
                    layer.Add(neuronFactory(weights, activation));
                    weightIndex += weightsPerNeuron;
                    neuronCounter++;
                }
                Layers.Add(layer);
            }
        }

        public List<double> Think(List<double> inputs)
        {
            if (inputs.Count != Dna.OutputsPerLayer[0])
                throw new System.ArgumentException($"Network received {inputs.Count} inputs, but expected {Dna.OutputsPerLayer[0]}");

            return FeedForward(inputs, Layers);
        }

        private List<double> FeedForward(List<double> inputs, List<List<INeuron>> remainingLayers)
        {
            OnLayerFeedForward(inputs, remainingLayers.First(), Dna.OutputsPerLayer.Count - remainingLayers.Count);

            List<double> outputs = remainingLayers
                .First()
                .ConvertAll((neuron) => neuron.Compute(inputs));

            if (remainingLayers.Count == 1)
                return outputs;

            return FeedForward(outputs, remainingLayers.Skip(1).ToList());
        }
    }
}
