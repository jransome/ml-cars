using UnityEngine;

public class Car : MonoBehaviour
{
    public bool IsHumanControlled = false;
    [SerializeField] private float torque = 800f;
    [SerializeField] private float maxLockDegrees = 35f;
    [SerializeField] private WheelCollider[] frontWheels;

    public void Throttle(float input)
    {
        foreach (WheelCollider w in frontWheels)
            w.motorTorque = input * torque;
    }

    public void Steer(float input)
    {
        foreach (WheelCollider w in frontWheels)
            w.steerAngle = input * maxLockDegrees;
    }

    private void Update()
    {
        if (!IsHumanControlled) return;
        Throttle(Input.GetAxis("Vertical"));
        Steer(Input.GetAxis("Horizontal"));
    }
}
