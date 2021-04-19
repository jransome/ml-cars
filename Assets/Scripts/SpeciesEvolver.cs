using RansomeCorp.AI.Evolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeciesEvolver : MonoBehaviour
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

    public static List<Dna> CreateGenerationDna(CarSpecies species, List<Dna> previousGeneration = null) // TODO: move to darwin class
    {
        if (previousGeneration == null)
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
        int nUnchanged = Mathf.RoundToInt(species.GenerationSize * species.ProportionUnchanged);
        if (nUnchanged > 0)
        {
            TNG.AddRange(
                previousGenerationOrderedByFitness
                    .Take(nUnchanged)
                    .Select(dna => Dna.Clone(dna))
            );
        }

        // Add mutated verstions of elites
        int nMutatedUnchanged = Mathf.RoundToInt(species.GenerationSize * species.ProportionMutatantsOfUnchanged);
        if (((species.GenerationSize - (nUnchanged + nMutatedUnchanged + nNew)) % 2 == 1)) nMutatedUnchanged++; // make sure remaining spaces for offspring is an even number
        for (int i = 0; i < nMutatedUnchanged; i++)
        {
            Dna randomElite = Darwin.SelectRandomBasedOnFitness(
                previousGenerationOrderedByFitness.Take(Mathf.RoundToInt(species.GenerationSize * 0.2f)).ToList()
            );
            TNG.Add(Dna.CloneAndMutate(randomElite, DnaHeritage.Mutated, species.MutationSeverity, species.ActivationMutationSeverity));
        }

        // Populate the rest with offspring of previous
        int nOffspring = 0, nMutatedOffspring = 0;
        int freeSpacesForOffspring = nUnchanged + nMutatedUnchanged + nNew;
        for (int i = 0; i < Mathf.RoundToInt((species.GenerationSize - freeSpacesForOffspring) / 2); i++)
        {
            Dna parent1 = Darwin.SelectRandomBasedOnFitness(previousGeneration);
            Dna parent2 = Darwin.SelectRandomBasedOnFitness(previousGeneration, parent1);
            List<Dna> children = Dna.CreateOffspring(
                parent1,
                parent2,
                species.CrossoverSeverity,
                species.ActivationCrossoverSeverity
            ).ConvertAll(child =>
            {
                if (Random.Range(0f, 1f) > species.OffspringMutationProbability)
                {
                    nOffspring++;
                    return child;
                }

                nMutatedOffspring++;
                return Dna.CloneAndMutate(child, DnaHeritage.BredAndMutated, species.MutationSeverity, species.ActivationMutationSeverity);
            });

            TNG.AddRange(children);
        }

        Debug.Log(
            "Created next generation of " + species.name + " with " + TNG.Count + " agents\n" +
            nNew + " new, " +
            nOffspring + " decendants, " +
            nMutatedOffspring + " mutated descendants " +
            nUnchanged + " elites from previous " +
            nMutatedUnchanged + " mutated elites from previous "
        );

        return TNG;
    }

    private IEnumerator DoEvolution(List<CarBrain> previousGenerationPhenotypes, bool isFirstGeneration = false)
    {
        Debug.Log("Creating generation " + ++GenerationCount + " of " + Species.name);
        List<Dna> newDnaList = isFirstGeneration ?
            SpeciesEvolver.CreateGenerationDna(Species) :
            SpeciesEvolver.CreateGenerationDna(Species, previousGenerationPhenotypes.ConvertAll(p => p.Dna));

        if (previousGenerationPhenotypes.Count != newDnaList.Count)
            Debug.LogError("Phenotype/Genotype count mismatch! " + newDnaList.Count + "/" + previousGenerationPhenotypes.Count);

        // Show selection process for visual feedback if crossover was performed
        yield return SelectedForBreeding.Count > 0 ? StartCoroutine(ShowSelectionProcess()) : null;

        for (int i = 0; i < newDnaList.Count; i++)
        {
            Dna newDna = newDnaList[i];
            CarBrain carBrain = previousGenerationPhenotypes[i];

            newDna.OnSelectedForBreedingCb = () => SelectedForBreeding.Add(carBrain);
            carBrain.Arise(newDna, transform.position, transform.rotation);
        }
        CurrentlyAlive = new List<CarBrain>(GenerationPool);
        Debug.Log("Generation " + GenerationCount + " of " + Species.name + " created and released");
    }

    private void HandleIndividualDied(CarBrain deceased)
    {
        CurrentlyAlive.Remove(deceased);
        if (CurrentlyAlive.Count == 0)
        {
            Debug.Log("Generation " + GenerationCount + " of " + Species.name + " finished");
            StartCoroutine(DoEvolution(GenerationPool));
        }
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
            b.Initialise(Species, transform.position, transform.rotation, HandleIndividualDied);
        }
    }

    private void Start()
    {
        InstantiatePhenotypes();
        StartCoroutine(DoEvolution(GenerationPool, true));
    }
}
