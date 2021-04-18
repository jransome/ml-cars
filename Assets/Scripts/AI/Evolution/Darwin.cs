using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RansomeCorp.AI.Evolution
{
    public class Darwin
    {
        public static Dna SelectRandomBasedOnFitness(List<Dna> parentPool, Dna excluding = null)
        {
            List<Dna> candidates = excluding == null ? parentPool : parentPool.Where(p => p != excluding).ToList();
            float totalFitness = candidates.Aggregate(0f, (total, candidate) => total + candidate.RawFitnessRating); // TODO: optimise this
            List<KeyValuePair<Dna, float>> candidateChances = candidates.ConvertAll(c => new KeyValuePair<Dna, float>(c, c.RawFitnessRating / totalFitness));

            float diceRoll = Random.Range(0f, 1f);
            float cumulative = 0f;
            for (int i = 0; i < candidateChances.Count; i++)
            {
                cumulative += candidateChances[i].Value;
                if (diceRoll < cumulative) return candidateChances[i].Key;
            }

            Debug.LogWarning("Failed to choose new random parent by fitness...");
            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}