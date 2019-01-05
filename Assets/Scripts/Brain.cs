using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Brain : MonoBehaviour
{
    [SerializeField] protected ColliderTrigger colliderTrigger = null;
    [SerializeField] protected Renderer rend = null;
    [SerializeField] protected float thoughtInterval = 0.1f;
    [SerializeField] protected DnaHeritage heritage; // For debugging in inspector
    protected NeuralNetwork nn;
    protected float timeOfBirth;
    protected float timeLastGateCrossed;

    public Dna Dna { get; protected set; }
    public bool IsAlive { get; set; }
    public float ThrottleDecision { get; protected set; } = 0f;
    public float SteeringDecision { get; protected set; } = 0f;

    public float ChaseCameraOrderingVariable { get; protected set; } // TODO: change this
    public float LifeSpan { get; protected set; }
    public float MaxLifeSpan { get; set; } = 0f;
    public int StartingGate { get; set; }
    public int GatesCrossed { get; protected set; }
    public float GateSuicideThreshold { get; set; } = 5f;

    public event Action<Brain, float> Died = delegate { };

    public virtual void Arise(Vector3 startPosition, Quaternion startRotation)
    {
        if (IsAlive) Debug.LogWarning("Brain was not dead when reset");
        transform.localScale = Vector3.one;
        transform.position = startPosition;
        transform.rotation = startRotation;
        heritage = Dna.Heritage;
        rend.material.color = God.LineageColours[Dna.Heritage];
        LifeSpan = 0f;

        IsAlive = true;
        timeOfBirth = Time.time;
        timeLastGateCrossed = Time.time;

        StartCoroutine(ThoughtProcess());
    }

    protected void OnSelectForBreeding()
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

    protected IEnumerator ThoughtProcess()
    {
        while(IsAlive)
        {
            if (MaxLifeSpan > 0 && LifeSpan > MaxLifeSpan) Die();
            if (Time.time - timeLastGateCrossed > GateSuicideThreshold) Die();
            Think();
            LifeSpan += thoughtInterval;
            yield return new WaitForSeconds(thoughtInterval);
        }
    }

    protected abstract void Think();

    protected virtual void Die()
    {
        if (!IsAlive) return;
        IsAlive = false;
        LifeSpan = Time.time - timeOfBirth;
        float fitness = CalculateFitness();
        Dna.Fitness = fitness;
        Died(this, fitness);
    }

    protected abstract float CalculateFitness();

    protected abstract void HandleColliderTriggerEnter(Collider other);

    protected void Start() 
    {
        colliderTrigger.TriggerEntered += HandleColliderTriggerEnter;
    }
}
