﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
    [SerializeField] private GameObject populationPrefab;
    [SerializeField] private int generationSize = 10;
    private List<Brain> generationPool;

    public int GenerationCount { get; set; }
    public int CurrentlyAlive; //{ get; set; }

    private void CreateGeneration()
    {
        GenerationCount++;
        Debug.Log("Generation " + GenerationCount + ": Avg: " + generationPool.Average(b => b.Fitness) + " range: " + generationPool.Min(b => b.Fitness) + " - " + generationPool.Max(b => b.Fitness));
        int nReplaced = 0;
        int nMutated = 0;
        int nBred = 0;

        if (GenerationCount != 1)
        {
            generationPool = generationPool.OrderByDescending(b => b.Fitness).ToList();
            // List<int> randomUnfitSurvivors = new List<int> {
            //     Random.Range(3, generationSize),
            //     Random.Range(3, generationSize),
            //     Random.Range(3, generationSize),
            //     Random.Range(3, generationSize),
            //     Random.Range(3, generationSize),
            // };

            // replace all but the top 3 AND 5 random others with clones of the top 3
            for (int i = generationSize / 2; i < generationSize; i++)
            {
                // if (!randomUnfitSurvivors.Contains(i))
                    generationPool[i].ReplaceDna(generationPool[Random.Range(0, generationSize / 3)].Dna.Clone());
            }

            // do breeding/mutation for all but top 3
            for (int i = 3; i < generationSize; i++)
            {
                var agent = generationPool[i];
                float chance = Random.Range(0f, 1f);

                if (chance < 0.1f)
                {
                    // 10% chance to replace completely
                    agent.ReplaceDna(new DNA(5, 2, 1, 5));
                    nReplaced++;
                }
                else if (chance > 0.70f)
                {
                    // 30% chance to mutate
                    agent.Dna.Mutate(Random.Range(0f,1f));
                    nMutated++;
                }
                else
                {
                    // 60% chance to breed with a random top other without being 
                    agent.Dna.Splice(generationPool[Random.Range(0, generationSize / 2)].Dna);
                    nBred++;
                }
            }

            Debug.Log("Replaced: " + nReplaced + " Mutated: " + nMutated + " Bred: " + nBred);
        }

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
        brain.Fitness = brain.DistanceCovered;
    }

    private void Start()
    {
        generationPool = new List<Brain>();
        for (int i = 0; i < generationSize; i++)
        {
            Brain b = Instantiate(populationPrefab).GetComponent<Brain>();
            b.ReplaceDna(new DNA(5, 2, 1, 5));
            generationPool.Add(b);
            b.Died += IndividualDiedCallBack;
        }

        CreateGeneration();
    }
}