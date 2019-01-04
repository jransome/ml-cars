using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarBrain : Brain
{
    [SerializeField] protected Car carController = null;
    [SerializeField] private DistanceSensors distanceSensors = null;
    [SerializeField] private PhysicsSensors physicsSensors = null;
    [SerializeField] private int terrainBumps = 0;
    [SerializeField] private float cumulativeTerrainImpactVelocity = 0;

    public float BrakingDecision { get; private set; } = 0f;

    public override void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        base.Arise(startPosition, startRotation);
        terrainBumps = 0;
        cumulativeTerrainImpactVelocity = 0f;
    }

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
        List<double> inputs = distanceSensors.CalculateNormalisedDistances()
            .Concat(physicsSensors.GetVelocityVectors())
            .Concat(physicsSensors.GetAngularVelocityVector())
            .ToList();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        // BrakingDecision = (float)outputs[2];
        DistanceCovered = LastGateCrossed.CalculateCumulativeDistance(transform.position);
    }

    protected override void Die()
    {
        base.Die();
        carController.Brake(1f);
        rend.material.color = Color.red;        
    }

    protected override void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            // Gate g = other.GetComponent<Gate>();
            // if (GatesCrossed + 1 == g.Number)
            // {
            //     GatesCrossed++;
            //     LastGateCrossed = g;
            //     timeLastGateCrossed = Time.time;
            // }
            // else Die();
        }
    }

    protected override float CalculateFitness() => DistanceCovered > 0 ? Mathf.Pow(DistanceCovered, 2) : 0;

    private void OnCollisionEnter(Collision other) 
    {
        if (other.collider.tag == "Terrain") 
        {
            // terrainBumps++;
            // cumulativeTerrainImpactVelocity += other.relativeVelocity.sqrMagnitude;
        }
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        carController.Throttle(ThrottleDecision);
        carController.Steer(SteeringDecision);
        carController.Brake(BrakingDecision);
    }
}
