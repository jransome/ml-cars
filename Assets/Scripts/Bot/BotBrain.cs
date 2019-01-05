using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBrain : Brain
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private DistanceSensors distanceSensors = null;
    [SerializeField] private DistanceGate lastGateCrossed;

    public override void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        base.Arise(startPosition, startRotation);
        DistanceCovered = 0f;
        GatesCrossed = StartingGate;
        lastGateCrossed = DistanceGateManager.Instance.StartingGate;
    }

    protected override void Think()
    {
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
        List<double> inputs = distanceSensors.CalculateNormalisedDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        DistanceCovered = lastGateCrossed.CalculateCumulativeDistance(transform.position);
    }

    protected override void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain")) Die();
        else if (other.CompareTag("Gate"))
        {
            DistanceGate g = other.GetComponent<DistanceGate>();
            if (GatesCrossed + 1 == g.Number)
            {
                GatesCrossed++;
                lastGateCrossed = g;
                timeLastGateCrossed = Time.time;
            }
            else Die();
        }
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        botController.Throttle(ThrottleDecision);
        botController.Steer(SteeringDecision);
    }
}
