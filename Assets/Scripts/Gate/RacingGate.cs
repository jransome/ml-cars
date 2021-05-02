using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingGate : MonoBehaviour
{
    [SerializeField] private Transform optimalVectorPosition = null;

    public Vector3 OptimalDirection { get { return optimalVectorPosition.forward; } }
    public Vector3 OptimalPosition { get { return optimalVectorPosition.position; } }

    // used for setting up gates
    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(optimalVectorPosition.position - optimalVectorPosition.forward * 5, optimalVectorPosition.forward * 10); 
    }
}
