using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Range(0, 500)] public float thrust = 10f;
    [Range(-1, 1)] public float xA = 0f;
    [Range(-1, 1)] public float zA = 0f;

    [SerializeField] Rigidbody rb;
    [SerializeField] Thruster mainEngine;
    [SerializeField] Thruster[] RcsThrusters;

    public void FireMainEngine(float xAttitude, float zAttitude, float magnitude) => mainEngine.Fire(rb, xAttitude, zAttitude, thrust);
    public void PitchUpRcs(float magnitude) => FireRcsThruster(0, magnitude);
    public void PitchDownRcs(float magnitude) => FireRcsThruster(1, magnitude);
    public void YawRightRcs(float magnitude) => FireRcsThruster(2, magnitude);
    public void YawLeftRcs(float magnitude) => FireRcsThruster(3, magnitude);

    private void FireRcsThruster(int index, float magnitude)
    {
        RcsThrusters[index].Fire(rb, 0, 0, magnitude);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) FireMainEngine(xA, zA, thrust);

        if (Input.GetKey(KeyCode.W)) PitchDownRcs(thrust);
        if (Input.GetKey(KeyCode.S)) PitchUpRcs(thrust);
        if (Input.GetKey(KeyCode.A)) YawLeftRcs(thrust);
        if (Input.GetKey(KeyCode.D)) YawRightRcs(thrust);
    }
}
