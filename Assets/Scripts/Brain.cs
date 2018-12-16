using UnityEngine;

public class Brain : MonoBehaviour
{
    private NeuralNetwork nn;

    private void OnEnable()
    {
        nn = new NeuralNetwork(2, 1, 2);
    }
}
