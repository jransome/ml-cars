using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RansomeCorp.AI.Evolution
{
    [System.Serializable]
    public class Dna
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

        // public Dna Clone()
        // {
        //     return new Dna(
        //         Inputs,
        //         Outputs,
        //         OutputsPerLayer.ToArray(),
        //         new List<double>(WeightsAndBiases),
        //         new List<int>(ActivationIndexes)
        //     );
        // }
    }
}
