using UnityEngine;
using UnityEngine.UI;

public class OutputsView : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public Slider SteeringIndicator;
    public Slider ThrottleIndicator;
    public Slider BrakeIndicator;

    private CarBrain carToTrack = null;

    private void Start()
    {
        MostSuccessfulPoller.OnMostSuccessfulAliveChanged += (newCarBrain) =>
        {
            carToTrack = newCarBrain;
        };
    }

    private void Update()
    {
        if (carToTrack == null) return;
        SteeringIndicator.value = carToTrack.SteeringDecision;
        ThrottleIndicator.value = carToTrack.ThrottleDecision;
        BrakeIndicator.value = carToTrack.BrakingDecision;
    }
}
