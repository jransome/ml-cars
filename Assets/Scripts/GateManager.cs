using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    public static GateManager Instance; 
    
    public float TrackDistance = 0;
    [SerializeField] private Gate[] Gates;

    public Gate StartingGate { get; private set; }

    private void Awake()
    {
        Instance = this;
        Gates = GetComponentsInChildren<Gate>().OrderBy(g => g.GetNumber()).ToArray();
        for (int i = 0; i < Gates.Length; i++)
        {
            Gate nextGate = i == Gates.Length - 1 ? Gates[0] : Gates[i + 1];
            Gate previousGate = i == 0 ? Gates.Last() : Gates[i - 1];

            Gates[i].DirectionToNext = nextGate.transform.position - Gates[i].transform.position;
            if (i > 0) TrackDistance += Vector3.Distance(Gates[i].transform.position, previousGate.transform.position);
            Gates[i].CumulativeDistance = TrackDistance;
        }
        StartingGate = Gates[0];
    }
}
