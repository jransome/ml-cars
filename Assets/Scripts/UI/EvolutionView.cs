using System.Collections;
using UnityEngine;

public class EvolutionView : MonoBehaviour
{
    public CarBrain ViewSubject
    {
        get => viewSubject;
        private set
        {
            if (value == viewSubject || value == null) return;
            viewSubject = value;
            chaseCamera.ChaseTransform = viewSubject.transform;
            neuralNetworkView.DisplayedNeuralNetwork = viewSubject.NeuralNetwork;
        }
    }

    [SerializeField] float bestAgentPollIntervalSeconds = 3;
    [SerializeField] ChaseCamera chaseCamera;
    [SerializeField] SpeciesEvolver speciesEvolver;
    [SerializeField] CarControlsView carControlsView;
    [SerializeField] NetworkView neuralNetworkView;
    CarBrain viewSubject;

    private void Update()
    {
        if (ViewSubject == null) return;
        carControlsView.UpdateView(ViewSubject);
    }

    IEnumerator PollForMostSuccessful()
    {
        while (true) // StopAllCoroutines will still terminate this loop
        {
            ViewSubject = speciesEvolver.MostSuccessfulAlive;
            yield return new WaitForSeconds(bestAgentPollIntervalSeconds);
        }
    }

    void OnEnable() => StartCoroutine(PollForMostSuccessful());

    void OnDisable() => StopAllCoroutines();
}
