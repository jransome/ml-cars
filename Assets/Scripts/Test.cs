using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Gate g;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(g.DirectionToNext);
        Debug.DrawRay(g.transform.position, g.DirectionToNext * 10, Color.blue, 5000);
        transform.forward = g.DirectionToNext;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(g.CalculateDistanceTo(transform.position));
    }
}
