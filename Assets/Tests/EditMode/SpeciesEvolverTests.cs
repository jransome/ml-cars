using FluentAssertions;
using RansomeCorp.AI.Evolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

        List<Dna> previousGen = Enumerable.Range(0, species.GenerationSize).Select((_, index) =>
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

        // Act
        List<Dna> TNG = SpeciesEvolver.CreateGenerationDna(species, previousGen);

        // Assert
        int[] expectedOutputsPerLayer = new int[]
        {
            species.Inputs,
            hiddenLayers[0],
            hiddenLayers[1],
            hiddenLayers[2],
            CarSpecies.Outputs,
        };
        int expectedNumberNew = Mathf.RoundToInt(species.NewDnaRate * 100);
        int expectedNumberUnchanged = Mathf.RoundToInt(species.ProportionUnchanged * 100);
        int expectedNumberMutantsOfUnchanged = Mathf.RoundToInt(species.ProportionMutatantsOfUnchanged * 100);
        int sumNewUnchangedMutated = expectedNumberNew + expectedNumberUnchanged + expectedNumberMutantsOfUnchanged;
        if (sumNewUnchangedMutated % 2 == 1) expectedNumberMutantsOfUnchanged++;
        int expectedNumberOffspring = species.GenerationSize - (expectedNumberNew + expectedNumberUnchanged + expectedNumberMutantsOfUnchanged);

        foreach (Dna dna in TNG)
        {
            dna.Inputs.Should().Be(species.Inputs);
            dna.OutputsPerLayer.Should().Equal(expectedOutputsPerLayer);
            dna.ActivationIndexes.Should().OnlyContain(i => i == (int)species.OutputLayerActivation);
        }

        TNG.Where(d => d.Heritage == DnaHeritage.New).Should().HaveCount(expectedNumberNew);
        TNG.Where(d => d.Heritage == DnaHeritage.Unchanged).Should().HaveCount(expectedNumberUnchanged);
        TNG.Where(d => d.Heritage == DnaHeritage.Mutated).Should().HaveCount(expectedNumberMutantsOfUnchanged);
        TNG.Where(d =>
                d.Heritage != DnaHeritage.New
                && d.Heritage != DnaHeritage.Unchanged
                && d.Heritage != DnaHeritage.Mutated
            )
            .Should().HaveCount(expectedNumberOffspring);
        TNG.Should().HaveCount(species.GenerationSize);

        previousGen.Concat(TNG).Should().OnlyHaveUniqueItems();
    }
}
