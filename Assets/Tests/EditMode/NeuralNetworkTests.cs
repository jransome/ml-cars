using FluentAssertions;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NeuralNetworkTests
{
    [Test]
    public void NeuralNetworkStructureTests()
    {
        int nInputs = 5;
        int nOutputs = 3;
        int[] hiddenLayers = new int[] { 4, 3, 6 };

        NeuralNetwork neuralNetwork = new NeuralNetwork(nInputs, hiddenLayers, nOutputs);

        // Correct number of layers and neurons per layer
        neuralNetwork.Layers.Should().HaveCount(hiddenLayers.Count() + 1);
        neuralNetwork.Layers[0].Should().HaveCount(hiddenLayers[0]);
        neuralNetwork.Layers[1].Should().HaveCount(hiddenLayers[1]);
        neuralNetwork.Layers[2].Should().HaveCount(hiddenLayers[2]);
        neuralNetwork.Layers[3].Should().HaveCount(nOutputs);

        // Neuron assertions
        int hiddenNeuronCount = hiddenLayers.Aggregate((sum, neuronsPerLayer) => sum + neuronsPerLayer);
        IEnumerable<INeuron> allNeurons = neuralNetwork.Layers.SelectMany(n => n); // SelectMany = flatmap
        allNeurons.Should().HaveCount(hiddenNeuronCount + nOutputs);
        allNeurons.Should().OnlyHaveUniqueItems();
        neuralNetwork.Layers[0].Should().OnlyContain(n => n.Weights.Count() == nInputs, "neurons in hidden layer 0 had incorrect number of weight inputs");
        neuralNetwork.Layers[1].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[0], "neurons in hidden layer 1 had incorrect number of weight inputs");
        neuralNetwork.Layers[2].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[1], "neurons in hidden layer 2 had incorrect number of weight inputs");
        neuralNetwork.Layers[3].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[2], "neurons in output layer had incorrect number of weight inputs");
        allNeurons.Should().OnlyContain(n => n.Bias <= 1 && n.Bias >= -1, " all neurons have Bias values between -1 and 1");
    }

    // [Test]
    // public void NeuronTests()
    // {
    //     NeuronGene ng = new NeuronGene(3, ActivationType.Relu);
    //     List<double> inputs = new List<double>() { 3, 7, 2 };

    //     double neuronOutput = Neuron.Compute(inputs, ng);

    //     neuronOutput.Should().Be(3);
    // }
}
