using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GenerationData
{
    public readonly float TotalFitness = 0;
    public readonly float BestFitness = 0;
    public readonly float AverageFitness = 0;
    public readonly ReadOnlyCollection<Dna> GenePool;

    public GenerationData(int generationCount, List<CarBrain> generationPool)
    {
        GenePool = new ReadOnlyCollection<Dna>(generationPool.ConvertAll(b => b.Dna));

        foreach (var brain in generationPool)
        {
            TotalFitness += brain.Fitness;
            if (brain.Fitness > BestFitness) BestFitness = brain.Fitness;
        }

        AverageFitness = TotalFitness / GenePool.Count;
    }
}
