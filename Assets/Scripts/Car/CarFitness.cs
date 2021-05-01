using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CarFitness : MonoBehaviour // TODO: refactor as plain class?
{
    public float Fitness { get { return Mathf.Max(1, rawFitness); } }
    public bool IsAlive { get; private set; } = false;

    [SerializeField] private ColliderTrigger colliderTrigger = null;

    [Header("Counters (readonly)")]
    [SerializeField] private float rawFitness;
    [SerializeField] private float timeOfBirth;
    [SerializeField] private float lastGateCrossedTime;
    [SerializeField] private List<RacingGate> gatesCrossed = new List<RacingGate>();

    private CarSpecies species;
    private bool initialised = false;
    private Action callDeath;
    private bool deathCalled = false;

    public void Initialise(CarSpecies carSpecies, Action DieCallback)
    {
        deathCalled = false;
        species = carSpecies;
        colliderTrigger.TriggerEntered += HandleColliderTriggerEnter;
        callDeath = () =>
        {
            if (deathCalled) return;
            deathCalled = true;
            DieCallback();
        };
        initialised = true;
    }

    public void Reset()
    {
        deathCalled = false;
        rawFitness = 0f;
        timeOfBirth = Time.time;
        lastGateCrossedTime = timeOfBirth;
        gatesCrossed.Clear();
    }

    private void UpdateFitness(float currentTime)
    {
        if (deathCalled) return;
        if (species.MaxLifeSpanSecs > 0 && currentTime - timeOfBirth > species.MaxLifeSpanSecs) callDeath();
        if (currentTime - lastGateCrossedTime > species.MaxTimeToReachNextGateSecs) callDeath();
    }

    private void UpdateFitness(RacingGate gate)
    {
        if (deathCalled) return;
        if (gatesCrossed.Contains(gate))
        {
            callDeath();
            return;
        }

        gatesCrossed.Add(gate);
        lastGateCrossedTime = Time.time;

        // fitnesses squared to give greater weighting to slightly higher fitness scores
        rawFitness += Mathf.Pow(gatesCrossed.Count * species.GateCrossedReward, 2);
        float normalisedInverseDistance = (species.MaxPositionDifferenceTolerance - Vector3.Distance(transform.position, gate.OptimalPosition)) / species.MaxPositionDifferenceTolerance;
        rawFitness += Mathf.Pow(normalisedInverseDistance * species.OptimalPositionReward, 2);
        rawFitness += Mathf.Pow(Mathf.Abs(Vector3.Dot(transform.forward, gate.OptimalDirection)) * species.OptimalDirectionReward, 2);
    }

    private void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("RacingGate"))
            UpdateFitness(other.GetComponent<RacingGate>());
    }

    private void OnCollisionEnter(Collision col)
    {
        if (deathCalled) return;
        if (col.gameObject.tag == "Terrain")
        {
            // callDeath();
            // rawFitness -= 20000;
        }
    }

    private void Update()
    {
        if (initialised) UpdateFitness(Time.time);
    }
}