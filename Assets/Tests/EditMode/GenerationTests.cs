using FluentAssertions;
using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class GenerationTests
{
    [Test]
    public void CreateSeedGenerationTest()
    {
        // Arrange
        int spawnIndex = 3;
        int[] hiddenLayers = new int[] { 4, 3, 6 };
        CarSpecies species = CarSpecies.CreateInstance(typeof(CarSpecies)) as CarSpecies;
        species.HiddenLayersNeuronCount = hiddenLayers;
        int[] expectedOutputsPerLayer = new int[]
        {
            species.Inputs,
            hiddenLayers[0],
            hiddenLayers[1],
            hiddenLayers[2],
            CarSpecies.Outputs,
        };

        // Act
        Generation TNG = Generation.CreateSeed(species, spawnIndex);

        // Assert
        TNG.SpawnLocationIndex.Should().Be(spawnIndex);
        TNG.GenePool.Should().HaveCount(species.GenerationSize);
        TNG.GenePool.Should().OnlyHaveUniqueItems();
        AssertDnaHeterogeneity(TNG.GenePool);
        foreach (Dna dna in TNG.GenePool)
        {
            dna.Heritage.Should().Be(DnaHeritage.New);
            dna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
        }
    }

    [Test]
    public void CreateGenerationFromPreviousTest()
    {
        // Arrange
        int[] hiddenLayers = new int[] { 4, 3, 6 };
        CarSpecies species = CarSpecies.CreateInstance(typeof(CarSpecies)) as CarSpecies;
        species.GenerationSize = 100;
        species.HeterogeneousHiddenActivation = false;
        species.HiddenLayersNeuronCount = hiddenLayers;
        species.ActivationMutationSeverity = 0f;
        species.ProportionUnchanged = 0.05f;
        species.ProportionMutatantsOfUnchanged = 0.2f;
        species.OffspringMutationProbability = 0.5f;
        species.NewDnaRate = 0.05f;

        Generation seed = Generation.CreateSeed(species, 1);
        AssertDnaHeterogeneity(seed.GenePool);

        // Act
        Generation TNG = Generation.FromPrevious(seed, 1);

        // Assert
        AssertOnDerrivedGenerationDna(species, seed.GenePool, TNG.GenePool);
    }

    [Test]
    public void CreateGenerationsIterativelyTest()
    {
        // Arrange
        int[] hiddenLayers = new int[] { 4, 3, 6 };
        CarSpecies species = CarSpecies.CreateInstance(typeof(CarSpecies)) as CarSpecies;
        species.GenerationSize = 100;
        species.HeterogeneousHiddenActivation = false;
        species.HiddenLayersNeuronCount = hiddenLayers;
        species.ActivationMutationSeverity = 0f;
        species.ProportionUnchanged = 0.05f;
        species.ProportionMutatantsOfUnchanged = 0.2f;
        species.OffspringMutationProbability = 0.5f;
        species.NewDnaRate = 0.05f;
        species.CrossoverPasses = 5;

        Generation seed = Generation.CreateSeed(species, 1);
        AssertDnaHeterogeneity(seed.GenePool);

        Generation previous = seed;
        for (int i = 1; i < 101; i++)
        {
            AssignFakeFitness(previous);
            
            // Act
            Generation TNG = Generation.FromPrevious(previous, 1);
            Debug.Log("Created gen " + i);

            // Assert
            AssertOnDerrivedGenerationDna(species, previous.GenePool, TNG.GenePool);

            previous = TNG;
        }
    }

    static void AssertOnDerrivedGenerationDna(CarSpecies species, List<Dna> previousGenDna, List<Dna> TNGdna)
    {
        int[] expectedOutputsPerLayer = new int[] { species.Inputs }.Concat(species.HiddenLayersNeuronCount).Append(CarSpecies.Outputs).ToArray();
        int expectedNumberNew = Mathf.RoundToInt(species.NewDnaRate * 100);
        int expectedNumberUnchanged = Mathf.RoundToInt(species.ProportionUnchanged * 100);
        int expectedNumberMutantsOfUnchanged = Mathf.RoundToInt(species.ProportionMutatantsOfUnchanged * 100);
        int sumNewUnchangedMutated = expectedNumberNew + expectedNumberUnchanged + expectedNumberMutantsOfUnchanged;
        if (sumNewUnchangedMutated % 2 == 1) expectedNumberMutantsOfUnchanged++;
        int expectedNumberOffspring = species.GenerationSize - (expectedNumberNew + expectedNumberUnchanged + expectedNumberMutantsOfUnchanged);
        int expectedNumberMutatedOffspring = Mathf.RoundToInt(expectedNumberOffspring * species.OffspringMutationProbability);
        expectedNumberOffspring.Should().BeGreaterOrEqualTo(expectedNumberMutatedOffspring);

        // Topologies
        foreach (Dna dna in TNGdna)
        {
            dna.Inputs.Should().Be(species.Inputs);
            dna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
            dna.ActivationIndexes.Should().OnlyContain(i => i == (int)species.OutputLayerActivation);
        }

        // Expected proportions
        TNGdna.Where(d => d.Heritage == DnaHeritage.New).Should().HaveCount(expectedNumberNew);
        TNGdna.Where(d => d.Heritage == DnaHeritage.Elite).Should().HaveCount(expectedNumberUnchanged);
        TNGdna.Where(d => d.Heritage == DnaHeritage.MutatedElite).Should().HaveCount(expectedNumberMutantsOfUnchanged);
        TNGdna.Where(d => d.Heritage == DnaHeritage.MutatedOffspring).Count().Should().BeInRange(
            Mathf.RoundToInt(expectedNumberMutatedOffspring * 0.5f),
            Mathf.RoundToInt(expectedNumberMutatedOffspring * 1.5f)
        );
        TNGdna.Where(d =>
                d.Heritage != DnaHeritage.New
                && d.Heritage != DnaHeritage.Elite
                && d.Heritage != DnaHeritage.MutatedElite
            )
            .Should().HaveCount(expectedNumberOffspring);
        previousGenDna.Concat(TNGdna).Should().OnlyHaveUniqueItems();
        TNGdna.Should().HaveCount(species.GenerationSize);

        // Genetic variation
        AssertDnaHeterogeneity(TNGdna);

        // Genetic variation from previous
        foreach (Dna g2Dna in TNGdna.Where(d => d.Heritage != DnaHeritage.Elite))
            previousGenDna.Any(g1Dna => g1Dna.Equals(g2Dna)).Should().BeFalse();
    }

    static void AssertDnaHeterogeneity(List<Dna> population)
    {
        population.Should().OnlyHaveUniqueItems(); // by reference

        var occurrences = population.ToDictionary(dna => dna, (_) => 1);
        for (int i = 0; i < population.Count; i++)
        {
            Dna pop = population[i];
            for (int j = 0; j < population.Count; j++)
                if (i != j && pop.Equals(population[j])) occurrences[pop] += 1;
        }

        occurrences.Values.Should().OnlyContain(occurrenceCount => occurrenceCount == 1);
    }

    static void AssignFakeFitness(Generation g)
    {
        for (int i = 0; i < g.GenePool.Count; i++)
            g.GenePool[i].RawFitnessRating = Mathf.Pow((1 + i), 2);
    }
}
