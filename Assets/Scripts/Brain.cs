using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    private NeuralNetwork nn;

    public DNA Dna { get; set; }

    private void OnEnable()
    {
        nn = new NeuralNetwork(2, 1, 2);

        double input1 = 1, input2 = 1;
        List<double> inputs = new List<double>() { input1, input2 };

        List<double> outputs = nn.Calculate(inputs);

        foreach (var o in outputs)
        {
            Debug.Log(o);
        }
    }
}
