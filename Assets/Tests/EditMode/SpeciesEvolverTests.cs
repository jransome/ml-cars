using FluentAssertions;
using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class SpeciesEvolverTests
{
    [Test]
    public void CreateSeedGenerationDnaTest()
    {
        // Arrange
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
        List<Dna> TNG = SpeciesEvolver.CreateGenerationDna(species);

        // Assert
        TNG.Should().HaveCount(species.GenerationSize);
        TNG.Should().OnlyHaveUniqueItems();
        AssertPopulationHeterogeneity(TNG);
        foreach (Dna dna in TNG)
        {
            dna.Heritage.Should().Be(DnaHeritage.New);
            dna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
        }
    }

    [Test]
    public void CreateGenerationFromPreviousDnaTest()
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

        List<Dna> seedGeneration = Enumerable.Range(0, species.GenerationSize).Select((_, index) =>
        {
            Dna dna = Dna.GenerateRandomDnaEncoding(
                species.Inputs,
                species.HiddenLayersNeuronCount,
                CarSpecies.Outputs,
                species.OutputLayerActivation,
                species.HeterogeneousHiddenActivation
            );
            dna.RawFitnessRating = index * 5;
            return dna;
        }).ToList();
        seedGeneration.Should().OnlyHaveUniqueItems(dna => dna.WeightsAndBiases); // checks the values in the weights and biases list

        List<GenerationData> generationHistory = new List<GenerationData>();
        generationHistory.Add(new GenerationData(0, seedGeneration));

        // Act
        List<Dna> TNG = SpeciesEvolver.CreateGenerationDna(species, generationHistory);

        // Assert
        AssertOnDerrivedGeneration(species, seedGeneration, TNG);
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
        species.UseSinglePointCrossover = false;

        List<Dna> seedGeneration = Enumerable.Range(0, species.GenerationSize).Select((_, index) =>
        {
            Dna dna = Dna.GenerateRandomDnaEncoding(
                species.Inputs,
                species.HiddenLayersNeuronCount,
                CarSpecies.Outputs,
                species.OutputLayerActivation,
                species.HeterogeneousHiddenActivation
            );
            dna.RawFitnessRating = Mathf.Pow(index, 2);
            return dna;
        }).ToList();
        AssertPopulationHeterogeneity(seedGeneration);

        List<GenerationData> generationHistory = new List<GenerationData>();
        generationHistory.Add(new GenerationData(0, seedGeneration));

        for (int i = 1; i < 101; i++)
        {
            // Act
            List<Dna> newGeneration = SpeciesEvolver.CreateGenerationDna(species, generationHistory);
            Debug.Log("Created gen " + i);
            // Assert
            AssertOnDerrivedGeneration(species, generationHistory.Last().GenePool, newGeneration);

            // Fake fitness values for the next generation
            for (int j = 0; j < newGeneration.Count; j++)
                newGeneration[j].RawFitnessRating = Mathf.Pow((1 + j), 2);
            generationHistory.Add(new GenerationData(0, newGeneration));
        }
    }

    static void AssertOnDerrivedGeneration(CarSpecies species, List<Dna> previousGen, List<Dna> TNG)
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
        foreach (Dna dna in TNG)
        {
            dna.Inputs.Should().Be(species.Inputs);
            dna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
            dna.ActivationIndexes.Should().OnlyContain(i => i == (int)species.OutputLayerActivation);
        }

        // Expected proportions
        TNG.Where(d => d.Heritage == DnaHeritage.New).Should().HaveCount(expectedNumberNew);
        TNG.Where(d => d.Heritage == DnaHeritage.Unchanged).Should().HaveCount(expectedNumberUnchanged);
        TNG.Where(d => d.Heritage == DnaHeritage.Mutated).Should().HaveCount(expectedNumberMutantsOfUnchanged);
        TNG.Where(d => d.Heritage == DnaHeritage.BredAndMutated).Count().Should().BeInRange(
            Mathf.RoundToInt(expectedNumberMutatedOffspring * 0.5f),
            Mathf.RoundToInt(expectedNumberMutatedOffspring * 1.5f)
        );
        TNG.Where(d =>
                d.Heritage != DnaHeritage.New
                && d.Heritage != DnaHeritage.Unchanged
                && d.Heritage != DnaHeritage.Mutated
            )
            .Should().HaveCount(expectedNumberOffspring);
        previousGen.Concat(TNG).Should().OnlyHaveUniqueItems();
        TNG.Should().HaveCount(species.GenerationSize);

        // Genetic variation
        AssertPopulationHeterogeneity(TNG);

        // Genetic variation from previous
        foreach (Dna g2Dna in TNG.Where(d => d.Heritage != DnaHeritage.Unchanged))
            previousGen.Any(g1Dna => g1Dna.WeightsAndBiases.SequenceEqual(g2Dna.WeightsAndBiases)).Should().BeFalse();
    }

    static void AssertPopulationHeterogeneity(List<Dna> population)
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
}
