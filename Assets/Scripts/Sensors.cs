using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensors : MonoBehaviour
{
    [SerializeField] public bool drawRaycasts = false;
    [SerializeField] private float raycastDistance = 30f;
    [SerializeField] private float raycastInterval = 0.1f;

    public bool IsSensing { get; set; } = true;
    public List<double> Distances { get; set; }

    private void Start()
    {
        Distances = new List<double>()
        {
            raycastDistance,    // Left
            raycastDistance,    // Left-Fwd
            raycastDistance,    // Fwd
            raycastDistance,    // Right-Fwd
            raycastDistance,    // Right
        };
        StartCoroutine(CastRays());
    }

    private IEnumerator CastRays()
    {
        while (IsSensing)
        {
            Distances[0] = CheckDistance(-90);  // Left
            Distances[1] = CheckDistance(-45);  // Left-Fwd
            Distances[2] = CheckDistance(0);    // Fwd
            Distances[3] = CheckDistance(45);   // Right-Fwd
            Distances[4] = CheckDistance(90);   // Right
            yield return new WaitForSeconds(raycastInterval);
        }
    }

    private float CheckDistance(float angle)
    {
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        if (drawRaycasts) Debug.DrawRay(transform.position, direction * raycastDistance, Color.blue, raycastInterval);
        RaycastHit hit;
        return Physics.Raycast(transform.position, direction, out hit, raycastDistance) ? hit.distance : raycastDistance;
    }
}
