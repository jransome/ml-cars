using System.Collections.Generic;
using UnityEngine;

public class DistanceSensors : MonoBehaviour
{
    public bool DrawSensors = false;
    [SerializeField] private float raycastDistance = 15f;

    public List<double> NormalisedDistances { get; set; }

    public List<double> CalculateNormalisedDistances()
    {
        NormalisedDistances[0] = CheckDistance(-90) / raycastDistance;  // Left
        NormalisedDistances[1] = CheckDistance(-45) / raycastDistance;  // Left-Fwd
        NormalisedDistances[2] = CheckDistance(0) / raycastDistance;    // Fwd
        NormalisedDistances[3] = CheckDistance(45) / raycastDistance;   // Right-Fwd
        NormalisedDistances[4] = CheckDistance(90) / raycastDistance;   // Right
        return NormalisedDistances;
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
        NormalisedDistances = new List<double>()
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
        if (!DrawSensors) return;
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(-90) * raycastDistance, GetColour(NormalisedDistances[0]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(-45) * raycastDistance, GetColour(NormalisedDistances[1]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(0) * raycastDistance, GetColour(NormalisedDistances[2]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(45) * raycastDistance, GetColour(NormalisedDistances[3]));
        Debug.DrawRay(transform.position, CalculateDirectionFromAngle(90) * raycastDistance, GetColour(NormalisedDistances[4]));
    }

    private Color GetColour(double distance) => distance < raycastDistance ? Color.red : Color.green;
}
