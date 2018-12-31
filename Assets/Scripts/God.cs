using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
    List<Transform> ts;

    [Header("Lineage colours")]
    public static Dictionary<DnaHeritage, Color> LineageColours;
    public Color NewGenome;
    public Color UnchangedFromLastGen;
    public Color Bred;
    public Color Mutated;

    [Header("Neural network variables")]
    public int Inputs = 5;
    public int Outputs = 2;
    public int HiddenLayers = 1;
    public int MaxNeuronsPerLayer = 5;

    [Header("Algorithm variables")]
    public float ProportionUnchanged = 0.05f;
    public float NewDnaRate = 0.05f;
    public float MutationRate = 0.05f;
    public bool MutateClones = true;
    public float MaxTimeToReachNextGate = 5f;

    [Header("Population")]
    [SerializeField] private GameObject populationPrefab = null;
    [SerializeField] private int generationSize = 10;
    [SerializeField] private List<Brain> generationPool;
    [SerializeField] private List<Dna> genePool;
    [SerializeField] private float generationTotalFitness; // for debugging

    public int GenerationCount { get; set; }
    public int CurrentlyAlive { get; set; }

    private Dna[] CreateOffspring(List<Dna> parentPool)
    {
        Dna parent1 = SelectRandomBasedOnFitness(parentPool);
        Dna parent2 = SelectRandomBasedOnFitness(parentPool.Where(p => p != parent1).ToList());
        return parent1.Splice(parent2);
    }

    private Dna SelectRandomBasedOnFitness(List<Dna> parentPool)
    {
        float random = Random.Range(0f, parentPool.Sum(p => p.Fitness));
        foreach(Dna candidate in parentPool.OrderBy(c => Random.Range(0f, 1f)))
        {
            random -= candidate.Fitness;
            if (random < 0) return candidate;
        }
        return parentPool.Last();
    }

    private void ReleaseNewGeneration(List<Dna> newGenerationDna)
    {
        genePool = newGenerationDna;
        generationTotalFitness = 0; // used for debugging only
        CurrentlyAlive = generationSize;
        generationPool = generationPool.Zip(newGenerationDna, (brain, dna) => {
            brain.ReplaceDna(dna);
            brain.Arise(transform.position, transform.rotation);
            return brain;
        }).ToList();
    }

    private IEnumerator CreateGeneration(List<Dna> oldGeneration = null)
    {
        // create TNG
        List<Dna> theNextGeneration = new List<Dna>();

        if (oldGeneration == null)
        {
            for (int i = 0; i < generationSize; i++)
                theNextGeneration.Add(new Dna(Inputs, Outputs, HiddenLayers, MaxNeuronsPerLayer));
            Debug.Log("Generation 1 seeded");
        }
        else
        {
            // report on last generation
            Debug.Log("Generation " + GenerationCount + ": Avg: " + oldGeneration.Average(d => d.Fitness) + " Max: " + oldGeneration.Max(d => d.Fitness));
            
            // preserve top survivors 
            int nUnchanged = Mathf.RoundToInt(generationSize * ProportionUnchanged);
            if (nUnchanged > 0) theNextGeneration.AddRange(oldGeneration.OrderByDescending(d => d.Fitness).ToList().GetRange(0, nUnchanged).Select(d => d.Clone()));

            // add completely new dna into next gen
            int nNew = Mathf.RoundToInt(generationSize * NewDnaRate);
            if ((generationSize - nNew) % 2 == 1) nNew ++; // make sure remaining spaces is an even number
            if (nNew > 0) for (int i = 0; i < nNew; i++) theNextGeneration.Add(new Dna(Inputs, Outputs, HiddenLayers, MaxNeuronsPerLayer));

            // populate rest of next generation with offspring
            for (int i = nUnchanged + nNew; i < generationSize; i += 2)
            {
                // pick 2 random DIFFERENT parents and breed
                foreach (Dna child in CreateOffspring(oldGeneration))
                {
                    // do mutation
                    if (Random.Range(0f, 1f) < MutationRate) child.Mutate(0.5f);
                    theNextGeneration.Add(child);   
                }
                yield return new WaitForSeconds(0.1f); // so we can visualise selection of agents for the next generation
            }
            yield return new WaitForSeconds(0.5f); // pause so we can see the results of selection
        }

        // check for clones
        CheckForIdenticalGenomes(theNextGeneration);
        if (MutateClones) MutateIdenticalGenomes(theNextGeneration);
        
        // setup next generation
        GenerationCount++;
        ReleaseNewGeneration(theNextGeneration);
    }

    private void MutateIdenticalGenomes(List<Dna> dnaList)
    {
        Debug.Log("Mutating clones...");
        foreach (var d in dnaList)
        {
            foreach (var otherD in dnaList)
                if (d != otherD && d.IsEqual(otherD)) d.Mutate(0.5f);
        }
    }

    private void CheckForIdenticalGenomes(List<Dna> dnaList)
    {
        List<Dna> duplicates = dnaList.Where(d => {
            foreach (var otherD in dnaList)
                if (d != otherD && d.IsEqual(otherD)) return true;

            return false;
        }).ToList();

        if (duplicates.Count > 0) Debug.Log(duplicates.Count + " identical genomes found");
    }

    private void HandleIndividualDied(Brain deceased, float fitness)
    {
        generationTotalFitness += fitness; // used for debugging only
        --CurrentlyAlive;
        if (CurrentlyAlive == 0) StartCoroutine(CreateGeneration(genePool));
    }

    private void Start()
    {
        LineageColours = new Dictionary<DnaHeritage, Color> ()
        {
            { DnaHeritage.IsNew, NewGenome },
            { DnaHeritage.UnchangedFromLastGen, UnchangedFromLastGen },
            { DnaHeritage.Mutated, Mutated },
            { DnaHeritage.Bred, Bred },
        };

        generationPool = new List<Brain>();
        for (int i = 0; i < generationSize; i++)
        {
            Brain b = Instantiate(populationPrefab).GetComponent<Brain>();
            generationPool.Add(b);
            b.SuicideThreshold = MaxTimeToReachNextGate;
            b.Died += HandleIndividualDied;
        }

        ts = generationPool.Select(b => b.transform).ToList();

        StartCoroutine(CreateGeneration());
    }
}
