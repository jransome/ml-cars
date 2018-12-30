using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
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

    [SerializeField] private GameObject populationPrefab = null;
    [SerializeField] private int generationSize = 10;
    [SerializeField] private List<Brain> generationPool;
    [SerializeField] private float generationTotalFitness;

    public int GenerationCount { get; set; }
    public int CurrentlyAlive { get; set; }

    private Dna SelectDna(Dictionary<Brain, float> selectionProbabilities, Dna exceptThisOne = null)
    {
        float random = Random.Range(0f, 1f);
        foreach(KeyValuePair<Brain, float> entry in selectionProbabilities)
        {
            random -= entry.Value;
            if(random < 0 && entry.Key.Dna != exceptThisOne) return entry.Key.SelectForBreeding();
        }
        Debug.Log("dfhjgd");
        return selectionProbabilities.Keys.ToArray()[Random.Range(0, selectionProbabilities.Count)].SelectForBreeding();
    }

    private IEnumerator CreateGeneration(bool seed = false)
    {
        // create TNG
        List<Dna> theNextGeneration = new List<Dna>();

        if (seed)
        {
            Debug.Log("Generation 1 seeded");
            for (int i = 0; i < generationSize; i++)
                theNextGeneration.Add(new Dna(Inputs, Outputs, HiddenLayers, MaxNeuronsPerLayer));
        }
        else
        {
            // report on last generation
            Debug.Log("Generation " + GenerationCount + ": Avg: " + generationPool.Average(b => b.Fitness) + " range: " + generationPool.Min(b => b.Fitness) + " - " + generationPool.Max(b => b.Fitness));
            
            // preserve top survivors 
            int nUnchanged = Mathf.RoundToInt(generationSize * ProportionUnchanged);
            if (nUnchanged > 0) theNextGeneration.AddRange(generationPool.OrderByDescending(b => b.Fitness).ToList().GetRange(0, nUnchanged).Select(b => b.Dna.Clone()));

            // TODO: mutate without breeding

            // add completely new dna into next gen
            int nNew = Mathf.RoundToInt(generationSize * NewDnaRate);
            if (nNew > 0) for (int i = 0; i < nNew; i++) theNextGeneration.Add(new Dna(Inputs, Outputs, HiddenLayers, MaxNeuronsPerLayer));

            // calculate probability of selection for each genome
            Dictionary<Brain, float> SelectionProbabilities = new Dictionary<Brain, float>();
            foreach (Brain b in generationPool)
                SelectionProbabilities.Add(b, b.Fitness / generationTotalFitness);

            // populate rest of next generation with offspring
            for (int i = nUnchanged; i < generationSize; i++)
            {
                // pick 2 random DIFFERENT parents and breed
                Dna p1 = SelectDna(SelectionProbabilities);
                Dna p2 = SelectDna(SelectionProbabilities, p1);
                // Dna offspring = p1.Clone().Splice(p2);
                Dna unmutoffspring = p1.Clone().Splice(p2);
                
                bool parSame = p1.IsEqual(p2);
                bool offSameP1 = p1.IsEqual(unmutoffspring);
                bool offSameP2 = p2.IsEqual(unmutoffspring);

                // do mutation
                Dna offspring = unmutoffspring.Clone();
                offspring.Mutate(1f);
                bool mutantSame = unmutoffspring.IsEqual(offspring);
                // if (Random.Range(0f, 1f) < MutationRate) offspring.Mutate(0.7f); 
                Debug.Log("parents same: " + parSame + " child == 1: " + (offSameP1 || offSameP2) + " child == both: " + (offSameP1 && offSameP2) + " mutation worked " + !mutantSame);
                
                theNextGeneration.Add(offspring);
                yield return new WaitForSeconds(0.2f); // so we can visualise selection of agents for the next generation
            }

            yield return new WaitForSeconds(3f); // pause so we can see the results of selection
        }

        // setup next generation
        CheckForIdenticalGenomes(theNextGeneration);
        GenerationCount++;
        generationTotalFitness = 0;
        CurrentlyAlive = generationSize;
        generationPool = generationPool.Zip(theNextGeneration, (brain, dna) => {
            brain.ReplaceDna(dna);
            brain.Arise(transform.position, transform.rotation);
            return brain;
        }).ToList();
    }

    private void CheckForIdenticalGenomes(List<Dna> dnaList)
    {
        List<Dna> duplicates = dnaList.Where(d => {
            foreach (var otherD in dnaList)
                if (d != otherD && d.IsEqual(otherD)) return true;

            return false;
        }).ToList();
        if (duplicates.Count > 0) Debug.LogError(duplicates.Count + " identical genomes in generation");
    }

    private void HandleIndividualDied(Brain deceased)
    {
        generationTotalFitness += deceased.Fitness;
        CurrentlyAlive--;
        if (CurrentlyAlive == 0) StartCoroutine(CreateGeneration());
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
            b.Died += HandleIndividualDied;
        }
        StartCoroutine(CreateGeneration(true));
    }
}
