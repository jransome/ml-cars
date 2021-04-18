using UnityEngine;
using UnityEngine.UI;

public class OutputsView : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public Slider SteeringIndicator;
    public Slider ThrottleIndicator;
    public Slider BrakeIndicator;


    private void Update()
    {
        CarBrain carToTrack = (CarBrain)EvolutionManager.MostSuccessfulAlive;

        if (carToTrack) 
            UpdateCarOutputSliders(carToTrack);
    }

    private void UpdateCarOutputSliders(CarBrain carBrain)
    {
        SteeringIndicator.value = carBrain.SteeringDecision;
        ThrottleIndicator.value = carBrain.ThrottleDecision;
        BrakeIndicator.value = Mathf.Clamp01(carBrain.BrakingDecision);
    }
}
