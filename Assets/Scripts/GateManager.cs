using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GateManager : MonoBehaviour
{
    public static GateManager Instance; 
    
    private Gate[] Gates;

    public Gate StartingGate { get; private set; }

    private void Awake()
    {
        Instance = this;
        Gates = GetComponentsInChildren<Gate>().OrderBy(g => g.Number).ToArray();
        for (int i = 0; i < Gates.Length; i++)
        {
            Gate nextGate = i == Gates.Length - 1 ? Gates[0] : Gates[i + 1];
            Gates[i].DirectionToNext = nextGate.transform.position - Gates[i].transform.position;
            Gates[i].DistanceToNext = Vector3.Distance(nextGate.transform.position, Gates[i].transform.position);
        }
        StartingGate = Gates[0];
    }   
}
