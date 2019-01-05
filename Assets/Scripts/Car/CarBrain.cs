using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarBrain : Brain
{
    [SerializeField] private Car carController = null;
    [SerializeField] private CarFitness fitnessCalculator = null;
    [SerializeField] private DistanceSensors distanceSensors = null;
    [SerializeField] private PhysicsSensors physicsSensors = null;
    [SerializeField] private RacingGate lastGateCrossed = null;

    public float BrakingDecision { get; private set; } = 0f;

    public override void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        BrakingDecision = 0;
        lastGateCrossed = null;
        fitnessCalculator.ResetFitness();
        base.Arise(startPosition, startRotation);
    }

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > GateSuicideThreshold) Die();
        List<double> inputs = distanceSensors.CalculateNormalisedDistances()
            .Concat(physicsSensors.GetVelocityVectors())
            .Concat(physicsSensors.GetAngularVelocityVector())
            .ToList();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        // BrakingDecision = (float)outputs[2];
        ChaseCameraOrderingVariable = fitnessCalculator.Fitness;
    }

    protected override void Die()
    {
        base.Die();
        carController.Brake(1f);
        rend.material.color = Color.red;        
    }

    protected override void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("RacingGate"))
        {
            timeLastGateCrossed = Time.time;
            RacingGate g = other.GetComponent<RacingGate>();
            if (g == lastGateCrossed)
            {
                Die();
                return;
            }
            lastGateCrossed = g;
            if (Vector3.Dot(transform.forward, g.OptimalDirection) < -0.5f) Die(); // if obviously going backwards
            else fitnessCalculator.UpdateFitness(g);
        }
    }

    protected override float CalculateFitness() => fitnessCalculator.Fitness;

    // private void OnCollisionEnter(Collision other) 
    // {
    //     if (other.collider.tag == "Terrain") 
    //     {
    //         terrainBumps++;
    //         cumulativeTerrainImpactVelocity += other.relativeVelocity.sqrMagnitude;
    //     }
    // }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        carController.Throttle(ThrottleDecision);
        carController.Steer(SteeringDecision);
        carController.Brake(BrakingDecision);
    }
}
