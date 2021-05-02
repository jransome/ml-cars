using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;

public class NeuralNetworkTests
{
    // This is run before each test
    [SetUp]
    public void Setup()
    {
        FakeNeuron.Instances.Clear();
    }

    [Test]
    public void NeuralNetworkStructureTests()
    {
        // Arrange
        const int nInputs = 5;
        const int nOutputs = 3;
        const int outputLayerActivationIndex = 0;
        int[] hiddenLayersNeuronCount = new int[] { 4, 3, 6 };
        Dna dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayersNeuronCount, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        NeuralNetwork neuralNetwork = new NeuralNetwork(dna);

        // Assert
        // Correct number of layers and neurons per layer
        neuralNetwork.Layers.Should().HaveCount(hiddenLayersNeuronCount.Count() + 1);
        neuralNetwork.Layers[0].Should().HaveCount(hiddenLayersNeuronCount[0]);
        neuralNetwork.Layers[1].Should().HaveCount(hiddenLayersNeuronCount[1]);
        neuralNetwork.Layers[2].Should().HaveCount(hiddenLayersNeuronCount[2]);
        neuralNetwork.Layers[3].Should().HaveCount(nOutputs);

        // Neurons and weight counts
        IEnumerable<INeuron> allNeurons = neuralNetwork.Layers.SelectMany(n => n); // SelectMany = flatmap
        int expectedNumberOfNeurons = hiddenLayersNeuronCount[0] + hiddenLayersNeuronCount[1] + hiddenLayersNeuronCount[2] + nOutputs;
        allNeurons.Should().HaveCount(expectedNumberOfNeurons);
        allNeurons.Should().OnlyHaveUniqueItems();
        neuralNetwork.Layers[0].Should().OnlyContain(n => n.Weights.Count == nInputs, "neurons in hidden layer 0 had incorrect number of weight inputs");
        neuralNetwork.Layers[1].Should().OnlyContain(n => n.Weights.Count == hiddenLayersNeuronCount[0], "neurons in hidden layer 1 had incorrect number of weight inputs");
        neuralNetwork.Layers[2].Should().OnlyContain(n => n.Weights.Count == hiddenLayersNeuronCount[1], "neurons in hidden layer 2 had incorrect number of weight inputs");
        neuralNetwork.Layers[3].Should().OnlyContain(n => n.Weights.Count == hiddenLayersNeuronCount[2], "neurons in output layer had incorrect number of weight inputs");
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

    [Test]
    public void NeuralNetworkFeedForwardTests()
    {
        // Arrange
        const int nInputs = 3;
        const int nOutputs = 3;
        const int outputLayerActivationIndex = 0;
        int[] hiddenLayersNeuronCount = new int[] { 2, 9, 4 };
        Dna dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayersNeuronCount, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        Func<List<double>, ActivationType, INeuron> fakeNeuronFactory = (List<double> a, ActivationType b) => new FakeNeuron(a, b);

        // Act
        NeuralNetwork neuralNetwork = new NeuralNetwork(dna, fakeNeuronFactory);
        List<double> networkInput = new List<double>() { 1, 2, 3 };
        List<double> networkOutput = neuralNetwork.Think(networkInput);

        // Assert
        IEnumerable<FakeNeuron> allNeurons = neuralNetwork.Layers.SelectMany(n => n).Cast<FakeNeuron>(); // SelectMany = flatmap
        int expectedNumberOfNeurons = hiddenLayersNeuronCount[0] + hiddenLayersNeuronCount[1] + hiddenLayersNeuronCount[2] + nOutputs;
        allNeurons.Should().HaveCount(expectedNumberOfNeurons);
        allNeurons.Should().HaveCount(FakeNeuron.Instances.Count, "count of fake neurons did not match number of created neurons");

        foreach (FakeNeuron n in neuralNetwork.Layers[0].Cast<FakeNeuron>())
        {
            n.RecievedInputs.Should().HaveCount(1, "neurons were not fired the expected number of times");
            n.RecievedInputs[0].Should().Equal(networkInput);
            n.Outputs.Should().HaveCount(1);
        }

        foreach (FakeNeuron n in neuralNetwork.Layers[1].Cast<FakeNeuron>())
        {
            n.RecievedInputs.Should().HaveCount(1, "neurons were not fired the expected number of times");
            n.RecievedInputs[0].Should().Equal(Enumerable.Range(0, hiddenLayersNeuronCount[0]), "layer did not receive expected input from previous layer");
            n.Outputs.Should().HaveCount(1);
        }

        foreach (FakeNeuron n in neuralNetwork.Layers[2].Cast<FakeNeuron>())
        {
            n.RecievedInputs.Should().HaveCount(1, "neurons were not fired the expected number of times");
            n.RecievedInputs[0].Should().Equal(Enumerable.Range(hiddenLayersNeuronCount[0], hiddenLayersNeuronCount[1]), "layer did not receive expected input from previous layer");
            n.Outputs.Should().HaveCount(1);
        }

        foreach (FakeNeuron n in neuralNetwork.Layers[3].Cast<FakeNeuron>())
        {
            n.RecievedInputs.Should().HaveCount(1, "neurons were not fired the expected number of times");
            n.RecievedInputs[0].Should().Equal(Enumerable.Range(hiddenLayersNeuronCount[0] + hiddenLayersNeuronCount[1], hiddenLayersNeuronCount[2]), "layer did not receive expected input from previous layer");
            n.Outputs.Should().HaveCount(1);
        }

        networkOutput.Should().Equal(Enumerable.Range(hiddenLayersNeuronCount[0] + hiddenLayersNeuronCount[1] + hiddenLayersNeuronCount[2], nOutputs), "received expected outputs from network");
    }

    class FakeNeuron : INeuron
    {
        // Testing fields
        public static List<FakeNeuron> Instances = new List<FakeNeuron>();
        public readonly List<List<double>> RecievedInputs = new List<List<double>>();
        public readonly List<double> Outputs = new List<double>();
        public readonly double StubbedOutput;

        public double Bias { get; private set; }
        public List<double> Weights { get; private set; }
        public Func<double, double> ActivationFunction { get; private set; }

        public FakeNeuron(List<double> weights, ActivationType activationType)
        {
            Bias = weights[0];
            Weights = weights.Skip(1).ToList();
            ActivationFunction = Activation.Functions[activationType];
            StubbedOutput = FakeNeuron.Instances.Count;
            FakeNeuron.Instances.Add(this);
        }

        public double Compute(List<double> inputValues)
        {
            RecievedInputs.Add(inputValues);
            Outputs.Add(StubbedOutput);
            return StubbedOutput;
        }
    }
}
