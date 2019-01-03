using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBrain : Brain
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private DistanceSensors distanceSensors = null;

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
        List<double> inputs = distanceSensors.CalculateNormalisedDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        DistanceCovered = LastGateCrossed.CalculateCumulativeDistance(transform.position);
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        botController.Throttle(ThrottleDecision);
        botController.Steer(SteeringDecision);
    }
}
