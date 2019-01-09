using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OutputsView : MonoBehaviour
{
    public God EvolutionManager;
    public Slider SteeringIndicator;
    public Slider ThrottleIndicator;
    public Slider BrakeIndicator;

    private CarBrain carBrain;

    private void Update() 
    {
        IOrderedEnumerable<Brain> livingAgents = EvolutionManager.GenerationPool
            .Where(b => b.IsAlive)
            .OrderByDescending(b => b.ChaseCameraOrderingVariable);

        if (livingAgents.Count() > 0) carBrain = (CarBrain)livingAgents.First();
        else return;

        SteeringIndicator.value = carBrain.SteeringDecision;        
        ThrottleIndicator.value = carBrain.ThrottleDecision;
        BrakeIndicator.value = Mathf.Clamp01(carBrain.BrakingDecision);
    }
}
