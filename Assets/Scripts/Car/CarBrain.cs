using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarBrain : Brain
{
    [SerializeField] protected Car carController = null;
    [SerializeField] private DistanceSensors distanceSensors = null;
    [SerializeField] private PhysicsSensors physicsSensors = null;

    public float BrakingDecision { get; private set; } = 0f;

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
        List<double> inputs = distanceSensors.CalculateNormalisedDistances()
            .Concat(physicsSensors.GetVelocityVectors())
            .Concat(physicsSensors.GetAngularVelocityVectors())
            .ToList();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        BrakingDecision = (float)outputs[2];
        DistanceCovered = LastGateCrossed.CalculateCumulativeDistance(transform.position);
    }

    protected override void Die()
    {
        base.Die();
        rend.material.color = Color.red;        
    }

    protected override void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            Gate g = other.GetComponent<Gate>();
            if (GatesCrossed + 1 == g.Number)
            {
                GatesCrossed++;
                LastGateCrossed = g;
                timeLastGateCrossed = Time.time;
            }
            else Die();
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
