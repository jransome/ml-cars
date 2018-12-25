using System;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public int Number { get; set; }

    private void Start()
    {
        Number = Convert.ToInt32(gameObject.name.Split(new char[] { '(', ')' })[1]);
    }
}
