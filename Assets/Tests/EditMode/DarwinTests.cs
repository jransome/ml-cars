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
}
