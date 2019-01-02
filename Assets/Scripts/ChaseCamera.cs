using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform FollowTransform { get; set; }

    private void Update() 
    {
        if (FollowTransform != null) transform.position = FollowTransform.position;
    }
}
