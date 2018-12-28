using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
    [Header("Lineage colours")]
    public static Dictionary<DnaOrigin, Color> LineageColours;
    public Color NewGenome;
    public Color UnchangedFromLastGen;
    public Color Bred;
    public Color Mutated;
    public Color Eliminated;

    [Header("Algorithm variables")]
    public float SurvivorProportion = 0.5f;
    public int TopSurvivorsToKeepUnchanged = 1;
    public float NewDnaRate = 0.05f;
    public float MutationRate = 0.05f;

    [SerializeField] private GameObject populationPrefab = null;
    [SerializeField] private int generationSize = 10;
    [SerializeField] private List<Brain> generationPool;

    public int GenerationCount { get; set; }
    public int CurrentlyAlive { get; set; }

    private IEnumerator CreateGeneration()
    {
        GenerationCount++;
        int nSurvivors = (int)(generationSize * SurvivorProportion);
        Debug.Log("Generation " + GenerationCount + ": Avg: " + generationPool.Average(b => b.Fitness) + " range: " + generationPool.Min(b => b.Fitness) + " - " + generationPool.Max(b => b.Fitness));

        if (GenerationCount != 1)
        {
            generationPool = generationPool.OrderByDescending(b => b.Fitness).ToList();

            // eliminate the laggards and replace them with clones of the survivors
            // TODO: keep randomUnfitSurvivors?
            for (int i = nSurvivors; i < generationSize; i++)
            {
                generationPool[i].ReplaceDna(generationPool[Random.Range(0, generationSize / 3)].Dna.Clone());
                generationPool[i].GetComponent<Renderer>().material.color = Eliminated;
            }

            // do breeding/mutation for all but a number specified at the very top
            for (int i = 0; i < generationSize; i++)
            {
                Brain brain = generationPool[i];
                if (i < TopSurvivorsToKeepUnchanged)
                    brain.Dna.Origin = DnaOrigin.UnchangedFromLastGen;
                else
                {
                    float chance = Random.Range(0f, 1f);
                    if (chance < NewDnaRate)
                    {
                        // replace completely
                        brain.ReplaceDna(new Dna(5, 2, 1, 5));
                    }
                    else if (chance > (1 - MutationRate))
                    {
                        // mutate
                        brain.Dna.Mutate(Random.Range(0f,1f));
                    }
                    else
                    {
                        // breed with a random top others
                        brain.Dna.Splice(generationPool[Random.Range(0, nSurvivors)].Dna);
                    }
                }
            }
            yield return new WaitForSeconds(3f); // pause so we can see the eliminated agents change colour
        }

        CurrentlyAlive = generationSize;
        foreach (Brain b in generationPool)
            b.Arise(transform.position, transform.rotation);
    }

    private void IndividualDiedCallBack(Brain deceased)
    {
        CalculateFitness(deceased);
        CurrentlyAlive--;
        if (CurrentlyAlive == 0) StartCoroutine(CreateGeneration());
    }

    private void CalculateFitness(Brain brain)
    {
        brain.Fitness = brain.DistanceCovered;
    }

    private void Start()
    {
        LineageColours = new Dictionary<DnaOrigin, Color> ()
        {
            { DnaOrigin.IsNew, NewGenome },
            { DnaOrigin.UnchangedFromLastGen, UnchangedFromLastGen },
            { DnaOrigin.Mutated, Mutated },
            { DnaOrigin.Bred, Bred },
        };

        generationPool = new List<Brain>();
        for (int i = 0; i < generationSize; i++)
        {
            Brain b = Instantiate(populationPrefab).GetComponent<Brain>();
            b.ReplaceDna(new Dna(5, 2, 1, 5));
            generationPool.Add(b);
            b.Died += IndividualDiedCallBack;
        }

        StartCoroutine(CreateGeneration());
    }
}
