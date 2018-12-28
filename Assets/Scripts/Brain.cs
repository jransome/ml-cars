﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    [SerializeField] private Bot botController = null;
    [SerializeField] private Sensors sensors = null;
    [SerializeField] private Renderer renderer = null;
    [SerializeField] private float thoughtInterval = 0.1f;
    [SerializeField] private float suicideThreshold = 20f;
    [SerializeField] private DnaHeritage heritage; // For debugging in inspector
    private NeuralNetwork nn;
    private float timeOfBirth;
    private float timeLastGateCrossed;

    public Dna Dna { get; private set; }
    public bool IsAlive { get; set; } = false;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;

    public float Fitness { get; set; }
    public float LifeSpan { get; private set; }
    public float DistanceCovered { get; set; }
    public int GatesCrossed { get; private set; }
    public Gate LastGateCrossed { get; private set; }

    public event Action<Brain> Died = delegate { };

    public void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        if (IsAlive) Debug.LogError("Brain was not dead when reset");
        transform.position = startPosition;
        transform.rotation = startRotation;
        heritage = Dna.Heritage;
        renderer.material.color = God.LineageColours[Dna.Heritage];

        Fitness = 0f;
        LifeSpan = 0f;
        DistanceCovered = 0f;
        GatesCrossed = 0;
        LastGateCrossed = GateManager.Instance.StartingGate;

        IsAlive = true;
        timeOfBirth = Time.time;
        timeLastGateCrossed = Time.time;

        StartCoroutine(ThoughtProcess());
    }

    public void ReplaceDna(Dna dna)
    {
        Dna = dna;
        if (nn == null) 
            nn = new NeuralNetwork(Dna);
        else
            nn.ReplaceDna(dna);
    }

    private IEnumerator ThoughtProcess()
    {
        while(IsAlive)
        {
            Think();
            yield return new WaitForSeconds(thoughtInterval);
        }
    }

    private void Think()
    {
        if (Time.time - timeLastGateCrossed > suicideThreshold) Die();
        List<double> inputs = sensors.CalculateNormalisedDistances();
        List<double> outputs = nn.Calculate(inputs);
        ThrottleDecision = (float)outputs[0];
        SteeringDecision = (float)outputs[1];
    }

    private void Die()
    {
        IsAlive = false;
        LifeSpan = Time.time - timeOfBirth;
        DistanceCovered += LastGateCrossed.CalculateDistanceTo(transform.position);
        Died(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain")) Die();
        else if (other.CompareTag("Gate"))
        {
            Gate g = other.GetComponent<Gate>();
            if (GatesCrossed + 1 == g.Number)
            {
                DistanceCovered += LastGateCrossed.DistanceToNext;
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
        botController.Throttle(ThrottleDecision);
        botController.Steer(SteeringDecision);
    }
}
