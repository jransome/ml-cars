using System.Collections.Generic;
using UnityEngine;

public class SpawnLocations : MonoBehaviour
{
    public List<Transform> Points = new List<Transform>();

    public Transform GetNext(Transform current) => Points[(Points.IndexOf(current) + 1) % Points.Count];
    public Transform GetRandom() => Points[Random.Range(0, Points.Count)];
}
