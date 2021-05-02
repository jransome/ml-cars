using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RansomeCorp.AI.Evolution
{
    public static class DnaUtils
    {
        struct Identicle
        {
            public Dna NextGenDna;
            public List<Dna> ClonesInPrevGen;

            public Identicle(Dna dna, List<Dna> clonesInPrevGen)
            {
                NextGenDna = dna;
                ClonesInPrevGen = clonesInPrevGen;
            }
        }

        public static void DebugDnaDiff(Dna dna1, Dna dna2, string relation)
        {
            var comparison = CompareWeights(dna1, dna2);
            string log = System.String.Format("{0} | {1:P2} of weights are different, {2:P2} absolute value difference", relation, comparison.Item1, comparison.Item2);
            Debug.Log(log);
        }

        public static void DebugGenerationDiff(List<Dna> gen1, List<Dna> gen2)
        {
            // remove deliberate clones from previous
            var shouldBeUniques = gen2.Where(d => d.Heritage != DnaHeritage.Elite).ToList();
            List<Identicle> identicles = new List<Identicle>();

            foreach (Dna g2Dna in shouldBeUniques)
            {
                var clones = gen1.Where(g1Dna => g1Dna.Equals(g2Dna));
                if (clones.Count() > 0) identicles.Add(new Identicle(g2Dna, clones.ToList()));
            }

            if (identicles.Count > 0)
            {
                string log = System.String.Format(
                    "{0} dna instances identicle to previous gen out of {1}. {2} mutant elites, {3} offspring, {4} mutated offspring",
                    identicles.Count,
                    shouldBeUniques.Count,
                    identicles.Where(i => i.NextGenDna.Heritage == DnaHeritage.MutatedElite).Count(),
                    identicles.Where(i => i.NextGenDna.Heritage == DnaHeritage.Offspring).Count(),
                    identicles.Where(i => i.NextGenDna.Heritage == DnaHeritage.MutatedOffspring).Count()
                );
                Debug.LogError(log);
            }

            // check for intra generation clones
            int nClones = 0;
            for (int i = 0; i < gen2.Count; i++)
            {
                for (int j = i + 1; j < gen2.Count; j++)
                {
                    if (gen2[i].Equals(gen2[j])) nClones++;
                }
            }
            if (nClones > 0) Debug.LogError($"{nClones} clones detected!");
        }

        /// <summary>
        /// Returns tuple containing 1) percentage of weight values that differ, and 2) the absolute difference in weight values of dna2 as a percentage of the aggregated absolute weight values of dna1
        /// </summary>
        public static System.Tuple<float, double> CompareWeights(Dna dna1, Dna dna2)
        {
            if (!Dna.TopologiesEqual(dna1, dna2))
                throw new System.ArgumentException("Can't compare dna weights of different topologies!");

            if (dna1.WeightsAndBiases.Count != dna2.WeightsAndBiases.Count) // should always be false if topologies are the same. but just in case...
                throw new System.ArgumentException("Weights arrays not equal???");

            double accumulatedAbsoluteDifference = 0;
            int nWeightsDiffering = 0;
            for (int i = 0; i < dna1.WeightsAndBiases.Count; i++)
            {
                double absoluteDifference = System.Math.Abs(dna1.WeightsAndBiases[i] - dna2.WeightsAndBiases[i]);
                accumulatedAbsoluteDifference += absoluteDifference;
                if (absoluteDifference > 0) nWeightsDiffering++;
            }

            double dna1TotalWeight = dna1.WeightsAndBiases.Aggregate(0.0, (total, weight) => total + System.Math.Abs(weight));
            double weightDifferenceAsProportion = accumulatedAbsoluteDifference / dna1TotalWeight;
            return new System.Tuple<float, double>((float)nWeightsDiffering / (float)dna1.WeightsAndBiases.Count, weightDifferenceAsProportion);
        }
    }
}
