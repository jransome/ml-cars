using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBrain : Brain
{
    [SerializeField] protected Car carController = null;

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
        List<double> inputs = sensors.CalculateNormalisedDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        DistanceCovered = LastGateCrossed.CalculateCumulativeDistance(transform.position);
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        carController.Throttle(ThrottleDecision);
        carController.Steer(SteeringDecision);
    }
}
