using System.Collections;
using System;
using UnityEngine;

public class MostSuccessfulPoller : MonoBehaviour
{
    public static event Action<CarBrain> OnMostSuccessfulAliveChanged = delegate { };

    public CarBrain CurrentMostSuccessful { get; private set; }

    [SerializeField] SpeciesEvolver speciesEvolver;

    IEnumerator PollForMostSuccessful()
    {
        while (true) // StopAllCoroutines will still terminate this loop
        {
            CarBrain mostSuccessful = speciesEvolver.MostSuccessfulAlive;

            if (mostSuccessful && CurrentMostSuccessful != mostSuccessful)
            {
                CurrentMostSuccessful = mostSuccessful;
                OnMostSuccessfulAliveChanged(CurrentMostSuccessful);
            }

            yield return new WaitForSeconds(1);
        }
    }

    void OnEnable() => StartCoroutine(PollForMostSuccessful());

    void OnDisable() => StopAllCoroutines();
}
