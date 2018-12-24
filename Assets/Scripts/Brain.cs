using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private Sensors sensors = null;
    [SerializeField] private float thoughtInterval = 0.1f;
    private NeuralNetwork nn;

    public DNA Dna { get; set; }
    public bool IsThinking { get; set; } = true;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;

    private IEnumerator ThoughtProcess()
    {
        while(IsThinking)
        {
            Think();
            yield return new WaitForSeconds(thoughtInterval);
        }
    }

    private void Think()
    {
        List<double> inputs = sensors.CalculateDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];

        foreach (var o in outputs)
            Debug.Log(o);
    }

    private void Start()
    {
        Dna = new DNA(5, 2, 1, 5);
        nn = new NeuralNetwork(Dna);
        StartCoroutine(ThoughtProcess());
    }

    private void Update()
    {
        botController.Throttle(ThrottleDecision);
        botController.Steer(SteeringDecision);
    }
}
