using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
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
        int[] expectedOutputsPerLayer = new int[]
        {
            nInputs,
            hiddenLayers[0],
            hiddenLayers[1],
            hiddenLayers[2],
            nOutputs,
        };

        // Act
        Dna randomlyGeneratedDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Assert
        randomlyGeneratedDna.Inputs.Should().Be(nInputs);
        randomlyGeneratedDna.Outputs.Should().Be(nOutputs);
        randomlyGeneratedDna.OutputsPerLayer.Should().BeEquivalentTo(expectedOutputsPerLayer);
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
        const int neuronActivationIndex = 3;
        int[] hiddenLayers = new int[] { 30, 50, 4, 12 };

        int expectedNumberOfNeurons = hiddenLayers[0] + hiddenLayers[1] + hiddenLayers[2] + hiddenLayers[3] + nOutputs;
        int expectedNumberOfWeightsAndBiases = expectedNumberOfNeurons + // ie. number of bias weights
            nInputs * hiddenLayers[0] +
            hiddenLayers[0] * hiddenLayers[1] +
            hiddenLayers[1] * hiddenLayers[2] +
            hiddenLayers[2] * hiddenLayers[3] +
            hiddenLayers[3] * nOutputs;
        int[] expectedOutputsPerLayer = new int[]
        {
            nInputs,
            hiddenLayers[0],
            hiddenLayers[1],
            hiddenLayers[2],
            hiddenLayers[3],
            nOutputs,
        };

        // Act
        Dna randomlyGeneratedDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)neuronActivationIndex, false);

        // Assert
        randomlyGeneratedDna.Inputs.Should().Be(nInputs);
        randomlyGeneratedDna.Outputs.Should().Be(nOutputs);
        randomlyGeneratedDna.OutputsPerLayer.Should().BeEquivalentTo(expectedOutputsPerLayer);
        randomlyGeneratedDna.WeightsAndBiases.Should().HaveCount(expectedNumberOfWeightsAndBiases);
        randomlyGeneratedDna.ActivationIndexes.Should().HaveCount(expectedNumberOfNeurons);
        randomlyGeneratedDna.ActivationIndexes.Should().OnlyContain(index => index == neuronActivationIndex, "dna was not initialised with uniform hidden layer activation functions");

        foreach (var weightValue in randomlyGeneratedDna.WeightsAndBiases)
            weightValue.Should().BeOfType(typeof(double)).And.BeInRange(-1, 1, "weight was not initialised between -1 and 1");

        foreach (var activationIndex in randomlyGeneratedDna.ActivationIndexes)
            activationIndex.Should().BeOfType(typeof(int)).And.BeInRange(0, Activation.FunctionsCount, "invalid activation index generated");
    }

    [Test]
    public void PerformsDnaCrossover()
    {
        // Arrange
        const int nInputs = 7;
        const int nOutputs = 4;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 7, 1, 4, 7 };
        Dna parent1Dna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        Dna parent2Dna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        List<Dna> offspring = Darwin.CreateOffspring(parent1Dna, parent2Dna, 0.5f, 0.5f);

        // Assert
        offspring.Should().HaveCount(2);
        CheckDnaIsNotReferentiallyEqual(offspring[0], offspring[1]);

        foreach (var child in offspring)
        {
            CheckDnaIsNotReferentiallyEqual(child, parent1Dna);
            CheckDnaIsNotReferentiallyEqual(child, parent2Dna);
            child.Inputs.Should().Be(nInputs);
            child.Outputs.Should().Be(nOutputs);
            child.OutputsPerLayer.Should().Equal(parent1Dna.OutputsPerLayer);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent1Dna.WeightsAndBiases);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent2Dna.WeightsAndBiases);
            child.ActivationIndexes.Should().NotBeEquivalentTo(parent1Dna.ActivationIndexes);
            child.ActivationIndexes.Should().NotBeEquivalentTo(parent2Dna.ActivationIndexes);
        }

        double offspringTotalWeights = offspring[0].WeightsAndBiases.Sum() + offspring[1].WeightsAndBiases.Sum();
        double parentsTotalWeights = parent1Dna.WeightsAndBiases.Sum() + parent2Dna.WeightsAndBiases.Sum();
        parentsTotalWeights.Should().Be(offspringTotalWeights);

        int offspringTotalActivation = offspring[0].ActivationIndexes.Sum() + offspring[1].ActivationIndexes.Sum();
        int parentsTotalActivation = parent1Dna.ActivationIndexes.Sum() + parent2Dna.ActivationIndexes.Sum();
        offspringTotalActivation.Should().Be(parentsTotalActivation);
    }

    [Test]
    public void PerformsDnaCrossoverWithoutActivation()
    {
        // Arrange
        const int nInputs = 5;
        const int nOutputs = 2;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 10, 4, 2, 6 };
        Dna parent1Dna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        Dna parent2Dna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        List<Dna> offspring = Darwin.CreateOffspring(parent1Dna, parent2Dna, 0.5f, 0);

        // Assert
        offspring.Should().HaveCount(2);
        CheckDnaIsNotReferentiallyEqual(offspring[0], offspring[1]);

        foreach (var child in offspring)
        {
            CheckDnaIsNotReferentiallyEqual(child, parent1Dna);
            CheckDnaIsNotReferentiallyEqual(child, parent2Dna);
            child.Inputs.Should().Be(nInputs);
            child.Outputs.Should().Be(nOutputs);
            child.OutputsPerLayer.Should().Equal(parent1Dna.OutputsPerLayer);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent1Dna.WeightsAndBiases);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent2Dna.WeightsAndBiases);
        }

        offspring[0].ActivationIndexes.Should().Equal(parent1Dna.ActivationIndexes);
        offspring[1].ActivationIndexes.Should().Equal(parent2Dna.ActivationIndexes);

        double offspringTotalWeights = offspring[0].WeightsAndBiases.Sum() + offspring[1].WeightsAndBiases.Sum();
        double parentsTotalWeights = parent1Dna.WeightsAndBiases.Sum() + parent2Dna.WeightsAndBiases.Sum();
        parentsTotalWeights.Should().Be(offspringTotalWeights);
    }

    // public void PerformsDnaMutationWithoutActivation()
    [Test]
    public void PerformsDnaMutation()
    {
        // Arrange
        const int nInputs = 20;
        const int nOutputs = 8;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 30, 4, 12 };
        Dna originalDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        Dna mutatedDna = Darwin.CloneAndMutate(originalDna, 0.02f, 0.8f);

        // Assert
        CheckDnaIsNotReferentiallyEqual(originalDna, mutatedDna);
        mutatedDna.Inputs.Should().Be(nInputs);
        mutatedDna.Outputs.Should().Be(nOutputs);
        mutatedDna.OutputsPerLayer.Should().Equal(originalDna.OutputsPerLayer);
        mutatedDna.ActivationIndexes.Should().NotEqual(originalDna.ActivationIndexes);
        mutatedDna.ActivationIndexes.Should().HaveCount(originalDna.ActivationIndexes.Count);
        int indexOfOutputLayerActivation = originalDna.ActivationIndexes.Count - nOutputs;
        mutatedDna.ActivationIndexes.Skip(indexOfOutputLayerActivation)
            .Should().Equal(originalDna.ActivationIndexes.Skip(indexOfOutputLayerActivation), "preserves output layer activation functions");
        mutatedDna.WeightsAndBiases.Should().NotEqual(originalDna.WeightsAndBiases);
        mutatedDna.WeightsAndBiases.Should().HaveCount(originalDna.WeightsAndBiases.Count);
    }

    [Test]
    public void PerformsDnaMutationWithoutActivation()
    {
        // Arrange
        const int nInputs = 20;
        const int nOutputs = 8;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 30, 4, 12 };
        Dna originalDna = Darwin.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        Dna mutatedDna = Darwin.CloneAndMutate(originalDna, 0.02f, 0);

        // Assert
        CheckDnaIsNotReferentiallyEqual(originalDna, mutatedDna);
        mutatedDna.Inputs.Should().Be(nInputs);
        mutatedDna.Outputs.Should().Be(nOutputs);
        mutatedDna.OutputsPerLayer.Should().Equal(originalDna.OutputsPerLayer);
        mutatedDna.ActivationIndexes.Should().Equal(originalDna.ActivationIndexes);
        mutatedDna.ActivationIndexes.Should().HaveCount(originalDna.ActivationIndexes.Count);
        mutatedDna.WeightsAndBiases.Should().NotEqual(originalDna.WeightsAndBiases);
        mutatedDna.WeightsAndBiases.Should().HaveCount(originalDna.WeightsAndBiases.Count);
    }

    public void CheckDnaIsNotReferentiallyEqual(Dna dna1, Dna dna2)
    {
        dna1.Should().NotBeSameAs(dna2); // NotBeSameAs = referential inequality
        dna1.ActivationIndexes.Should().NotBeSameAs(dna2.ActivationIndexes);
        dna1.WeightsAndBiases.Should().NotBeSameAs(dna2.WeightsAndBiases);
        dna1.OutputsPerLayer.Should().NotBeSameAs(dna2.OutputsPerLayer);
    }
}
