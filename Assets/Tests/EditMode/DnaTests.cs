using RansomeCorp.AI.Evolution;
using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;

public class DnaTests
{
    // [Test]
    // public void CloneReturnsADeepCopy()
    // {
    //     // Arrange
    //     const int nInputs = 5;
    //     const int nOutputs = 3;
    //     int[] outputsPerLayer = new int[] { 1, 2, 3, 4, 5 };
    //     List<double> weightsAndBiases = new List<double>() { 1, 3, 5, 7, 9 };
    //     List<int> activationIndexes = new List<int>() { 6, 7, 8 };
    //     Dna original = new Dna(nInputs, nOutputs, outputsPerLayer, weightsAndBiases, activationIndexes);

    //     // Act
    //     Dna clone = original.Clone();

    //     // Assert
    //     clone.Inputs.Should().Be(original.Inputs);
    //     clone.Outputs.Should().Be(original.Outputs);
    //     clone.OutputsPerLayer.Should().Equal(original.OutputsPerLayer);
    //     clone.WeightsAndBiases.Should().Equal(original.WeightsAndBiases);
    //     clone.ActivationIndexes.Should().Equal(original.ActivationIndexes);
    //     clone.Should().NotBeSameAs(original);
    //     clone.OutputsPerLayer.Should().NotBeSameAs(original.OutputsPerLayer); // check both instances are not referencing the same collections
    //     clone.WeightsAndBiases.Should().NotBeSameAs(original.WeightsAndBiases);
    //     clone.ActivationIndexes.Should().NotBeSameAs(original.ActivationIndexes);
    // }
}
