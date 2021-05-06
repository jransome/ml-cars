using RansomeCorp.AI.NeuralNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkView : MonoBehaviour
{
    [SerializeField] GameObject layoutViewPrefab;
    [SerializeField] GameObject neuronViewPrefab;
    [SerializeField] GameObject connectionViewPrefab;
    [SerializeField] Transform neuronsContainer;
    [SerializeField] Transform connectionsContainer;

    [SerializeField] Vector2 neuronSpacing = new Vector2(80, -30);

    NeuralNetwork displayedNeuralNetwork;
    List<List<NeuronView>> displayedNeuronViews = new List<List<NeuronView>>(); // includes the input 'layer'

    // private List<NeuronView> neuronViewsPool = new List<NeuronView>();
    // private List<ConnectionView> connectionViewsPool = new List<ConnectionView>();

    private void Start()
    {
        MostSuccessfulPoller.OnMostSuccessfulAliveChanged += (carBrain) =>
        {
            displayedNeuronViews.Clear();
            foreach (Transform neuronView in neuronsContainer) Destroy(neuronView.gameObject);
            foreach (Transform connectionview in connectionsContainer) Destroy(connectionview.gameObject);

            if (displayedNeuralNetwork != null) displayedNeuralNetwork.OnLayerFeedForward -= UpdateWeightVisualisation;
            DrawNetwork(carBrain.NeuralNetwork);
            displayedNeuralNetwork = carBrain.NeuralNetwork;
            displayedNeuralNetwork.OnLayerFeedForward += UpdateWeightVisualisation;
        };
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

    // NeuronView GetPooledNeuronView()
    // {
    //     NeuronView nv = neuronViewsPool.Find(nv => !nv.isActiveAndEnabled);
    //     if (nv) return nv;

    //     nv = Instantiate(neuronViewPrefab, neuronsContainer).GetComponent<NeuronView>();
    //     nv.gameObject.SetActive(false);
    //     neuronViewsPool.Add(nv);
    //     return nv;
    // }

    // ConnectionView GetPooledConnectionView()
    // {
    //     ConnectionView cv = connectionViewsPool.Find(cv => !cv.isActiveAndEnabled);
    //     if (cv) return cv;

    //     cv = Instantiate(connectionViewPrefab, connectionsContainer).GetComponent<ConnectionView>();
    //     cv.gameObject.SetActive(false);
    //     connectionViewsPool.Add(cv);
    //     return cv;
    // }

    void DrawNetwork(NeuralNetwork neuralNetwork)
    {
        List<int> outputsPerLayer = neuralNetwork.Dna.OutputsPerLayer;
        displayedNeuronViews = new List<List<NeuronView>>(outputsPerLayer.Count);

        for (int layerIndex = 0; layerIndex < outputsPerLayer.Count; layerIndex++)
        {
            List<NeuronView> neuronViewsInCurrentLayer = new List<NeuronView>(outputsPerLayer[layerIndex]);
            displayedNeuronViews.Add(neuronViewsInCurrentLayer);

            Vector2 offset = new Vector2(0, 0);
            for (int neuronIndex = 0; neuronIndex < outputsPerLayer[layerIndex]; neuronIndex++)
            {
                var nv = Instantiate(neuronViewPrefab, neuronsContainer).GetComponent<NeuronView>();
                nv.transform.localPosition = new Vector2(layerIndex * neuronSpacing.x, -neuronIndex * (neuronSpacing.y / outputsPerLayer[layerIndex])) + offset;
                neuronViewsInCurrentLayer.Add(nv);
            }
        }

        for (int layerIndex = 1; layerIndex < outputsPerLayer.Count; layerIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < outputsPerLayer[layerIndex]; neuronIndex++)
            {
                for (int k = 0; k < outputsPerLayer[layerIndex - 1]; k++)
                {
                    ConnectionView cv = Instantiate(connectionViewPrefab, connectionsContainer).GetComponent<ConnectionView>();
                    cv.Init(
                        displayedNeuronViews[layerIndex - 1][k].transform.localPosition,
                        displayedNeuronViews[layerIndex][neuronIndex].transform.localPosition
                    );
                    displayedNeuronViews[layerIndex][neuronIndex].ConnectionViews.Add(cv);
                }
            }
        }
    }
}