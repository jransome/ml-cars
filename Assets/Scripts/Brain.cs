using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    private NeuralNetwork nn;
    private Sensors sensors;
    private Car carController;

    public DNA Dna { get; set; }

    public void Think()
    {
        double input1 = 1, input2 = 1;
        List<double> inputs = new List<double>() { input1, input2 };

        List<double> outputs = nn.Calculate(inputs);

        foreach (var o in outputs)
        {
            Debug.Log(o);
        }
    }

    private void Start()
    {
        Dna = new DNA(2, 1, 2, 5);
        nn = new NeuralNetwork(Dna);

        Think();
    }
}
