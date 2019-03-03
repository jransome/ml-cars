using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistanceSensors : MonoBehaviour
{
    public bool DrawSensors = false;
    public LayerMask IgnoredLayers;
    public float[] SensorSpreadAngles;
    private List<float> realSensorSpreadAngles;
    [SerializeField] private float raycastDistance = 15f;

    public List<double> CalculateNormalisedDistances() => realSensorSpreadAngles.Select(angle => CheckDistance(angle) / raycastDistance).ToList();

    private double CheckDistance(float angle)
    {
        Vector3 direction = CalculateDirectionFromAngle(angle);
        RaycastHit hit;
        return Physics.Raycast(transform.position, direction, out hit, raycastDistance, ~IgnoredLayers) ? hit.distance : raycastDistance;
    }

    private Vector3 CalculateDirectionFromAngle(float angle) => Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(transform.forward.x, 0f, transform.forward.z);

    private void Awake()
    {
        realSensorSpreadAngles = SensorSpreadAngles.Aggregate(new List<float>(), (acc, angle) => {
            acc.Add(angle);
            if (angle != 0) acc.Add(-angle);
            return acc;
        });
    }

    private void Update()
    {
        if (!DrawSensors) return;
        realSensorSpreadAngles.ForEach(a => {
            Debug.DrawRay(transform.position, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * 0.01f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * -0.01f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * 0.02f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * -0.02f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * 0.03f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * -0.03f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * 0.04f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
            Debug.DrawRay(transform.position + transform.right * -0.04f, CalculateDirectionFromAngle(a) * raycastDistance, CheckDistance(a) < raycastDistance ? Color.red : Color.green);
        });
    }
}
