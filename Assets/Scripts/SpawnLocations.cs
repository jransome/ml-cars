using System.Collections.Generic;
using UnityEngine;

public class SpawnLocations : MonoBehaviour
{
    public List<Transform> Locations = new List<Transform>();

    public int GetIndex(Transform current) => Locations.IndexOf(current);
    public Transform GetNext(Transform current) => Locations[(Locations.IndexOf(current) + 1) % Locations.Count];
    public Transform GetRandom() => Locations[Random.Range(0, Locations.Count)];
}
