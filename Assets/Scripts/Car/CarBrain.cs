using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class CarBrain : MonoBehaviour
{
    [SerializeField] private AgentController agentController;
    [SerializeField] private CarFitness fitnessCalculator;
    [SerializeField] private DistanceSensors distanceSensors;
    [SerializeField] private PhysicsSensors physicsSensors;
    [SerializeField] private Renderer speciesIndicator;
    [SerializeField] private Renderer heritageIndicator;

    private Action<CarBrain> OnDeathCb = delegate { };
    private NeuralNetwork neuralNetwork;
    public Dna Dna { get { return neuralNetwork.Dna; } }
    public bool IsAlive { get; private set; } = false;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;
    public float BrakingDecision { get; private set; } = 0f;
    public float Fitness { get => fitnessCalculator.Fitness; }

    public void Initialise(CarSpecies species, Vector3 startPosition, Quaternion startRotation, Action<CarBrain> onDeathCb)
    {
        IsAlive = false;
        OnDeathCb = onDeathCb;
        distanceSensors.Initialise(species);
        fitnessCalculator.Initialise(species, this.Die);
        speciesIndicator.material.color = species.SpeciesColour;
    }

    public void Arise(Dna dna, Vector3 startPosition, Quaternion startRotation)
    {
        if (IsAlive)
            Debug.LogWarning("Brain was not dead when reset"); // expected when loading a save

        if (GetSensorInputs().Count != dna.Inputs)
            Debug.LogError("Network not configured with expected number of inputs. Received " + GetSensorInputs().Count + ", expected " + dna.Inputs);

        if (dna.Outputs != CarSpecies.Outputs)
            Debug.LogError("Network not configured with expected number of outputs");

        ThrottleDecision = SteeringDecision = BrakingDecision = 0;
        agentController.ResetToPosition(startPosition, startRotation);
        fitnessCalculator.Reset();

        neuralNetwork = new NeuralNetwork(dna);
        heritageIndicator.material.color = CarSpecies.LineageColours[dna.Heritage];
        IsAlive = true;
        StartCoroutine(ThoughtProcess());
    }

    private IEnumerator ThoughtProcess()
    {
        while (IsAlive)
        {
            List<double> outputs = neuralNetwork.Think(GetSensorInputs());
            bool isThrottling = outputs[1] > 0;
            SteeringDecision = (float)outputs[0];
            ThrottleDecision = isThrottling ? (float)outputs[1] : 0f;
            BrakingDecision = isThrottling ? 0f : -(float)outputs[1];

            // sigmoid
            // var tOutputMapped = (outputs[1] - 0.5) * 2;
            // bool isThrottling = tOutputMapped > 0;
            // SteeringDecision = ((float)outputs[0] - 0.5f) * 2;
            // ThrottleDecision = isThrottling ? (float)outputs[1] : 0f;
            // BrakingDecision = isThrottling ? 0f : -(float)outputs[1];

            yield return new WaitForSeconds(0.1f);
        }
    }

    private List<double> GetSensorInputs() => distanceSensors.CalculateNormalisedDistances()
        .Concat(physicsSensors.GetVelocityVectors())
        .Concat(physicsSensors.GetAngularVelocityVector())
        .ToList();

    private void Die()
    {
        if (!IsAlive) Debug.LogError("Tried to kill brain but was already dead?");
        if (!IsAlive) return;

        IsAlive = false;
        Dna.RawFitnessRating = fitnessCalculator.Fitness;
        StopAllCoroutines();
        OnDeathCb(this);
        agentController.Brake(1f);
        heritageIndicator.material.color = Color.red;
    }

    private void Update()
    {
        if (!IsAlive) return;
        agentController.Throttle(ThrottleDecision);
        agentController.Steer(SteeringDecision);
        agentController.Brake(BrakingDecision);
    }
}
