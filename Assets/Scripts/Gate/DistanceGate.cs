using System;
using UnityEngine;

public class DistanceGate : MonoBehaviour
{
    public int Number { get; private set; }
    public Vector3 DirectionToNext { get; set; }
    public float CumulativeDistance;

    public float CalculateCumulativeDistance(Vector3 position) => CumulativeDistance + CalculateDistanceTo(position);

    private float CalculateDistanceTo(Vector3 position)
    {
        // Calculates the distance a point is away from this gate along a line stretching from this gate to the next gate
        float adjacentToHypotenuseRadians = Vector3.Angle(DirectionToNext, position - transform.position) * (Mathf.PI / 180);
        return Mathf.Cos(adjacentToHypotenuseRadians) * Vector3.Distance(position, transform.position);
    }

    public int GetNumber()
    {
        Number = Convert.ToInt32(gameObject.name.Split(new char[] { '(', ')' })[1]);
        return Number;
    }

    // used for setting up gates
    // private void OnDrawGizmos() 
    // {
    //     Gizmos.DrawRay(transform.position, transform.forward * 50); 
    // }
}
