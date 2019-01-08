using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFitness : MonoBehaviour
{
    [Header("Counters")]
    public float rawFitness = 0f;
    [SerializeField] private float gatesCrossed = 0; // TODO duplicated in Brain class
    // [SerializeField] private int terrainBumps = 0;
    // [SerializeField] private float cumulativeTerrainImpactVelocity = 0;

    [Header("Multipliers")]
    [SerializeField] private int gateCrossedReward = 10;
    [SerializeField] private int optimalDirectionReward = 10;
    [SerializeField] private int optimalPositionReward = 10;
    [SerializeField] private int maxPositionDifferenceTolerance = 15;

    public float Fitness { get { return rawFitness < 0 ? 0 : rawFitness; } }

    public void ResetFitness() 
    {
        rawFitness = 0f;
        gatesCrossed = 0;
        // terrainBumps = 0;
        // cumulativeTerrainImpactVelocity = 0f;
    }

    public void UpdateFitness(RacingGate gate)
    {
        // fitnesses squared to give greater weighting to slightly higher fitness scores
        rawFitness += Mathf.Pow(++gatesCrossed * gateCrossedReward, 2);
        float normalisedInverseDistance = (maxPositionDifferenceTolerance - Vector3.Distance(transform.position, gate.OptimalPosition)) / maxPositionDifferenceTolerance;
        rawFitness += Mathf.Pow(normalisedInverseDistance * optimalPositionReward, 2); 
        rawFitness += Mathf.Pow(Vector3.Dot(transform.forward, gate.OptimalDirection) * optimalDirectionReward, 2);
    }
}
