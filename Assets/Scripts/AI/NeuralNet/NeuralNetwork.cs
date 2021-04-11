using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace RansomeCorp.AI.NeuralNet
{
    public class NeuralNetwork
    {
        public readonly List<List<Neuron>> Layers;

        public NeuralNetwork(int inputs, int[] hiddenLayers, int outputs)
        {
            Layers = hiddenLayers
                .Concat(new int[] { outputs })
                .Select((int nNeurons, int index) =>
                    {
                        int inputsPerNeuron = index == 0 ? inputs : hiddenLayers[index - 1];
                        return new int[nNeurons].Select((_) => new Neuron(inputsPerNeuron)).ToList();
                    })
                .ToList();
        }

        public List<double> Think(List<double> inputs) => FeedForward(inputs, Layers);

        private List<double> FeedForward(List<double> inputs, List<List<Neuron>> remainingLayers)
        {
            List<double> outputs = remainingLayers
                .First()
                .Select((neuron) => neuron.Compute(inputs))
                .ToList();

            if (remainingLayers.Count == 1)
                return outputs;

            return FeedForward(outputs, remainingLayers.Skip(1).ToList());
        }
    }

    // public struct NeuronGene
    // {
    //     public readonly double Bias;
    //     public readonly List<double> Weights;
    //     public readonly Func<double, double> ActivationFunction;

    //     public NeuronGene(int nInputs, ActivationType activationType = ActivationType.TanH)
    //     {
    //         Bias = UnityEngine.Random.Range(-1f, 1f);
    //         ActivationFunction = Activation.Functions[activationType];
    //         Weights = new List<double>(new double[nInputs])
    //             .Select((_) => (double)UnityEngine.Random.Range(-1f, 1f)).ToList();
    //     }
    // }
}