using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistanceGateManager : MonoBehaviour
{
    public static DistanceGateManager Instance; 
    
    public float TrackDistance = 0;
    [SerializeField] private DistanceGate[] Gates;

    public DistanceGate StartingGate { get; private set; }

    private void Awake()
    {
        Instance = this;
        Gates = GetComponentsInChildren<DistanceGate>().OrderBy(g => g.GetNumber()).ToArray();
        for (int i = 0; i < Gates.Length; i++)
        {
            DistanceGate nextGate = i == Gates.Length - 1 ? Gates[0] : Gates[i + 1];
            DistanceGate previousGate = i == 0 ? Gates.Last() : Gates[i - 1];

            Gates[i].DirectionToNext = nextGate.transform.position - Gates[i].transform.position;
            if (i > 0) TrackDistance += Vector3.Distance(Gates[i].transform.position, previousGate.transform.position);
            Gates[i].CumulativeDistance = TrackDistance;
        }
        StartingGate = Gates[0];
    }
}
