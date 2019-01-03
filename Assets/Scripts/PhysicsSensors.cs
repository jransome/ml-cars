using System.Collections.Generic;
using UnityEngine;

public class PhysicsSensors : MonoBehaviour
{
    [SerializeField] private Rigidbody rb = null;

    public List<double> GetVelocityVectors() => Vector3ToDoubleList(rb.velocity);

    public List<double> GetAngularVelocityVectors() => Vector3ToDoubleList(rb.angularVelocity);

    private List<double> Vector3ToDoubleList(Vector3 input) => new List<double>(3) { input.x, input.y, input.z };
}
