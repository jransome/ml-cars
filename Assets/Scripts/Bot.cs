using UnityEngine;

public class Bot : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 3f;
    private Rigidbody rb;

    public void Throttle (float input)
    {
        Vector3 positionDelta = transform.forward * input * speed * Time.deltaTime;
        rb.MovePosition(transform.position + positionDelta);
    }

    public void Steer(float input)
    {
        Quaternion rotationDelta = Quaternion.Euler(Vector3.up * input * turnSpeed);
        rb.MoveRotation(transform.rotation * rotationDelta);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //private void Update()
    //{
    //    Throttle(Input.GetAxis("Vertical"));
    //    Steer(Input.GetAxis("Horizontal"));
    //}
}
