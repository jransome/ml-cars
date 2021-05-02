using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
    Goal: go from 1 dimensional encoding => instance of nn

    Keeping the activation indexes in a separate list allows us to optionally mutate/evolve the activation functions,
    as well as prevent the functions of the output layer being mutated more easily.

    inputs: 4,
    outputs: 2,
    layers:[
        L1NeuronCount,
        L2NeuronCount,
        ...
    ]
    weights: [
        n1w1,
        n1w2,
        n1b,
        n2w1,
        n2w2,
        n2b,
        n3w1,
        n3w2,
        n3b,
        ...
    ],
    neuronActivationIndexes: [
        a1,
        a2,
        a3,
        ...
    ]
*/

namespace RansomeCorp.AI.Evolution
{
    public enum DnaHeritage
    {
        New,
        Elite,
        Offspring,
        MutatedOffspring,
        MutatedElite,
    }

    [System.Serializable]
    public class Dna : System.IEquatable<Dna>
    {
        public System.Action OnSelectedForBreedingCb { get; set; } = delegate { };
        public List<int> OutputsPerLayer; // includes the input 'layer'. Indicates the number of neurons per layer
        public List<double> WeightsAndBiases;
        public List<int> ActivationIndexes;
        public float RawFitnessRating = 1f;
        public DnaHeritage Heritage { get; private set; }

        public readonly int Id;
        static int idCounter = 0;

        private Dna(int[] outputsPerLayer, List<double> weightsAndBiases, List<int> activationIndexes, DnaHeritage origin = DnaHeritage.New)
        {
            Id = idCounter++;
            OutputsPerLayer = new List<int>(outputsPerLayer);
            WeightsAndBiases = new List<double>(weightsAndBiases);
            ActivationIndexes = new List<int>(activationIndexes);
            Heritage = origin;
        }

        public static Dna GenerateRandomDnaEncoding(int inputs, int[] hiddenLayersNeuronCount, int outputs, ActivationType activationType, bool heterogeneousHiddenActivation)
        {
            if (hiddenLayersNeuronCount.Length == 0) // TODO: add test to check if this is actually okay
                throw new System.ArgumentException("No hidden layers specified when generating new dna!");
            if (hiddenLayersNeuronCount.Any(count => count < 1))
                throw new System.ArgumentException("Hidden layers specified with less than 1 neuron!");

            int[] outputsPerLayer = new int[] { inputs }
                .Concat(hiddenLayersNeuronCount)
                .Concat(new int[] { outputs })
                .ToArray();

            int totalWeights = 0;
            int totalNeurons = 0;

            for (int layerIndex = 1; layerIndex < outputsPerLayer.Count(); layerIndex++)
            {
                totalWeights += outputsPerLayer[layerIndex - 1] * outputsPerLayer[layerIndex];
                totalNeurons += outputsPerLayer[layerIndex];
            }

            List<double> weightsAndBiases = new double[totalWeights + totalNeurons]
                .Select((_) => (double)Random.Range(-1f, 1f))
                .ToList();

            List<int> activationIndexes = new int[totalNeurons - outputs]
                .Select((_) => heterogeneousHiddenActivation ? Random.Range(0, Activation.FunctionsCount) : (int)activationType)
                .Concat(Enumerable.Repeat((int)activationType, outputs))
                .ToList();

            return new Dna(outputsPerLayer, weightsAndBiases, activationIndexes);
        }

        public static List<Dna> CreateOffspring(Dna parent1, Dna parent2, int crossoverPasses, bool includeActivationCrossover)
        {
            int weightsCount = parent1.WeightsAndBiases.Count;
            if (weightsCount != parent2.WeightsAndBiases.Count)
                Debug.LogError("Inter-species mating is happening... this hasn't been coded for!!");

            if (parent1.WeightsAndBiases.SequenceEqual(parent2.WeightsAndBiases))
                Debug.LogError("Tried to cross 2 identical parents!");

            // Sexi time
            parent1.OnSelectedForBreedingCb();
            parent2.OnSelectedForBreedingCb();

            var childWeightGenes = parent1.WeightsAndBiases.SinglePointCrossover(parent2.WeightsAndBiases, crossoverPasses);

            // Activation crossing may need rework. Currently independent of weight crossover so no adjacency effect exists
            var childActivationGenes = includeActivationCrossover ?
                parent1.ActivationIndexes.SinglePointCrossover(parent2.ActivationIndexes, crossoverPasses) :
                new List<List<int>>(2) { parent1.ActivationIndexes, parent2.ActivationIndexes };

            // Return children. Inputs, outputs and network structure should be the same in both parents
            return new List<Dna>()
            {
                new Dna(parent1.OutputsPerLayer.ToArray(), childWeightGenes[0], childActivationGenes[0], DnaHeritage.Offspring),
                new Dna(parent1.OutputsPerLayer.ToArray(), childWeightGenes[1], childActivationGenes[1], DnaHeritage.Offspring),
            };
        }

        public static Dna CloneAndMutate(Dna dna, DnaHeritage origin, float weightMutationPrevalence, float activationMutationPrevalence = 0)
        {
            if (!(weightMutationPrevalence > 0))
            {
                Debug.LogWarning("Attempted to mutate with a factor of zero");
                return Dna.Clone(dna);
            }

            List<double> mutatedWeightGene = dna.WeightsAndBiases.GuaranteedApplyToPercentage(weightMutationPrevalence, (weight) =>
            {
                double random01 = Random.Range(0f, 1f);
                bool shouldScramble = random01 < 0.25f;
                return shouldScramble ? (random01 * 2) - 1 : weight * (0.5 + random01); // scale by +/-50% OR random new value between -1 and 1
            });
            if (mutatedWeightGene.SequenceEqual(dna.WeightsAndBiases)) Debug.LogError("Failed to mutate weights???");

            List<int> activationGene;
            if (!(activationMutationPrevalence > 0)) activationGene = dna.ActivationIndexes;
            else
            {
                int startIndexOfOutputLayerActivation = dna.ActivationIndexes.Count - dna.OutputsPerLayer.Last();
                activationGene = dna.ActivationIndexes
                    .Take(startIndexOfOutputLayerActivation) // preserve the output layer activations
                    .ToList()
                    .GuaranteedApplyToPercentage(activationMutationPrevalence, (_) => Random.Range(0, Activation.FunctionsCount))
                    .Concat(dna.ActivationIndexes.Skip(startIndexOfOutputLayerActivation))
                    .ToList();
            }

            return new Dna(dna.OutputsPerLayer.ToArray(), mutatedWeightGene, activationGene, origin);
        }

        public static Dna Clone(Dna dna)
        {
            return new Dna(
                dna.OutputsPerLayer.ToArray(),
                new List<double>(dna.WeightsAndBiases),
                new List<int>(dna.ActivationIndexes),
                DnaHeritage.Elite
            );
        }

        public static bool TopologiesEqual(Dna dna1, Dna dna2)
        {
            return Enumerable.SequenceEqual(dna1.OutputsPerLayer, dna2.OutputsPerLayer);
        }

        public bool Equals(Dna other)
        {
            return Object.ReferenceEquals(this, other)
                || (TopologiesEqual(this, other)
                && ActivationIndexes.SequenceEqual(other.ActivationIndexes)
                && WeightsAndBiases.SequenceEqual(other.WeightsAndBiases));
        }

        // If Equals() returns true for a pair of objects then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // return the 'combined' hashcode of all pertinant fields by putting them in a tuple
            return (OutputsPerLayer, ActivationIndexes, WeightsAndBiases).GetHashCode();
        }
    }
}
