using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkView : MonoBehaviour
{
    public NeuralNetwork DisplayedNeuralNetwork
    {
        get => displayedNeuralNetwork;
        set
        {
            if (value == displayedNeuralNetwork) return;
            if (displayedNeuralNetwork != null) displayedNeuralNetwork.OnLayerFeedForward -= UpdateWeightVisualisation;
            displayedNeuralNetwork = value;
            DrawNetwork(displayedNeuralNetwork);
            displayedNeuralNetwork.OnLayerFeedForward += UpdateWeightVisualisation;
        }
    }

    NeuralNetwork displayedNeuralNetwork;
    List<List<NeuronView>> displayedNeuronViews = new List<List<NeuronView>>();
    List<NeuronView> neuronViewsPool = new List<NeuronView>();
    List<ConnectionView> connectionViewsPool = new List<ConnectionView>();

    [SerializeField] GameObject neuronViewPrefab;
    [SerializeField] GameObject connectionViewPrefab;
    [SerializeField] Transform neuronsContainer;
    [SerializeField] Transform connectionsContainer;
    [SerializeField] Vector2 neuronSpacing = new Vector2(200, 400);

    private static T GetPooledView<T>(List<T> pool, GameObject prefab, Transform viewContainer) where T : MonoBehaviour
    {
        T view = pool.Find(v => !v.isActiveAndEnabled);
        if (view != null) return view;

        view = Instantiate(prefab, viewContainer).GetComponent<T>();
        pool.Add(view);
        return view;
    }

    private void DrawNetwork(NeuralNetwork neuralNetwork)
    {
        ClearViews();

        List<int> outputsPerLayer = neuralNetwork.Dna.OutputsPerLayer;
        displayedNeuronViews = new List<List<NeuronView>>(outputsPerLayer.Count);

        for (int layerIndex = 0; layerIndex < outputsPerLayer.Count; layerIndex++)
        {
            List<NeuronView> neuronViewsInCurrentLayer = new List<NeuronView>(outputsPerLayer[layerIndex]);
            displayedNeuronViews.Add(neuronViewsInCurrentLayer);

            for (int neuronIndex = 0; neuronIndex < outputsPerLayer[layerIndex]; neuronIndex++)
            {
                NeuronView nv = GetPooledNeuronView();
                nv.Init(new Vector2(layerIndex * neuronSpacing.x, -neuronIndex * (neuronSpacing.y / outputsPerLayer[layerIndex])));
                neuronViewsInCurrentLayer.Add(nv);
            }
        }

        for (int layerIndex = 1; layerIndex < outputsPerLayer.Count; layerIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < outputsPerLayer[layerIndex]; neuronIndex++)
            {
                for (int k = 0; k < outputsPerLayer[layerIndex - 1]; k++)
                {
                    ConnectionView cv = GetPooledConnectionView();
                    cv.Init(
                        displayedNeuronViews[layerIndex - 1][k].transform.localPosition,
                        displayedNeuronViews[layerIndex][neuronIndex].transform.localPosition
                    );
                    displayedNeuronViews[layerIndex][neuronIndex].ConnectionViews.Add(cv);
                }
            }
        }
    }

    private void UpdateWeightVisualisation(List<double> inputs, List<INeuron> layer, int layerIndex)
    {
        if (layer.Count != displayedNeuronViews[layerIndex].Count)
            Debug.LogError("Mismatching neural network topology/displayed view");

        for (int i = 0; i < layer.Count; i++)
        {
            displayedNeuronViews[layerIndex][i].UpdateInputDisplay(
                inputs.Zip(layer[i].Weights, (i, w) => i * w).ToList()
            );
        }
    }

    private void ClearViews()
    {
        displayedNeuronViews.Clear();
        foreach (Transform neuronView in neuronsContainer) neuronView.gameObject.SetActive(false);
        foreach (Transform connectionview in connectionsContainer) connectionview.gameObject.SetActive(false);
    }

    NeuronView GetPooledNeuronView() => GetPooledView<NeuronView>(neuronViewsPool, neuronViewPrefab, neuronsContainer);

    ConnectionView GetPooledConnectionView() => GetPooledView<ConnectionView>(connectionViewsPool, connectionViewPrefab, connectionsContainer);
}
