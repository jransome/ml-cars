using UnityEngine;

public class Car : MonoBehaviour
{
    public bool IsHumanControlled = false;
    [SerializeField] private float torque = 800f;
    [SerializeField] private float maxLockDegrees = 35f;
    [SerializeField] private WheelCollider[] wheels = null;
    [SerializeField] private Transform[] frontWheelModels = null;

    public void Throttle(float input)
    {
        for (int i = 0; i < 2; i++) // front wheels
            wheels[i].motorTorque = input * torque;
    }

    public void Steer(float input)
    {
        float angle = input * maxLockDegrees;
        for (int i = 0; i < 2; i++) // front wheels
        {
            wheels[i].steerAngle = angle;
            frontWheelModels[i].localRotation = Quaternion.AngleAxis(angle, Vector3.up);
        }
    }

    public void Brake(float input)
    {
        input = Mathf.Clamp01(input);
        foreach (WheelCollider w in wheels)
            w.brakeTorque = input * torque;
    }

    private void Update()
    {
        if (!IsHumanControlled) return;
        Throttle(Input.GetAxis("Vertical"));
        Steer(Input.GetAxis("Horizontal"));
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        Brake(brakeInput);
    }
}
