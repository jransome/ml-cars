using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float force = 10f;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.left * force, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
