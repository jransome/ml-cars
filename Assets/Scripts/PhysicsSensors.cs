using System.Collections.Generic;
using UnityEngine;

public class PhysicsSensors : MonoBehaviour
{
    [SerializeField] private Rigidbody rb = null;

    public List<double> GetNormalisedVelocityVectors(float maxX, float maxZ) 
    {
        Vector3 localAxisVector = transform.InverseTransformDirection(rb.velocity);
        return new List<double>(2) { localAxisVector.x / maxX, localAxisVector.z / maxZ };
    }

    public List<double> GetAngularVelocityVector() => new List<double>(1) { rb.angularVelocity.y };
}
