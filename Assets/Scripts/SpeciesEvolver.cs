using RansomeCorp.AI.Evolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeciesEvolver : MonoBehaviour
{
    public CarSpecies Species;
    public SpawnLocations SpawnLocations;
    public Transform CurrentSpawnLocation = null;
    public int GenerationsPerSpawnLocation = 15;

    public List<Generation> GenerationHistory { get; private set; } = new List<Generation>();
    public List<CarBrain> PhenotypesPool { get; private set; } = new List<CarBrain>();
    public List<CarBrain> CurrentlyAlive { get; private set; } = new List<CarBrain>();
    public List<CarBrain> SelectedForBreeding { get; private set; } = new List<CarBrain>(); // used purely for visual feedback

    public CarBrain MostSuccessfulAlive
    {
        get => PhenotypesPool.Where(b => b.IsAlive).OrderByDescending(b => b.Fitness).DefaultIfEmpty(null).First();
    }

    public void LoadGeneration(SaveData saveData)
    {
        GenerationHistory = saveData.GenerationHistory;
        Generation currentGen = GenerationHistory.Last();

        // In case the number of spawn locations changed since this generation was saved
        Transform spawnLocation = currentGen.SpawnLocationIndex < SpawnLocations.Locations.Count ?
            SpawnLocations.Locations[currentGen.SpawnLocationIndex] :
            CurrentSpawnLocation;
        
        ReleaseGeneration(currentGen, spawnLocation); // TODO: depends on phenotypes already having been instanced
    }

    private IEnumerator CreateNextGeneration()
    {
        if (GenerationHistory.Count != 0 && GenerationHistory.Count % GenerationsPerSpawnLocation == 0)
            CurrentSpawnLocation = SpawnLocations.GetNext(CurrentSpawnLocation);

        int spawnLocationIndex = SpawnLocations.GetIndex(CurrentSpawnLocation);
        Debug.Log($"Creating generation {GenerationHistory.Count} of {Species.name} at spawn location {spawnLocationIndex}");

        Generation TNG = GenerationHistory.Count == 0 ?
            Generation.CreateSeed(Species, spawnLocationIndex) :
            Generation.FromPrevious(GenerationHistory.Last(), spawnLocationIndex);

        // Show selection process for visual feedback if crossover was performed
        yield return SelectedForBreeding.Count > 0 ? StartCoroutine(ShowSelectionProcess()) : null;
        ReleaseGeneration(TNG, CurrentSpawnLocation);
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

    private void ReleaseGeneration(Generation TNG, Transform spawnLocation)
    {
        if (PhenotypesPool.Count != TNG.GenePool.Count) // TODO: loading a save with mismatching pop size
            Debug.LogError($"Phenotype/Genotype count mismatch! {TNG.GenePool.Count}/{PhenotypesPool.Count}");

        for (int i = 0; i < TNG.GenePool.Count; i++)
        {
            Dna newDna = TNG.GenePool[i];
            CarBrain carBrain = PhenotypesPool[i];

            newDna.OnSelectedForBreedingCb = () => SelectedForBreeding.Add(carBrain);
            carBrain.Arise(newDna, spawnLocation.transform.position, spawnLocation.transform.rotation);
        }
        CurrentlyAlive = new List<CarBrain>(PhenotypesPool);
        GenerationHistory.Add(TNG);
    }

    private void GenerationFinished()
    {
        Generation gen = GenerationHistory.Last();
        gen.Finish();
        Debug.Log(
            $"Generation {GenerationHistory.Count} of {Species.name} finished.\n" +
            $"Fitness (Best/Average): {gen.performanceData.BestFitness}/{gen.performanceData.AverageFitness}"
        );
        StartCoroutine(CreateNextGeneration());
    }

    private void HandleIndividualDied(CarBrain deceased)
    {
        CurrentlyAlive.Remove(deceased);
        if (CurrentlyAlive.Count == 0) GenerationFinished();
    }

    private void InstantiatePhenotypes()
    {
        for (int i = 0; i < Species.GenerationSize; i++)
        {
            CarBrain b = Instantiate(Species.PopulationPrefab).GetComponent<CarBrain>();
            PhenotypesPool.Add(b);
            b.Initialise(Species, CurrentSpawnLocation.transform.position, CurrentSpawnLocation.transform.rotation, HandleIndividualDied);
        }
    }

    private void Start()
    {
        CurrentSpawnLocation ??= SpawnLocations.Locations[0];
        InstantiatePhenotypes();
        StartCoroutine(CreateNextGeneration());
    }
}
