using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotBrain : Brain
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private DistanceSensors distanceSensors = null;
    [SerializeField] private DistanceGate lastGateCrossed;
    [SerializeField] private float distanceCovered;

    public override void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        distanceCovered = 0f;
        GatesCrossed = StartingGate;
        lastGateCrossed = DistanceGateManager.Instance.StartingGate;
        base.Arise(startPosition, startRotation);
    }

    protected override void Think()
    {
        List<double> inputs = distanceSensors.CalculateNormalisedDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
        distanceCovered = lastGateCrossed.CalculateCumulativeDistance(transform.position);
        ChaseCameraOrderingVariable = distanceCovered;
    }

    protected override float CalculateFitness() => distanceCovered > 0 ? Mathf.Pow(distanceCovered / 10, 2) : 0;

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
