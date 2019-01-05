using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public God EvolutionManager;
    private Transform chaseTransform;

    private void Update() 
    {
        IOrderedEnumerable<Brain> livingAgents = EvolutionManager.GenerationPool
            .Where(b => b.IsAlive)
            .OrderByDescending(b => b.ChaseCameraOrderingVariable);

        if (livingAgents.Count() > 0) chaseTransform = livingAgents.First().transform;

        if (chaseTransform != null) transform.position = chaseTransform.position;
    }
}
