using RansomeCorp;
using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;

public class HelpersTests
{
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
        var crossedLists = list1.SinglePointCrossover(list2, 5);

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
