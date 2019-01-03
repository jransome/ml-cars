using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderTrigger : MonoBehaviour
{
    public event Action<Collider> TriggerEntered = delegate { };

    protected void OnTriggerEnter(Collider other) => TriggerEntered(other);
}
