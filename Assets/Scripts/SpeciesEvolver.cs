using RansomeCorp.AI.Evolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeciesEvolver : MonoBehaviour
{
    public CarSpecies Species;
    public SpawnLocations SpawnLocations;
    public int GenerationsPerSpawnLocation = 15;

    private Transform spawnLocation;

    public List<GenerationData> GenerationHistory { get; private set; } = new List<GenerationData>();
    public List<CarBrain> GenerationPool { get; private set; } = new List<CarBrain>();
    public List<CarBrain> CurrentlyAlive { get; private set; } = new List<CarBrain>();
    public List<CarBrain> SelectedForBreeding { get; private set; } = new List<CarBrain>(); // used purely for visual feedback

    public CarBrain MostSuccessfulAlive
    {
        get => GenerationPool.Where(b => b.IsAlive).OrderByDescending(b => b.Fitness).DefaultIfEmpty(null).First();
    }

    public void LoadGeneration(SaveData saveData)
    {
        GenerationHistory = saveData.GenerationHistory;
        ReleaseGeneration(GenerationHistory.Last().GenePool, GenerationPool); // TODO: depends on phenotypes already having been instanced
    }

    public static List<Dna> CreateGenerationDna(CarSpecies species, List<GenerationData> history = null) // TODO: move to darwin class
    {
        if (history == null)
        {
            Debug.Log("Creating randomly seeded new " + species.name + " generation of " + species.GenerationSize + " agents.");
            return Enumerable.Range(0, species.GenerationSize).Select((_) =>
                Dna.GenerateRandomDnaEncoding(
                    species.Inputs,
                    species.HiddenLayersNeuronCount,
                    CarSpecies.Outputs,
                    species.OutputLayerActivation,
                    species.HeterogeneousHiddenActivation
                )
            ).ToList();
        }

        List<Dna> previousGeneration = history.Last().GenePool;
        List<Dna> TNG = new List<Dna>();

        // Add fresh dna into next gen
        int nNew = Mathf.RoundToInt(species.GenerationSize * species.NewDnaRate);
        if (nNew > 0)
        {
            TNG.AddRange(Enumerable.Range(0, nNew).Select((_) =>
                Dna.GenerateRandomDnaEncoding(
                    species.Inputs,
                    species.HiddenLayersNeuronCount,
                    CarSpecies.Outputs,
                    species.OutputLayerActivation,
                    species.HeterogeneousHiddenActivation
                )
            ));
        }

        //  Preserve top survivors 
        var previousGenerationOrderedByFitness = previousGeneration.OrderByDescending((dna => dna.RawFitnessRating));
        int nElites = Mathf.RoundToInt(species.GenerationSize * species.ProportionUnchanged);
        if (nElites > 0) TNG.AddRange(previousGenerationOrderedByFitness.Take(nElites).Select(dna => Dna.Clone(dna)));

        // Add mutated versions of elites
        int nMutatedElites = Mathf.RoundToInt(species.GenerationSize * species.ProportionMutatantsOfUnchanged);
        if (((species.GenerationSize - (nElites + nMutatedElites + nNew)) % 2 == 1)) nMutatedElites++; // make sure remaining spaces for offspring is an even number
        for (int i = 0; i < nMutatedElites; i++)
        {
            Dna randomElite = Darwin.SelectRandomBasedOnFitness(
                previousGenerationOrderedByFitness.Take(Mathf.RoundToInt(species.GenerationSize * 0.2f)).ToList()
            );
            Dna mutatedElite = Dna.CloneAndMutate(randomElite, DnaHeritage.MutatedElite, species.MutationSeverity, species.ActivationMutationSeverity);
            TNG.Add(mutatedElite);
        }

        // Populate the rest with offspring of previous
        int nOffspring = 0, nMutatedOffspring = 0;
        int freeSpacesForOffspring = species.GenerationSize - (nElites + nMutatedElites + nNew);
        int targetNumMutatedOffspring = Mathf.RoundToInt(freeSpacesForOffspring * species.OffspringMutationProbability);
        for (int i = 0; i < Mathf.RoundToInt(freeSpacesForOffspring / 2); i++)
        {
            Dna parent1 = Darwin.SelectRandomBasedOnFitness(previousGeneration);
            Dna parent2 = Darwin.SelectRandomBasedOnFitness(previousGeneration, parent1);
            if (parent1.Id == parent2.Id) Debug.LogError("FAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAILFAIL");
            List<Dna> children = new List<Dna>(2);

            /* Attempts at crossover may fail if the genomes of the 2 parents are too similar. If for example:
                p1 = 1,2,3,4
                p2 = 1,2,3,5
            then no matter how we try to cross them, the children will end up as clones of the parents. To mitigate this
            we try a few times and if we consistently fail, then we mutate the dna as a fallback
            */
            bool crossoverFailed = false;
            for (int crossoverAttempt = 0; crossoverAttempt < 5; crossoverAttempt++)
            {
                children = Dna.CreateOffspring(
                    parent1,
                    parent2,
                    species.CrossoverPasses,
                    species.IncludeActivationCrossover
                );
                crossoverFailed = parent1.Equals(children[0]) || parent1.Equals(children[1]) || parent2.Equals(children[0]) || parent2.Equals(children[1]);
                if (!crossoverFailed) break;
            }
            if (crossoverFailed) Debug.Log("Crossover failed after several attempts - selected parent genomes are likely too similar");

            if (crossoverFailed || (nMutatedOffspring <= targetNumMutatedOffspring && Random.Range(0f, 1f) < species.OffspringMutationProbability))
            {
                TNG.AddRange(children.ConvertAll(c => Dna.CloneAndMutate(c, DnaHeritage.MutatedOffspring, species.MutationSeverity, species.ActivationMutationSeverity)));
                nMutatedOffspring += 2;
            }
            else
            {
                TNG.AddRange(children);
                nOffspring += 2;
            }
        }

        string genSummary = string.Format(
            "Created {0} agents of species {1}. {2} new, {3} elites, {4} mutated elites, {5} offspring, and {6} mutated offspring",
            TNG.Count, species.name, nNew, nElites, nMutatedElites, nOffspring, nMutatedOffspring
        );
        Debug.Log(genSummary);
        DnaUtils.DebugGenerationDiff(previousGeneration, TNG);

        return TNG;
    }

    private IEnumerator DoEvolution(List<CarBrain> previousGenerationPhenotypes, bool isFirstGeneration = false)
    {
        Debug.Log("Creating generation " + GenerationHistory.Count + " of " + Species.name);
        if (GenerationHistory.Count % GenerationsPerSpawnLocation == 0) spawnLocation = SpawnLocations.GetNext(spawnLocation);

        List<Dna> newDnaList = isFirstGeneration ?
            SpeciesEvolver.CreateGenerationDna(Species) :
            SpeciesEvolver.CreateGenerationDna(Species, GenerationHistory);

        if (previousGenerationPhenotypes.Count != newDnaList.Count)
            Debug.LogError("Phenotype/Genotype count mismatch! " + newDnaList.Count + "/" + previousGenerationPhenotypes.Count);

        // Show selection process for visual feedback if crossover was performed
        yield return SelectedForBreeding.Count > 0 ? StartCoroutine(ShowSelectionProcess()) : null;
        ReleaseGeneration(newDnaList, previousGenerationPhenotypes);
    }

    private void ReleaseGeneration(List<Dna> genePool, List<CarBrain> phenotypesPool)
    {
        if (genePool.Count != Species.GenerationSize) // TODO: loading a save with mismatching pop size
            Debug.LogError("Phenotype/Genotype count mismatch! " + genePool.Count + "/" + Species.GenerationSize);

        for (int i = 0; i < genePool.Count; i++)
        {
            Dna newDna = genePool[i];
            CarBrain carBrain = phenotypesPool[i];

            newDna.OnSelectedForBreedingCb = () => SelectedForBreeding.Add(carBrain);
            carBrain.Arise(newDna, spawnLocation.transform.position, spawnLocation.transform.rotation);
        }
        CurrentlyAlive = new List<CarBrain>(GenerationPool);
        Debug.Log("Generation " + GenerationHistory.Count + " of " + Species.name + " created and released");
    }

    private void GenerationFinished()
    {
        GenerationData data = new GenerationData(GenerationHistory.Count, SpawnLocations.Points.IndexOf(spawnLocation), GenerationPool.ConvertAll(b => b.Dna));
        GenerationHistory.Add(data);
        Debug.Log(
            "Generation " + GenerationHistory.Count + " of " + Species.name + " finished.\n" +
            "Best fitness: " + data.BestFitness +
            ", Avg.: " + data.AverageFitness
        );
        StartCoroutine(DoEvolution(GenerationPool));
    }

    private void HandleIndividualDied(CarBrain deceased)
    {
        CurrentlyAlive.Remove(deceased);
        if (CurrentlyAlive.Count == 0) GenerationFinished();
    }

    private IEnumerator ShowSelectionProcess()
    {
        foreach (var brain in SelectedForBreeding)
        {
            brain.transform.localScale += Vector3.up;
            yield return new WaitForSeconds(0.1f);
        }

        // pause to observe
        yield return new WaitForSeconds(0.5f);

        // reset
        foreach (var brain in SelectedForBreeding)
            brain.transform.localScale = Vector3.one;

        SelectedForBreeding.Clear();
    }

    private void InstantiatePhenotypes()
    {
        for (int i = 0; i < Species.GenerationSize; i++)
        {
            CarBrain b = Instantiate(Species.PopulationPrefab).GetComponent<CarBrain>();
            GenerationPool.Add(b);
            b.Initialise(Species, spawnLocation.transform.position, spawnLocation.transform.rotation, HandleIndividualDied);
        }
    }

    private void Start()
    {
        spawnLocation = SpawnLocations.Points[0];
        InstantiatePhenotypes();
        StartCoroutine(DoEvolution(GenerationPool, true));
    }
}
