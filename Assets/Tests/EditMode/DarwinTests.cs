using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Linq;
using NUnit.Framework;

public class DarwinTests
{
    [Test]
    public void GeneratesRandomDnaWithHeterogeneousActivation()
    {
        // Arrange
        const int nInputs = 5;
        const int nOutputs = 3;
        const int outputLayerActivationIndex = 0;
        int[] hiddenLayers = new int[] { 4, 3, 6 };

        int expectedNumberOfNeurons = hiddenLayers[0] + hiddenLayers[1] + hiddenLayers[2] + nOutputs;
        int expectedNumberOfWeightsAndBiases = expectedNumberOfNeurons + // ie. number of bias weights
            nInputs * hiddenLayers[0] +
            hiddenLayers[0] * hiddenLayers[1] +
            hiddenLayers[1] * hiddenLayers[2] +
            hiddenLayers[2] * nOutputs;

        // Act
        Dna randomlyGeneratedDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Assert
        randomlyGeneratedDna.Inputs.Should().Be(nInputs);
        randomlyGeneratedDna.Outputs.Should().Be(nOutputs);
        randomlyGeneratedDna.HiddenLayers.Should().BeEquivalentTo(hiddenLayers);
        randomlyGeneratedDna.WeightsAndBiases.Should().HaveCount(expectedNumberOfWeightsAndBiases);
        randomlyGeneratedDna.ActivationIndexes.Should().HaveCount(expectedNumberOfNeurons);
        randomlyGeneratedDna.ActivationIndexes.Should().Contain(index => index > outputLayerActivationIndex, "dna was not initialised with random hidden layer activation functions");
        randomlyGeneratedDna.ActivationIndexes.Skip(expectedNumberOfNeurons - nOutputs).Should().OnlyContain(index => index == outputLayerActivationIndex, "output layer activation functions were not of the expected kind");

        foreach (var weightValue in randomlyGeneratedDna.WeightsAndBiases)
            weightValue.Should().BeOfType(typeof(double)).And.BeInRange(-1, 1, "weight was not initialised between -1 and 1");

        foreach (var activationIndex in randomlyGeneratedDna.ActivationIndexes)
            activationIndex.Should().BeOfType(typeof(int)).And.BeInRange(0, Activation.FunctionsCount, "invalid activation index generated");
    }

    [Test]
    public void GeneratesRandomDnaWithHomogeneousActivation()
    {
        // Arrange
        const int nInputs = 50;
        const int nOutputs = 8;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 30, 50, 4, 12 };

        int expectedNumberOfNeurons = hiddenLayers[0] + hiddenLayers[1] + hiddenLayers[2] +  hiddenLayers[3] + nOutputs;
        int expectedNumberOfWeightsAndBiases = expectedNumberOfNeurons + // ie. number of bias weights
            nInputs * hiddenLayers[0] +
            hiddenLayers[0] * hiddenLayers[1] +
            hiddenLayers[1] * hiddenLayers[2] +
            hiddenLayers[2] * hiddenLayers[3] +
            hiddenLayers[3] * nOutputs;

        // Act
        Dna randomlyGeneratedDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, false);

        // Assert
        randomlyGeneratedDna.Inputs.Should().Be(nInputs);
        randomlyGeneratedDna.Outputs.Should().Be(nOutputs);
        randomlyGeneratedDna.HiddenLayers.Should().BeEquivalentTo(hiddenLayers);
        randomlyGeneratedDna.WeightsAndBiases.Should().HaveCount(expectedNumberOfWeightsAndBiases);
        randomlyGeneratedDna.ActivationIndexes.Should().HaveCount(expectedNumberOfNeurons);
        randomlyGeneratedDna.ActivationIndexes.Should().OnlyContain(index => index == outputLayerActivationIndex, "dna was not initialised with uniform hidden layer activation functions");

        foreach (var weightValue in randomlyGeneratedDna.WeightsAndBiases)
            weightValue.Should().BeOfType(typeof(double)).And.BeInRange(-1, 1, "weight was not initialised between -1 and 1");

        foreach (var activationIndex in randomlyGeneratedDna.ActivationIndexes)
            activationIndex.Should().BeOfType(typeof(int)).And.BeInRange(0, Activation.FunctionsCount, "invalid activation index generated");
    }
}
