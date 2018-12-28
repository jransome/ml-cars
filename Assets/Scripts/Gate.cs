using System;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public int Number { get; set; }
    public Vector3 DirectionToNext { get; set; }
    public float DistanceToNext { get; set; }

    public float CalculateDistanceTo(Vector3 position)
    {
        // Calculates the distance a point is away from this gate along a line stretching from this gate to the next gate
        float adjacentToHypotenuseRadians = Vector3.Angle(DirectionToNext, position - transform.position) * (Mathf.PI / 180);
        return Mathf.Cos(adjacentToHypotenuseRadians) * Vector3.Distance(position, transform.position);
    }

    private void Awake()
    {
        Number = Convert.ToInt32(gameObject.name.Split(new char[] { '(', ')' })[1]);
    }

    // used for setting up gates
    // private void OnDrawGizmos() 
    // {
    //     Gizmos.DrawRay(transform.position, transform.forward * 50); 
    // }
}
