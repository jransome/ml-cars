﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingGate : MonoBehaviour
{
    [SerializeField] private Transform optimalVectorPosition;

    // used for setting up gates
    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(optimalVectorPosition.position - optimalVectorPosition.forward * 5, optimalVectorPosition.forward * 10); 
    }
}
