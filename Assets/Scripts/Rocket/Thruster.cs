using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Thruster : MonoBehaviour
{
    [SerializeField] float maxConeAngle = 20f;
    [SerializeField] float maxThrust = 20f;
    [SerializeField] Transform particleFxTransform;
    [SerializeField] ParticleSystem particleFx;
    [SerializeField] VisualEffect vfx;

    public void Fire(Rigidbody shipRigidbody, float xAttitude, float zAttitude, float magnitude)
    {
        // x and z attitude should be clamped between -1 and 1
        Quaternion nozzleRotation = transform.rotation * Quaternion.Euler(xAttitude * maxConeAngle, 0, zAttitude * maxConeAngle);
        particleFxTransform.rotation = nozzleRotation;

        Vector3 force = nozzleRotation * Vector3.up * maxThrust * magnitude;
        shipRigidbody.AddForceAtPosition(force, transform.position);
        // Debug.DrawRay(transform.position, -force, Color.red);
        StartCoroutine(Vfx());
    }

    IEnumerator Vfx()
    {
        vfx.Play();
        particleFx.Play();
        yield return new WaitForSeconds(0.5f);
        particleFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        vfx.Stop();
    }

    void Start()
    {
        // vfx.Stop();
        vfx.pause = true;
    }
}
