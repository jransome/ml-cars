using UnityEngine;
using UnityEngine.UI;

public class CarControlsView : MonoBehaviour
{
    public SpeciesEvolver EvolutionManager;
    public Slider SteeringIndicator;
    public Slider ThrottleIndicator;
    public Slider BrakeIndicator;

    public void UpdateView(CarBrain car)
    {
        SteeringIndicator.value = car.SteeringDecision;
        ThrottleIndicator.value = car.ThrottleDecision;
        BrakeIndicator.value = car.BrakingDecision;
    }
}
