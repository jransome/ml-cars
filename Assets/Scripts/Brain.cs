using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private Sensors sensors = null;
    [SerializeField] private float thoughtInterval = 0.1f;
    private NeuralNetwork nn;
    private float timeOfBirth;

    public DNA Dna { get; set; }
    public bool IsAlive { get; set; } = true;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;

    public float LifeSpan { get; private set; }
    public int GatesCrossed { get; private set; }

    private IEnumerator ThoughtProcess()
    {
        while(IsAlive)
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
    }

    private void Die()
    {
        IsAlive = false;
        LifeSpan = Time.time - timeOfBirth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain")) Die();
        else if (other.CompareTag("Gate"))
        {
            if (GatesCrossed + 1 == other.GetComponent<Gate>().Number)
                GatesCrossed++;
            else
                Die();
        }
    }

    private void Start()
    {
        Dna = new DNA(5, 2, 1, 5);
        nn = new NeuralNetwork(Dna);
        timeOfBirth = Time.time;
        StartCoroutine(ThoughtProcess());
    }

    private void Update()
    {
        if (!IsAlive) return;
        botController.Throttle(ThrottleDecision);
        botController.Steer(SteeringDecision);
    }
}
