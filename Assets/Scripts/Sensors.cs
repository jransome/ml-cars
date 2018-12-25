using System.Collections.Generic;
using UnityEngine;

public class Sensors : MonoBehaviour
{
    [SerializeField] private float raycastDistance = 15f;

    public List<double> Distances { get; set; }

    public List<double> CalculateDistances() // TODO: Normalise?
    {
        Distances[0] = CheckDistance(-90);  // Left
        Distances[1] = CheckDistance(-45);  // Left-Fwd
        Distances[2] = CheckDistance(0);    // Fwd
        Distances[3] = CheckDistance(45);   // Right-Fwd
        Distances[4] = CheckDistance(90);   // Right
        return Distances;
    }

    private float CheckDistance(float angle)
    {
        Vector3 direction = CalculateDirectionFromAngle(angle);
        RaycastHit hit;
        return Physics.Raycast(transform.position, direction, out hit, raycastDistance) ? hit.distance : raycastDistance;
    }

    private Vector3 CalculateDirectionFromAngle(float angle) => Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

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

    private void Update()
    {
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(-90) * raycastDistance, GetColour(Distances[0]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(-45) * raycastDistance, GetColour(Distances[1]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(0) * raycastDistance, GetColour(Distances[2]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(45) * raycastDistance, GetColour(Distances[3]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(90) * raycastDistance, GetColour(Distances[4]));
    }

    private Color GetColour(double distance) => distance < raycastDistance ? Color.red : Color.green;
}
