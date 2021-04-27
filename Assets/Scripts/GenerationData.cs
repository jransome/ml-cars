using RansomeCorp.AI.Evolution;
using System.Collections.Generic;

[System.Serializable]
public class GenerationData
{
    public int GenerationNumber = 0;
    public int SpawnLocationIndex = 0;
    public List<Dna> GenePool;
    public float TotalFitness = 0;
    public float BestFitness = 0;
    public float AverageFitness = 0;

    public GenerationData(int generationNumber, int spawnLocationIndex, List<Dna> genePool)
    {
        GenerationNumber = generationNumber;
        GenePool = genePool;
        SpawnLocationIndex = spawnLocationIndex;

        foreach (var dna in genePool)
        {
            TotalFitness += dna.RawFitnessRating;
            if (dna.RawFitnessRating > BestFitness) BestFitness = dna.RawFitnessRating;
        }

        AverageFitness = TotalFitness / GenePool.Count;
    }
}
