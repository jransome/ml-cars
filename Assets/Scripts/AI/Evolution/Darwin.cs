using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RansomeCorp.AI.Evolution
{
    public static class Darwin
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

        /// <summary>
        /// Crosses a list of T with another at a single point to produce two new lists. Iterations must 
        /// be an odd number or there is a possibility that the result will be the same as the original.
        /// </summary>
        public static List<List<T>> SinglePointCrossover<T>(this List<T> list1, List<T> list2, int iterations = 1, int previousSlicePoint = -1)
        {
            int elementsCount = list1.Count;
            if (elementsCount == 1)
            {
                Debug.LogWarning("Lists not long enough to be crossed!");
                return new List<List<T>>(2) { list2, list1 };
            }

            if (iterations % 2 == 0) iterations++; // make iterations odd

            return SinglePointCrossoverRecursive(list1, list2, iterations);
        }

        private static List<List<T>> SinglePointCrossoverRecursive<T>(List<T> list1, List<T> list2, int iterations)
        {
            int elementsCount = list1.Count;
            int slicePoint = UnityEngine.Random.Range(1, elementsCount - 1);

            var crossed = new List<List<T>>(2)
            {
                list1.Take(slicePoint).Concat(list2.Skip(slicePoint)).ToList(),
                list2.Take(slicePoint).Concat(list1.Skip(slicePoint)).ToList(),
            };

            return --iterations == 0 ? crossed : SinglePointCrossoverRecursive(crossed[0], crossed[1], iterations);
        }
    }
}