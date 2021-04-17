using RansomeCorp.AI.Evolution;
using RansomeCorp.AI.NeuralNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class CarBrain : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private CarFitness fitnessCalculator;
    [SerializeField] private DistanceSensors distanceSensors;
    [SerializeField] private PhysicsSensors physicsSensors;
    [SerializeField] private Renderer heritageIndicator;

    private Action<CarBrain, float> OnDeathCb = delegate { };
    private NeuralNetwork neuralNetwork;
    public Dna Dna { get { return neuralNetwork.Dna; } }
    public bool IsAlive { get; private set; } = false;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;
    public float BrakingDecision { get; private set; } = 0f;
    public float Fitness { get => fitnessCalculator.Fitness; }

    public void Initialise(CarSpecies species, Vector3 startPosition, Quaternion startRotation, Action<CarBrain, float> onDeathCb)
    {
        IsAlive = false;
        OnDeathCb = onDeathCb;
        distanceSensors.Initialise(species);
        fitnessCalculator.Initialise(species, this.Die);
    }

    public void Arise(Dna dna, Vector3 startPosition, Quaternion startRotation)
    {
        if (IsAlive)
            Debug.LogError("Brain was not dead when reset");

        if (GetSensorInputs().Count != dna.Inputs)
            Debug.LogError("Network not configured with expected number of inputs. Received " + GetSensorInputs().Count + ", expected " + dna.Inputs);

        if (dna.Outputs != 3)
            Debug.LogError("Network not configured with expected number of outputs");

        ThrottleDecision = SteeringDecision = BrakingDecision = 0;
        carController.ResetToPosition(startPosition, startRotation);
        fitnessCalculator.Reset();

        neuralNetwork = new NeuralNetwork(dna);
        heritageIndicator.material.color = CarSpecies.LineageColours[dna.Heritage];
        IsAlive = true;
        StartCoroutine(ThoughtProcess());
    }

    private IEnumerator ThoughtProcess()
    {
        const float thoughtInterval = 0.1f;
        while (IsAlive)
        {
            fitnessCalculator.UpdateFitness(Time.time);

            List<double> outputs = neuralNetwork.Think(GetSensorInputs());
            ThrottleDecision = (float)outputs[0];
            SteeringDecision = (float)outputs[1];
            BrakingDecision = (float)outputs[2];

            // ChaseCameraOrderingVariable = fitnessCalculator.Fitness;
            yield return new WaitForSeconds(thoughtInterval);
        }
    }

    private List<double> GetSensorInputs() => distanceSensors.CalculateNormalisedDistances()
        .Concat(physicsSensors.GetVelocityVectors())
        .Concat(physicsSensors.GetAngularVelocityVector())
        .ToList();

    private void Die()
    {
        if (!IsAlive) Debug.LogError("Tried to kill brain but was already dead?");
        IsAlive = false;
        StopAllCoroutines();
        OnDeathCb(this, fitnessCalculator.Fitness);
        carController.Brake(1f);
        heritageIndicator.material.color = Color.red;
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        carController.Throttle(ThrottleDecision);
        carController.Steer(SteeringDecision);
        carController.Brake(BrakingDecision);
    }
}
