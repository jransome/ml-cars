using RansomeCorp.AI.Evolution;
using System.Collections.Generic;

[System.Serializable]
public class GenerationData
{
    public float TotalFitness = 0;
    public float BestFitness = 0;
    public float AverageFitness = 0;
    public List<Dna> GenePool;

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
