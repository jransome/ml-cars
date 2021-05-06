using System.Collections.Generic;
using UnityEngine;

public class NeuronView : MonoBehaviour
{
    public List<ConnectionView> ConnectionViews { get; private set; } = new List<ConnectionView>();
    public Transform WorldSpaceTransform;

    public void UpdateInputDisplay(List<double> weightedInputs)
    {
        if (weightedInputs.Count != ConnectionViews.Count)
            Debug.LogError("weightedInputs.Count != ConnectionViews.Count");

        for (int i = 0; i < weightedInputs.Count; i++)
            ConnectionViews[i].UpdateView((float)weightedInputs[i]);
    }
}
