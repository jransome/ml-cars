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
    [SerializeField] private int gatesCrossed;
    [SerializeField] private RacingGate lastGateCrossed;

    private CarSpecies species;
    private Action callDeath;
    private bool deathCalled = false;

    public void Initialise(CarSpecies carSpecies, Action DieCallback)
    {
        deathCalled = false;
        species = carSpecies;
        colliderTrigger.TriggerEntered += HandleColliderTriggerEnter;
        callDeath = () =>
        {
            deathCalled = true;
            DieCallback();
        };
    }

    public void Reset()
    {
        deathCalled = false;
        rawFitness = 0f;
        timeOfBirth = Time.time;
        lastGateCrossedTime = timeOfBirth;
        gatesCrossed = 0;
        lastGateCrossed = null;
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
        if (gate == lastGateCrossed)
        {
            callDeath();
            return;
        }

        lastGateCrossed = gate;
        lastGateCrossedTime = Time.time;

        // fitnesses squared to give greater weighting to slightly higher fitness scores
        rawFitness += Mathf.Pow(++gatesCrossed * species.GateCrossedReward, 2);
        // float normalisedInverseDistance = (species.MaxPositionDifferenceTolerance - Vector3.Distance(transform.position, gate.OptimalPosition)) / species.MaxPositionDifferenceTolerance;
        // rawFitness += Mathf.Pow(normalisedInverseDistance * species.OptimalPositionReward, 2);
        rawFitness += Mathf.Pow(Vector3.Dot(transform.forward, gate.OptimalDirection) * species.OptimalDirectionReward, 2);
    }

    private void HandleColliderTriggerEnter(Collider other)
    {
        if (other.CompareTag("RacingGate"))
            UpdateFitness(other.GetComponent<RacingGate>());
    }

    private void Update()
    {
        UpdateFitness(Time.time);
    }
}