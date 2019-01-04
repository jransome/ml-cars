using System.Collections.Generic;
using UnityEngine;

public class PhysicsSensors : MonoBehaviour
{
    [SerializeField] private Rigidbody rb = null;

    public List<double> GetVelocityVectors() 
    {
        Vector3 localAxisVector = transform.InverseTransformDirection(rb.velocity);
        return new List<double>(2) { localAxisVector.x, localAxisVector.z };
    }

    public List<double> GetAngularVelocityVector() => new List<double>(1) { rb.angularVelocity.y };
}
