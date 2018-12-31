﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    public double WeightSumFingerprint; // debugging

    [SerializeField] private Bot botController = null;
    [SerializeField] private Sensors sensors = null;
    [SerializeField] private Renderer botRenderer = null;
    [SerializeField] private float thoughtInterval = 0.1f;
    [SerializeField] private DnaHeritage heritage; // For debugging in inspector
    private NeuralNetwork nn;
    private float timeOfBirth;
    private float timeLastGateCrossed;

    public Dna Dna { get; private set; }
    public bool IsAlive { get; set; } = false;
    public float ThrottleDecision { get; private set; } = 0f;
    public float SteeringDecision { get; private set; } = 0f;

    public float LifeSpan { get; private set; }
    public float DistanceCovered; //{ get; private set; }
    public int GatesCrossed { get; private set; }
    public Gate LastGateCrossed { get; private set; }
    public float SuicideThreshold { get; set; } = 5f;

    public event Action<Brain, float> Died = delegate { };

    public void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        WeightSumFingerprint = Dna.WeightSumFingerprint;
        if (IsAlive) Debug.LogWarning("Brain was not dead when reset");
        transform.localScale = Vector3.one;
        transform.position = startPosition;
        transform.rotation = startRotation;
        heritage = Dna.Heritage;
        botRenderer.material.color = God.LineageColours[Dna.Heritage];

        LifeSpan = 0f;
        DistanceCovered = 0f;
        GatesCrossed = 0;
        LastGateCrossed = GateManager.Instance.StartingGate;

        IsAlive = true;
        timeOfBirth = Time.time;
        timeLastGateCrossed = Time.time;

        StartCoroutine(ThoughtProcess());
    }

    private void OnSelectForBreeding()
    {
        transform.localScale += Vector3.up;
    }

    public void ReplaceDna(Dna dna)
    {
        if (nn == null) 
            nn = new NeuralNetwork(dna);
        else
        {
            Dna.SelectedForBreeding -= OnSelectForBreeding;
            nn.ReplaceDna(dna);
        }

        Dna = dna;
        Dna.SelectedForBreeding += OnSelectForBreeding;
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
        if (Time.time - timeLastGateCrossed > SuicideThreshold) Die();
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
        // transform.localScale += Vector3.up * DistanceCovered; // debugging
        // float fitness = 1;
        float fitness = CalculateFitness();
        Dna.Fitness = fitness;
        Died(this, fitness);
    }

    private float CalculateFitness() => DistanceCovered > 0 ? Mathf.Pow(DistanceCovered, 2) : 0;

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
