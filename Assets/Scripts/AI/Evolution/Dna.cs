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
        Unchanged,
        Bred,
        BredAndMutated,
        Mutated,
    }

    [System.Serializable]
    public class Dna : System.IEquatable<Dna>
    {
        public System.Action OnSelectedForBreedingCb { get; set; } = delegate { };
        public int Inputs; // TODO: use props as can't make readonly because need to serialise for saving
        public int Outputs;
        public List<int> OutputsPerLayer;
        public List<double> WeightsAndBiases;
        public List<int> ActivationIndexes;
        public float RawFitnessRating { get; set; } = 1f;
        public DnaHeritage Heritage { get; private set; }

        public readonly int Id;
        public readonly List<Dna> Parents;
        static int idCounter = 0;

        private Dna(int inputs, int outputs, int[] outputsPerLayer, List<double> weightsAndBiases, List<int> activationIndexes, DnaHeritage origin = DnaHeritage.New, List<Dna> parents = null)
        {
            Id = idCounter++;
            Parents = parents;

            Inputs = inputs;
            Outputs = outputs;
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

            return new Dna(inputs, outputs, outputsPerLayer, weightsAndBiases, activationIndexes);
        }

        public static List<Dna> CreateOffspring(Dna parent1, Dna parent2, bool useSinglePointCrossover, float weightCrossoverProportion = 0.5f, float activationCrossoverProportion = 0)
        {
            // Sexi time
            parent1.OnSelectedForBreedingCb();
            parent2.OnSelectedForBreedingCb();

            if (parent1.WeightsAndBiases.Count != parent2.WeightsAndBiases.Count)
                Debug.LogError("Inter-species mating is happening... this hasn't been coded for!!");

            var child1WeightGene = new List<double>();
            var child2WeightGene = new List<double>();
            var child1ActivationGene = new List<int>(parent1.ActivationIndexes);
            var child2ActivationGene = new List<int>(parent2.ActivationIndexes);

            if (useSinglePointCrossover)
            {
                int slicePoint = Random.Range(1, parent1.WeightsAndBiases.Count - 1);
                child1WeightGene = parent1.WeightsAndBiases.Take(slicePoint).Concat(parent2.WeightsAndBiases.Skip(slicePoint)).ToList();
                child2WeightGene = parent2.WeightsAndBiases.Take(slicePoint).Concat(parent1.WeightsAndBiases.Skip(slicePoint)).ToList();
            }
            else
            {
                for (int i = 0; i < Mathf.Min(parent1.WeightsAndBiases.Count, parent2.WeightsAndBiases.Count); i++)
                {
                    if (Random.Range(0f, 1f) <= weightCrossoverProportion)
                    {
                        child1WeightGene.Add(parent2.WeightsAndBiases[i]);
                        child2WeightGene.Add(parent1.WeightsAndBiases[i]);
                    }
                    else
                    {
                        child1WeightGene.Add(parent1.WeightsAndBiases[i]);
                        child2WeightGene.Add(parent2.WeightsAndBiases[i]);
                    }
                }

                if (activationCrossoverProportion > 0)
                {
                    child1ActivationGene = new List<int>();
                    child2ActivationGene = new List<int>();

                    for (int i = 0; i < Mathf.Min(parent1.ActivationIndexes.Count, parent2.ActivationIndexes.Count); i++)
                    {
                        if (Random.Range(0f, 1f) <= activationCrossoverProportion)
                        {
                            child1ActivationGene.Add(parent2.ActivationIndexes[i]);
                            child2ActivationGene.Add(parent1.ActivationIndexes[i]);
                        }
                        else
                        {
                            child1ActivationGene.Add(parent1.ActivationIndexes[i]);
                            child2ActivationGene.Add(parent2.ActivationIndexes[i]);
                        }
                    }
                }
            }

            // Return children. Inputs, outputs and network structure should be the same in both parents
            int inputs = parent1.Inputs;
            int outputs = parent1.Outputs;
            int[] outputsPerLayer = parent1.OutputsPerLayer.ToArray();

            bool p1p2 = parent1.WeightsAndBiases.SequenceEqual(parent2.WeightsAndBiases);
            bool p1c1 = parent1.WeightsAndBiases.SequenceEqual(child1WeightGene);
            bool p1c2 = parent1.WeightsAndBiases.SequenceEqual(child2WeightGene);
            bool p2c1 = parent2.WeightsAndBiases.SequenceEqual(child1WeightGene);
            bool p2c2 = parent2.WeightsAndBiases.SequenceEqual(child2WeightGene);

            if (p1p2)
                Debug.LogError("p1==c2");
            if (p1c1)
                Debug.LogError("p1==c1");
            if (p1c2)
                Debug.LogError("p1==c2");
            if (p2c1)
                Debug.LogError("p2==c1");
            if (p2c2)
                Debug.LogError("p2==c2");

            return new List<Dna>()
            {
                new Dna(inputs, outputs, outputsPerLayer, child1WeightGene, child1ActivationGene, DnaHeritage.Bred, new List<Dna>(){ parent1, parent2 }),
                new Dna(inputs, outputs, outputsPerLayer, child2WeightGene, child2ActivationGene, DnaHeritage.Bred, new List<Dna>(){ parent1, parent2 }),
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
                int startIndexOfOutputLayerActivation = dna.ActivationIndexes.Count - dna.Outputs;
                activationGene = dna.ActivationIndexes
                    .Take(startIndexOfOutputLayerActivation) // preserve the output layer activations
                    .ToList()
                    .GuaranteedApplyToPercentage(activationMutationPrevalence, (_) => Random.Range(0, Activation.FunctionsCount))
                    .Concat(dna.ActivationIndexes.Skip(startIndexOfOutputLayerActivation))
                    .ToList();
            }

            return new Dna(dna.Inputs, dna.Outputs, dna.OutputsPerLayer.ToArray(), mutatedWeightGene, activationGene, origin, new List<Dna>() { dna });
        }

        public static Dna Clone(Dna dna)
        {
            return new Dna(
                dna.Inputs,
                dna.Outputs,
                dna.OutputsPerLayer.ToArray(),
                new List<double>(dna.WeightsAndBiases),
                new List<int>(dna.ActivationIndexes),
                DnaHeritage.Unchanged,
                new List<Dna>() { dna }
            );
        }

        // Returns tuple containing 1) percentage of weight values that differ, and 2) the absolute difference in weight values of dna2 as a percentage of the aggregated absolute weight values of dna1
        public static System.Tuple<float, double> CompareWeights(Dna dna1, Dna dna2)
        {
            if (!TopologiesEqual(dna1, dna2))
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

        public static bool TopologiesEqual(Dna dna1, Dna dna2)
        {
            return dna1.Inputs == dna2.Inputs
                && dna1.Outputs == dna2.Outputs
                && Enumerable.SequenceEqual(dna1.OutputsPerLayer, dna2.OutputsPerLayer);
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
            return (Inputs, Outputs, OutputsPerLayer, ActivationIndexes, WeightsAndBiases).GetHashCode();
        }
    }
}
