using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RansomeCorp.AI.Evolution
{
    [System.Serializable]
    public class Generation
    {
        public int GenerationNumber;
        public int SpawnLocationIndex;
        public List<Dna> GenePool;
        public GenerationPerformanceData PerformanceData;
        public Dictionary<DnaHeritage, int> Composition = new Dictionary<DnaHeritage, int>();
        CarSpecies species;

        private Generation(CarSpecies species, int generationNumber, int spawnIndex, IEnumerable<Dna> genes = null)
        {
            GenerationNumber = generationNumber;
            SpawnLocationIndex = spawnIndex;
            GenePool = new List<Dna>(species.GenerationSize);
            this.species = species;
        }

        public static Generation CreateSeed(CarSpecies species, int spawnLocationIndex)
        {
            Generation seed = new Generation(species, 0, spawnLocationIndex);
            seed.AddMultipleDna(GenerateRandomDna(species, species.GenerationSize), false);
            return seed;
        }

        public static Generation FromPrevious(Generation previous, int spawnLocationIndex)
        {
            CarSpecies species = previous.species;
            List<Dna> previousGenePool = previous.GenePool;
            Generation TNG = new Generation(species, previous.GenerationNumber + 1, spawnLocationIndex);

            // Create and add fresh dna into next gen
            int nNew = Mathf.RoundToInt(species.GenerationSize * species.NewDnaRate);
            if (nNew > 0)
                TNG.AddMultipleDna(GenerateRandomDna(species, nNew), false);

            //  Preserve elites
            var previousGenerationOrderedByFitness = previousGenePool.OrderByDescending((dna => dna.RawFitnessRating));
            int nElites = Mathf.RoundToInt(species.GenerationSize * species.ProportionUnchanged);
            if (nElites > 0)
                TNG.AddMultipleDna(previousGenerationOrderedByFitness.Take(nElites).Select(dna => Dna.Clone(dna)), true);

            // Add mutated versions of elites
            int nMutatedElites = Mathf.RoundToInt(species.GenerationSize * species.ProportionMutatantsOfUnchanged);
            if (((species.GenerationSize - (nElites + nMutatedElites + nNew)) % 2 == 1)) nMutatedElites++; // make sure remaining spaces for offspring is an even number
            for (int i = 0; i < nMutatedElites; i++)
            {
                Dna randomElite = Darwin.SelectRandomBasedOnFitness(
                    previousGenerationOrderedByFitness.Take(Mathf.RoundToInt(species.GenerationSize * 0.2f)).ToList()
                );
                Dna mutatedElite = Dna.CloneAndMutate(randomElite, DnaHeritage.MutatedElite, species.MutationSeverity, species.ActivationMutationSeverity);
                TNG.AddDna(mutatedElite, true);
            }

            // Populate the rest with offspring of previous
            int nMutatedOffspring = 0;
            int freeSpacesForOffspring = species.GenerationSize - (nElites + nMutatedElites + nNew);
            int targetNumMutatedOffspring = Mathf.RoundToInt(freeSpacesForOffspring * species.OffspringMutationProbability);
            for (int i = 0; i < Mathf.RoundToInt(freeSpacesForOffspring / 2); i++)
            {
                Dna parent1 = Darwin.SelectRandomBasedOnFitness(previousGenePool);
                Dna parent2 = Darwin.SelectRandomBasedOnFitness(previousGenePool, parent1);
                List<Dna> children = new List<Dna>(2);

                /* Attempts at crossover may fail if the genomes of the 2 parents are too similar. If for example:
                    p1 = 1,2,3,4
                    p2 = 1,2,3,5
                then no matter how we try to cross them, the children will end up as clones of the parents. To mitigate 
                this we try a few times and if we consistently fail, then we mutate the dna as a fallback
                */
                bool crossoverFailed = false;
                for (int crossoverAttempt = 0; crossoverAttempt < 5; crossoverAttempt++)
                {
                    children = Dna.CreateOffspring(
                        parent1,
                        parent2,
                        species.CrossoverPasses,
                        species.IncludeActivationCrossover
                    );
                    crossoverFailed = parent1.Equals(children[0]) || parent1.Equals(children[1]) || parent2.Equals(children[0]) || parent2.Equals(children[1]) || children[0].Equals(children[1]);
                    if (!crossoverFailed) break;
                }
                if (crossoverFailed) Debug.LogWarning("Crossover failed after several attempts - selected parent genomes are likely too similar");

                if (crossoverFailed || (nMutatedOffspring <= targetNumMutatedOffspring && Random.Range(0f, 1f) < species.OffspringMutationProbability))
                {
                    TNG.AddMultipleDna(children.ConvertAll(c => Dna.CloneAndMutate(c, DnaHeritage.MutatedOffspring, species.MutationSeverity, species.ActivationMutationSeverity)), true);
                    nMutatedOffspring += 2;
                }
                else
                {
                    TNG.AddMultipleDna(children, true);
                }
            }

            DnaUtils.DebugGenerationDiff(previousGenePool, TNG.GenePool);
            return TNG;
        }

        private static List<Dna> GenerateRandomDna(CarSpecies species, int number)
        {
            return Enumerable.Range(0, number).Select((_) =>
                Dna.GenerateRandomDnaEncoding(
                    species.Inputs,
                    species.HiddenLayersNeuronCount,
                    CarSpecies.Outputs,
                    species.OutputLayerActivation,
                    species.HeterogeneousHiddenActivation
                )
            ).ToList();
        }

        public void AddDna(Dna newDna, bool enforceDistinctive)
        {
            Dna addition = enforceDistinctive && GenePool.Any(dna => dna.Equals(newDna)) ?
                Dna.CloneAndMutate(newDna, newDna.Heritage, species.MutationSeverity, species.ActivationMutationSeverity) :
                newDna;

            GenePool.Add(addition);
            Composition.TryGetValue(addition.Heritage, out int count);
            Composition[addition.Heritage] = count + 1;
        }

        public void AddMultipleDna(IEnumerable<Dna> additions, bool enforceDistinctive)
        {
            foreach (Dna addition in additions)
                AddDna(addition, enforceDistinctive);
        }

        public void Finish()
        {
            PerformanceData = new GenerationPerformanceData(GenePool);
        }
    }

    [System.Serializable]
    public struct GenerationPerformanceData
    {
        public float TotalFitness;
        public float BestFitness;
        public float AverageFitness;

        public GenerationPerformanceData(List<Dna> genePool)
        {
            TotalFitness = BestFitness = 0f;

            foreach (var dna in genePool)
            {
                TotalFitness += dna.RawFitnessRating;
                if (dna.RawFitnessRating > BestFitness) BestFitness = dna.RawFitnessRating;
            }

            AverageFitness = TotalFitness / genePool.Count;
        }
    }
}
