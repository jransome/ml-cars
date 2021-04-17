using RansomeCorp.AI.Evolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class God : MonoBehaviour
{
    public CarSpecies Species;

    public int GenerationCount { get; private set; } = 0;
    public List<CarBrain> GenerationPool { get; private set; } = new List<CarBrain>();
    public List<CarBrain> CurrentlyAlive { get; private set; } = new List<CarBrain>();
    public List<CarBrain> SelectedForBreeding { get; private set; } = new List<CarBrain>(); // used purely for visual feedback

    public CarBrain MostSuccessfulAlive
    {
        get => GenerationPool.Where(b => b.IsAlive).OrderByDescending(b => b.Fitness).DefaultIfEmpty(null).First();
    }

    // public void LoadGeneration(PopulationData generation)
    // {
    //     TODO: generation size (and agent count)
    //     GenerationCount = generation.GenerationNumber;
    //     DnaStructure = generation.DnaStructure;
    //     ReleaseNewGeneration(generation.GenePool);
    // }

    private static Dna SelectRandomBasedOnFitness(List<Dna> parentPool, Dna excluding = null)
    {
        List<Dna> candidates = excluding == null ? parentPool : parentPool.Where(p => p != excluding).ToList();
        float totalFitness = parentPool.Aggregate(0f, (total, parent) => total + parent.RawFitnessRating); // TODO: optimise this
        List<KeyValuePair<Dna, float>> candidateChances = candidates.ConvertAll(c => new KeyValuePair<Dna, float>(c, c.RawFitnessRating / totalFitness));

        float diceRoll = Random.Range(0f, 1f);
        float cumulative = 0f;
        for (int i = 0; i < candidateChances.Count; i++)
        {
            cumulative += candidateChances[i].Value;
            if (diceRoll < cumulative) return candidateChances[i].Key;
        }

        Debug.LogWarning("Failed to choose new random parent by fitness...");
        return candidates[Random.Range(0, candidates.Count)];
    }

    private static List<Dna> CreateGenerationDna(CarSpecies species, List<Dna> previousGeneration = null)
    {
        if (previousGeneration == null)
        {
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

        List<Dna> TNG = new List<Dna>();

        // Preserve top survivors 
        int nUnchanged = Mathf.RoundToInt(species.GenerationSize * species.ProportionUnchanged);
        if (nUnchanged > 0)
        {
            TNG.AddRange(previousGeneration.OrderByDescending((dna => dna.RawFitnessRating)).Take(nUnchanged));
        }

        // Add fresh dna into next gen
        int nNew = Mathf.RoundToInt(species.GenerationSize * species.NewDnaRate);
        if ((species.GenerationSize - (nUnchanged + nNew) % 2 == 1)) nNew++; // make sure remaining spaces for offspring is an even number
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

        // Populate the rest with offspring of previous
        int offspring = 0, mutatedOffspring = 0;
        for (int i = 0; i < Mathf.RoundToInt((species.GenerationSize - (nUnchanged + nNew)) / 2); i++)
        {
            Dna parent1 = God.SelectRandomBasedOnFitness(previousGeneration);
            Dna parent2 = God.SelectRandomBasedOnFitness(previousGeneration, parent1);
            List<Dna> children = Dna.CreateOffspring(
                parent1,
                parent2,
                species.CrossoverSeverity,
                species.ActivationCrossoverSeverity
            ).ConvertAll(child =>
            {
                if (Random.Range(0f, 1f) > species.MutationProbability)
                {
                    offspring++;
                    return child;
                }

                mutatedOffspring++;
                return Dna.CloneAndMutate(child, DnaHeritage.BredAndMutated, species.MutationSeverity, species.ActivationMutationSeverity);
            });

            TNG.AddRange(children);
        }

        Debug.Log(
            "Created generation made up of " +
            nNew + " new, " +
            offspring + " descendants, " +
            mutatedOffspring + " mutated descendants, and " +
            nUnchanged + " unchanged genomes"
        );

        return TNG;
    }

    private void HandleIndividualDied(CarBrain deceased, float fitness)
    {
        CurrentlyAlive.Remove(deceased);
        if (CurrentlyAlive.Count == 0)
            StartCoroutine(DoEvolution(GenerationPool));
    }

    private IEnumerator DoEvolution(List<CarBrain> previousGenerationPhenotypes, bool isFirstGeneration = false)
    {
        Debug.Log("Creating generation " + ++GenerationCount + "...");
        List<Dna> newDnaList = isFirstGeneration ?
            God.CreateGenerationDna(Species) :
            God.CreateGenerationDna(Species, previousGenerationPhenotypes.ConvertAll(p => p.Dna));

        if (newDnaList.Count != previousGenerationPhenotypes.Count)
            Debug.LogError("Phenotype/Genotype count mismatch!");

        // Show selection process for visual feedback
        yield return StartCoroutine(ShowSelectionProcess());

        for (int i = 0; i < newDnaList.Count; i++)
        {
            Dna newDna = newDnaList[i];
            CarBrain carBrain = previousGenerationPhenotypes[i];

            newDna.OnSelectedForBreedingCb = () => SelectedForBreeding.Add(carBrain);
            carBrain.Arise(newDna, transform.position, transform.rotation);
        }
        CurrentlyAlive = new List<CarBrain>(GenerationPool);
    }

    private IEnumerator ShowSelectionProcess()
    {
        foreach (var brain in SelectedForBreeding)
        {
            brain.transform.localScale += Vector3.up;
            yield return new WaitForSeconds(0.2f);
        }

        // pause to observe
        yield return new WaitForSeconds(1f);

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
            b.Initialise(Species, transform.position, transform.rotation, HandleIndividualDied);
        }
    }

    private void Start()
    {
        InstantiatePhenotypes();
        StartCoroutine(DoEvolution(GenerationPool, true));
    }
}
