using UnityEngine;

public class CarController : AgentController
{
    public bool IsHumanControlled = false;
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float torque = 800f;
    [SerializeField] private float maxLockDegrees = 35f;
    [SerializeField] private WheelCollider[] wheels = null;
    [SerializeField] private Transform[] frontWheelModels = null;

    public override void Throttle(float input)
    {
        for (int i = 0; i < 2; i++) // front wheels
            wheels[i].motorTorque = input * torque;
        
        foreach (WheelCollider w in wheels)
            w.brakeTorque = 0; // release brakes on rear wheels
    }

    public override void Steer(float input)
    {
        float angle = input * maxLockDegrees;
        for (int i = 0; i < 2; i++) // front wheels
        {
            wheels[i].steerAngle = angle;
            frontWheelModels[i].localRotation = Quaternion.AngleAxis(angle, Vector3.up);
        }
    }

    public override void Brake(float input)
    {
        input = Mathf.Clamp01(input);
        foreach (WheelCollider w in wheels)
            w.brakeTorque = input * torque;
    }

    public override void ResetToPosition(Vector3 startPosition, Quaternion startRotation)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
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
