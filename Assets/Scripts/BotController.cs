using UnityEngine;

public class BotController : AgentController
{
    public bool IsHumanControlled = false;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float turnSpeed = 3f;

    private Rigidbody rb;

    public override void Throttle(float input)
    {
        input = (input - 1) * 2;
        Vector3 positionDelta = transform.forward * input * maxSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + positionDelta);
    }

    public override void Steer(float input)
    {
        Quaternion rotationDelta = Quaternion.Euler(Vector3.up * input * turnSpeed);
        rb.MoveRotation(transform.rotation * rotationDelta);
    }

    public override void Brake(float input) // reverse in this case
    {
        // Vector3 positionDelta = transform.forward * -input * (maxSpeed * 0.2f) * Time.fixedDeltaTime;
        // rb.MovePosition(transform.position + positionDelta);
    }

    public override void ResetToPosition(Vector3 startPosition, Quaternion startRotation)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsHumanControlled) return;
        Throttle(Input.GetAxis("Vertical"));
        Steer(Input.GetAxis("Horizontal"));
    }
}