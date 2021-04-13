using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class NeuralNetworkTests
{
    [Test]
    public void NeuralNetworkStructureTests()
    {
        // Arrange
        const int nInputs = 5;
        const int nOutputs = 3;
        const int outputLayerActivationIndex = 0;
        int[] hiddenLayers = new int[] { 4, 3, 6 };
        Dna dna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        NeuralNetwork neuralNetwork = new NeuralNetwork(dna);

        // Assert
        // Correct number of layers and neurons per layer
        neuralNetwork.Layers.Should().HaveCount(hiddenLayers.Count() + 1);
        neuralNetwork.Layers[0].Should().HaveCount(hiddenLayers[0]);
        neuralNetwork.Layers[1].Should().HaveCount(hiddenLayers[1]);
        neuralNetwork.Layers[2].Should().HaveCount(hiddenLayers[2]);
        neuralNetwork.Layers[3].Should().HaveCount(nOutputs);

        // Neurons and weight counts
        IEnumerable<INeuron> allNeurons = neuralNetwork.Layers.SelectMany(n => n); // SelectMany = flatmap
        int expectedNumberOfNeurons = hiddenLayers[0] + hiddenLayers[1] + hiddenLayers[2] + nOutputs;
        allNeurons.Should().HaveCount(expectedNumberOfNeurons);
        allNeurons.Should().OnlyHaveUniqueItems();
        neuralNetwork.Layers[0].Should().OnlyContain(n => n.Weights.Count() == nInputs, "neurons in hidden layer 0 had incorrect number of weight inputs");
        neuralNetwork.Layers[1].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[0], "neurons in hidden layer 1 had incorrect number of weight inputs");
        neuralNetwork.Layers[2].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[1], "neurons in hidden layer 2 had incorrect number of weight inputs");
        neuralNetwork.Layers[3].Should().OnlyContain(n => n.Weights.Count() == hiddenLayers[2], "neurons in output layer had incorrect number of weight inputs");
        allNeurons.Should().OnlyContain(n => n.Bias <= 1 && n.Bias >= -1, "all neurons did not have Bias values between -1 and 1");

        // Neuron weight values
        List<double> reconstructedWeightsAndBiases = new List<double>();
        foreach (var layer in neuralNetwork.Layers)
        {
            foreach (var neuron in layer)
            {
                dna.WeightsAndBiases.Should().ContainInOrder(neuron.Weights);
                reconstructedWeightsAndBiases.Add(neuron.Bias);
                reconstructedWeightsAndBiases.AddRange(neuron.Weights);
            }
        }
        reconstructedWeightsAndBiases.Should().Equal(dna.WeightsAndBiases);

        // Neuron activation functions
        var activationFunctions = allNeurons.Select(n => n.ActivationFunction);
        var expectedActivationFunctions = dna.ActivationIndexes.Select(index => Activation.Functions[(ActivationType)index]);
        activationFunctions.Should().Equal(expectedActivationFunctions);
    }
}
