using System.Collections.Generic;
using UnityEngine;

public class Sensors : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 30f;

    public List<double> Distances { get; set; }

    public List<double> CalculateDistances()
    {
        Distances[0] = CheckDistance(-90);  // Left
        Distances[1] = CheckDistance(-45);  // Left-Fwd
        Distances[2] = CheckDistance(0);    // Fwd
        Distances[3] = CheckDistance(45);   // Right-Fwd
        Distances[4] = CheckDistance(90);   // Right
        return Distances;
    }

    private void Awake()
    {
        Distances = new List<double>()
        {
            raycastDistance,    // Left
            raycastDistance,    // Left-Fwd
            raycastDistance,    // Fwd
            raycastDistance,    // Right-Fwd
            raycastDistance,    // Right
        };
    }

    private float CheckDistance(float angle)
    {
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        RaycastHit hit;
        return Physics.Raycast(transform.position, direction, out hit, raycastDistance) ? hit.distance : raycastDistance;
    }
}
