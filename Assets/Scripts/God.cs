using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
    [SerializeField] private GameObject populationPrefab;
    [SerializeField] private int generationSize = 10;
    private List<Brain> generationPool;

    public int GenerationCount { get; set; }
    public int CurrentlyAlive { get; set; }

    private void CreateGeneration()
    {
        generationPool.OrderByDescending(b => b.Fitness);

        // do breeding/mutation

        CurrentlyAlive = generationSize;
        foreach (Brain b in generationPool)
            b.Arise(transform.position, transform.rotation);
    }

    private void IndividualDiedCallBack(Brain deceased)
    {
        CalculateFitness(deceased);
        CurrentlyAlive--;
        if (CurrentlyAlive == 0) CreateGeneration();
    }

    private void CalculateFitness(Brain brain)
    {
        brain.Fitness = brain.LifeSpan == 0 ? 0 : brain.GatesCrossed / brain.LifeSpan;
    }

    private void Start()
    {
        generationPool = new List<Brain>();
        for (int i = 0; i < generationSize; i++)
        {
            Brain b = Instantiate(populationPrefab).GetComponent<Brain>();
            generationPool.Add(b);
            b.Died += IndividualDiedCallBack;
        }
        CreateGeneration();
    }
}
