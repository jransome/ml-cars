using RansomeCorp.AI.Evolution;
using System.Collections.Generic;

public class GenerationData
{
    public readonly float TotalFitness = 0;
    public readonly float BestFitness = 0;
    public readonly float AverageFitness = 0;
    public readonly List<Dna> GenePool;

    public GenerationData(int generationCount, List<Dna> genePool)
    {
        GenePool = genePool;

        foreach (var dna in genePool)
        {
            TotalFitness += dna.RawFitnessRating;
            if (dna.RawFitnessRating > BestFitness) BestFitness = dna.RawFitnessRating;
        }

        AverageFitness = TotalFitness / GenePool.Count;
    }
}
