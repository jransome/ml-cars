using FluentAssertions;
using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DarwinTests
{
    [Test]
    public void SelectsRandomDnaWeightedByFitness()
    {
        // Arrange
        float totalFitness = 0f;
        List<Dna> parentPool = Enumerable.Range(0, 5)
            .Select((_) => Dna.GenerateRandomDnaEncoding(1, new int[] { 1 }, 1, ActivationType.BinaryStep, false))
            .Select((dna, index) =>
            {
                dna.RawFitnessRating = Mathf.Pow(index, 2);
                totalFitness += dna.RawFitnessRating;
                return dna;
            })
            .ToList();

        Dictionary<Dna, float> expectedProportions = parentPool.ToDictionary(
            dna => dna,
            dna => dna.RawFitnessRating / totalFitness
        );


        // Act
        int sampleSize = 100000;
        Dictionary<Dna, int> selectionResults = parentPool.ToDictionary(dna => dna, dna => 0);

        for (int i = 0; i < sampleSize; i++)
            selectionResults[Darwin.SelectRandomBasedOnFitness(parentPool)]++;


        // Assert
        Dictionary<Dna, float> actualProportions = selectionResults.ToDictionary(
            kvp => kvp.Key,
            kvp => (float)kvp.Value / (float)sampleSize
        );

        foreach (Dna dna in parentPool)
            actualProportions[dna].Should().BeApproximately(expectedProportions[dna], 0.01f, "proportion was within 1% of expected value");
    }

    [Test]
    public void SelectsRandomDnaWeightedByFitnessExcluding()
    {
        // Arrange
        float totalFitness = 0f;
        List<Dna> parentPool = Enumerable.Range(0, 5)
            .Select((_) => Dna.GenerateRandomDnaEncoding(1, new int[] { 1 }, 1, ActivationType.BinaryStep, false))
            .Select((dna, index) =>
            {
                dna.RawFitnessRating = Mathf.Pow(index, 2);
                totalFitness += dna.RawFitnessRating;
                return dna;
            })
            .ToList();

        Dna excludedDna = parentPool[3]; // second fitess dna
        totalFitness -= excludedDna.RawFitnessRating;
        // List<Dna> remainingCandidatePool = parentPool.FindAll(dna => dna != excludedDna);

        Dictionary<Dna, float> expectedProportions = parentPool.ToDictionary(
            dna => dna,
            dna => dna == excludedDna ? 0 : dna.RawFitnessRating / totalFitness
        );


        // Act
        int sampleSize = 100000;
        Dictionary<Dna, int> selectionResults = parentPool.ToDictionary(dna => dna, dna => 0);

        for (int i = 0; i < sampleSize; i++)
            selectionResults[Darwin.SelectRandomBasedOnFitness(parentPool, excludedDna)]++;


        // Assert
        Dictionary<Dna, float> actualProportions = selectionResults.ToDictionary(
            kvp => kvp.Key,
            kvp => (float)kvp.Value / (float)sampleSize
        );

        selectionResults[excludedDna].Should().Be(0, "excluded was never selected");
        foreach (Dna dna in parentPool)
            actualProportions[dna].Should().BeApproximately(expectedProportions[dna], 0.01f, "proportion was within 1% of expected value");
    }

    [Test]
    public void SinglePointCrossoverTests()
    {
        for (int i = 0; i < 1000; i++) SinglePointCrossoverTest();
    }

    [Test]
    public void SinglePointCrossoverRecursiveTests()
    {
        for (int i = 0; i < 1000; i++) SinglePointCrossoverRecursiveTest();
    }

    private void SinglePointCrossoverTest()
    {
        // Arrange
        var list1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var list2 = new List<int>() { 91, 92, 93, 94, 95, 96, 97, 98, 99 };

        // Act
        var crossedLists = list1.SinglePointCrossover(list2);

        // Assert
        crossedLists[0].Should().HaveCount(9);
        crossedLists[1].Should().HaveCount(9);
        crossedLists[0].Should().NotEqual(crossedLists[1]);
        crossedLists[0].Should().NotEqual(list1);
        crossedLists[0].Should().NotEqual(list2);
        crossedLists[1].Should().NotEqual(list1);
        crossedLists[1].Should().NotEqual(list2);
    }

    private void SinglePointCrossoverRecursiveTest()
    {
        // Arrange
        var list1 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var list2 = new List<int>() { 91, 92, 93, 94, 95, 96, 97, 98, 99 };

        // Act
        var crossedLists = list1.SinglePointCrossover(list2, 10);

        // Assert
        crossedLists[0].Should().HaveCount(9);
        crossedLists[1].Should().HaveCount(9);
        crossedLists[0].Should().NotEqual(crossedLists[1]);
        crossedLists[0].Should().NotEqual(list1);
        crossedLists[0].Should().NotEqual(list2);
        crossedLists[1].Should().NotEqual(list1);
        crossedLists[1].Should().NotEqual(list2);
    }
}
