using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/*
    Goal: go from 1 dimensional encoding => instance of nn

    Encoding will be generated as per below.

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
    [System.Serializable]
    public struct Dna
    {
        public readonly int Inputs;
        public readonly int Outputs;
        public readonly ReadOnlyCollection<int> OutputsPerLayer;
        public readonly ReadOnlyCollection<double> WeightsAndBiases;
        public readonly ReadOnlyCollection<int> ActivationIndexes;

        public Dna(int inputs, int outputs, int[] outputsPerLayer, List<double> weightsAndBiases, List<int> activationIndexes)
        {
            Inputs = inputs;
            Outputs = outputs;
            OutputsPerLayer = new ReadOnlyCollection<int>(outputsPerLayer);
            WeightsAndBiases = new ReadOnlyCollection<double>(weightsAndBiases);
            ActivationIndexes = new ReadOnlyCollection<int>(activationIndexes);
        }
    }

    public class Darwin
    {
        public static Dna GenerateRandomDnaEncoding(int inputs, int[] hiddenLayersNeuronCount, int outputs, ActivationType activationType, bool heterogeneousHiddenActivation)
        {
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
                .Select((_) => (double)UnityEngine.Random.Range(-1f, 1f))
                .ToList();

            List<int> activationIndexes = new int[totalNeurons - outputs]
                .Select((_) => heterogeneousHiddenActivation ? UnityEngine.Random.Range(0, Activation.FunctionsCount) : (int)activationType)
                .Concat(Enumerable.Repeat((int)activationType, outputs))
                .ToList();

            return new Dna(inputs, outputs, outputsPerLayer, weightsAndBiases, activationIndexes);
        }
    }
}