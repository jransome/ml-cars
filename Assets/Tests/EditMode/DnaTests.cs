using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DnaTests
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
        Dna randomlyGeneratedDna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Assert
        randomlyGeneratedDna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
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
        Dna randomlyGeneratedDna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)neuronActivationIndex, false);

        // Assert
        randomlyGeneratedDna.OutputsPerLayer.Should().BeEquivalentTo(expectedOutputsPerLayer);
        randomlyGeneratedDna.WeightsAndBiases.Should().HaveCount(expectedNumberOfWeightsAndBiases);
        randomlyGeneratedDna.ActivationIndexes.Should().HaveCount(expectedNumberOfNeurons);
        randomlyGeneratedDna.ActivationIndexes.Should().OnlyContain(index => index == neuronActivationIndex, "dna was not initialised with uniform hidden layer activation functions");
        randomlyGeneratedDna.Heritage.Should().Be(DnaHeritage.New);

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
        Dna parent1Dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        Dna parent2Dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        List<Dna> offspring = Dna.CreateOffspring(parent1Dna, parent2Dna, 5, true);

        // Assert
        offspring.Should().HaveCount(2);
        CheckDnaIsNotReferentiallyEqual(offspring[0], offspring[1]);

        foreach (var child in offspring)
        {
            CheckDnaIsNotReferentiallyEqual(child, parent1Dna);
            CheckDnaIsNotReferentiallyEqual(child, parent2Dna);
            child.OutputsPerLayer.Should().Equal(parent1Dna.OutputsPerLayer);
            child.WeightsAndBiases.Should().HaveCount(parent1Dna.WeightsAndBiases.Count);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent1Dna.WeightsAndBiases);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent2Dna.WeightsAndBiases);
            child.ActivationIndexes.Should().HaveCount(parent1Dna.ActivationIndexes.Count);
            child.ActivationIndexes.Should().NotBeEquivalentTo(parent1Dna.ActivationIndexes);
            child.ActivationIndexes.Should().NotBeEquivalentTo(parent2Dna.ActivationIndexes);
            child.Heritage.Should().Be(DnaHeritage.Offspring);
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
        Dna parent1Dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        Dna parent2Dna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);

        // Act
        List<Dna> offspring = Dna.CreateOffspring(parent1Dna, parent2Dna, 5, false);

        // Assert
        offspring.Should().HaveCount(2);
        CheckDnaIsNotReferentiallyEqual(offspring[0], offspring[1]);

        foreach (var child in offspring)
        {
            CheckDnaIsNotReferentiallyEqual(child, parent1Dna);
            CheckDnaIsNotReferentiallyEqual(child, parent2Dna);
            child.OutputsPerLayer.Should().Equal(parent1Dna.OutputsPerLayer);
            child.WeightsAndBiases.Should().HaveCount(parent1Dna.WeightsAndBiases.Count);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent1Dna.WeightsAndBiases);
            child.WeightsAndBiases.Should().NotBeEquivalentTo(parent2Dna.WeightsAndBiases);
            child.Heritage.Should().Be(DnaHeritage.Offspring);
        }

        offspring[0].ActivationIndexes.Should().Equal(parent1Dna.ActivationIndexes);
        offspring[1].ActivationIndexes.Should().Equal(parent2Dna.ActivationIndexes);

        double offspringTotalWeights = offspring[0].WeightsAndBiases.Sum() + offspring[1].WeightsAndBiases.Sum();
        double parentsTotalWeights = parent1Dna.WeightsAndBiases.Sum() + parent2Dna.WeightsAndBiases.Sum();
        parentsTotalWeights.Should().Be(offspringTotalWeights);
    }

    [Test]
    public void PerformsDnaMutation()
    {
        // Arrange
        const int nInputs = 2;
        const int nOutputs = 2;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 7 };
        Dna originalDna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        float weightMutationPrevalence = 0.2f;

        // Act
        Dna mutatedDna = Dna.CloneAndMutate(originalDna, DnaHeritage.MutatedElite, weightMutationPrevalence, 0.8f);

        // Assert
        CheckDnaIsNotReferentiallyEqual(originalDna, mutatedDna);
        // Structure
        mutatedDna.OutputsPerLayer.Should().Equal(originalDna.OutputsPerLayer);
        mutatedDna.Heritage.Should().Be(DnaHeritage.MutatedElite);

        // Weights
        mutatedDna.WeightsAndBiases.Should().NotEqual(originalDna.WeightsAndBiases);
        mutatedDna.WeightsAndBiases.Should().HaveCount(originalDna.WeightsAndBiases.Count);
        List<double> mutatedWeights = new List<double>();
        for (int i = 0; i < originalDna.WeightsAndBiases.Count; i++)
        {
            if (originalDna.WeightsAndBiases[i] != mutatedDna.WeightsAndBiases[i])
                mutatedWeights.Add(mutatedDna.WeightsAndBiases[i]);
        }
        mutatedWeights.Should().HaveCount(Mathf.CeilToInt(weightMutationPrevalence * originalDna.WeightsAndBiases.Count));

        // Activation        
        mutatedDna.ActivationIndexes.Should().NotEqual(originalDna.ActivationIndexes);
        mutatedDna.ActivationIndexes.Should().HaveCount(originalDna.ActivationIndexes.Count);
        int indexOfOutputLayerActivation = originalDna.ActivationIndexes.Count - nOutputs;
        mutatedDna.ActivationIndexes.Skip(indexOfOutputLayerActivation)
            .Should().Equal(originalDna.ActivationIndexes.Skip(indexOfOutputLayerActivation), "preserves output layer activation functions");
    }

    [Test]
    public void PerformsDnaMutationWithoutActivation()
    {
        // Arrange
        const int nInputs = 2;
        const int nOutputs = 2;
        const int outputLayerActivationIndex = 3;
        int[] hiddenLayers = new int[] { 7 };
        Dna originalDna = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, (ActivationType)outputLayerActivationIndex, true);
        float weightMutationPrevalence = 0.02f;

        // Act
        Dna mutatedDna = Dna.CloneAndMutate(originalDna, DnaHeritage.MutatedElite, weightMutationPrevalence, 0);

        // Assert
        CheckDnaIsNotReferentiallyEqual(originalDna, mutatedDna);
        // Structure
        mutatedDna.OutputsPerLayer.Should().Equal(originalDna.OutputsPerLayer);
        mutatedDna.Heritage.Should().Be(DnaHeritage.MutatedElite);

        // Weights
        mutatedDna.WeightsAndBiases.Should().NotEqual(originalDna.WeightsAndBiases);
        mutatedDna.WeightsAndBiases.Should().HaveCount(originalDna.WeightsAndBiases.Count);
        List<double> mutatedWeights = new List<double>();
        for (int i = 0; i < originalDna.WeightsAndBiases.Count; i++)
        {
            if (originalDna.WeightsAndBiases[i] != mutatedDna.WeightsAndBiases[i])
                mutatedWeights.Add(mutatedDna.WeightsAndBiases[i]);
        }
        mutatedWeights.Should().HaveCount(Mathf.CeilToInt(weightMutationPrevalence * originalDna.WeightsAndBiases.Count));

        // Activation        
        mutatedDna.ActivationIndexes.Should().Equal(originalDna.ActivationIndexes);
        mutatedDna.ActivationIndexes.Should().HaveCount(originalDna.ActivationIndexes.Count);
    }

    [Test]
    public void CompareWeightsReturnsExpectedResultOfIdentical()
    {
        // Arrange
        const int nInputs = 20;
        const int nOutputs = 8;
        int[] hiddenLayers = new int[] { 30, 4, 12 };
        Dna dna1 = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, ActivationType.LeakyRelu, true);
        Dna dna2 = Dna.Clone(dna1);

        // Act
        var comparisonResult = DnaUtils.CompareWeights(dna1, dna2);

        // Assert
        float percentWeightDiff = comparisonResult.Item1;
        double percentWeightValueDiff = comparisonResult.Item2;
        percentWeightDiff.Should().Be(0);
        percentWeightValueDiff.Should().Be(0);
    }

    [Test]
    public void CompareWeightsReturnsExpectedResultOfDifferent()
    {
        // Arrange
        const int nInputs = 9;
        const int nOutputs = 1;
        int[] hiddenLayers = new int[] { 9 };
        Dna dna1 = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, ActivationType.LeakyRelu, true);
        dna1.WeightsAndBiases.Should().HaveCount(100);
        dna1.WeightsAndBiases.Clear();
        dna1.WeightsAndBiases.AddRange(Enumerable.Repeat(1.00, 100));
        Dna dna2 = Dna.Clone(dna1);
        dna1.WeightsAndBiases.Should().Equal(dna2.WeightsAndBiases);
        dna2.WeightsAndBiases[2] = -49.00; // aggregate absolute weight difference of dna2 vs dna1 should now be 50
        dna1.WeightsAndBiases.Should().NotEqual(dna2.WeightsAndBiases);

        // Act
        var comparisonResult = DnaUtils.CompareWeights(dna1, dna2);

        // Assert
        float percentWeightDiff = comparisonResult.Item1;
        double percentWeightValueDiff = comparisonResult.Item2;
        percentWeightDiff.Should().Be(0.01f); // ie. 1% of weights differ in value
        percentWeightValueDiff.Should().Be(0.5f); // ie. total weight values of dna2 differ by 50% (NOT 50% bigger)
    }

    [Test]
    public void EqualsTest()
    {
        // Arrange
        const int nInputs = 9;
        const int nOutputs = 1;
        int[] hiddenLayers = new int[] { 9 };
        Dna dna1 = Dna.GenerateRandomDnaEncoding(nInputs, hiddenLayers, nOutputs, ActivationType.LeakyRelu, true);
        Dna dna1ValueClone = Dna.Clone(dna1);
        Dna dna1ReferenceClone = dna1;
        Dna dna2 = Dna.Clone(dna1);
        dna2.WeightsAndBiases[0] = 50;

        // Act/Assert
        dna1.Equals(dna1).Should().BeTrue();

        dna1.Equals(dna1ReferenceClone).Should().BeTrue();
        dna1ReferenceClone.Equals(dna1).Should().BeTrue();

        dna1.Equals(dna1ValueClone).Should().BeTrue();
        dna1ValueClone.Equals(dna1).Should().BeTrue();

        dna1.Equals(dna2).Should().BeFalse();
        dna2.Equals(dna1).Should().BeFalse();

        dna1ValueClone.Equals(dna2).Should().BeFalse();
        dna2.Equals(dna1ValueClone).Should().BeFalse();

        dna1ReferenceClone.Equals(dna2).Should().BeFalse();
        dna2.Equals(dna1ReferenceClone).Should().BeFalse();

        dna1.WeightsAndBiases[1] = 100;

        dna1.Equals(dna1ReferenceClone).Should().BeTrue(); // reference stays equal
        dna1ReferenceClone.Equals(dna1).Should().BeTrue();

        dna1.Equals(dna1ValueClone).Should().BeFalse(); // value changes
        dna1ValueClone.Equals(dna1).Should().BeFalse();
    }

    private static void CheckDnaIsNotReferentiallyEqual(Dna dna1, Dna dna2)
    {
        dna1.Should().NotBeSameAs(dna2); // NotBeSameAs = referential inequality
        dna1.ActivationIndexes.Should().NotBeSameAs(dna2.ActivationIndexes);
        dna1.WeightsAndBiases.Should().NotBeSameAs(dna2.WeightsAndBiases);
        dna1.OutputsPerLayer.Should().NotBeSameAs(dna2.OutputsPerLayer);
    }
}
