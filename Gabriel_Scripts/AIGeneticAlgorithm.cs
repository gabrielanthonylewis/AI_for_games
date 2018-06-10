using System;
using System.Collections.Generic;

// Gabriel Lewis
// Q5094111

// Holds the DNA pool and provides functionallity to
// choose parents for splicing based on the fitness rating 
public class AIGeneticAlgorithm<T> // as a Template as the type of data within the genes might change
{
	// the AI DNA pool
	private List<AIDNA<T>> _population;
	public List<AIDNA<T>> population { get { return _population; } }

	// how many generations have passed
	private int _currGeneration;
	public int currentGeneration { get { return _currGeneration; } }

	// best genes and fitness rating of the last generation
	private T[] _bestGenes;
	public T[] bestGenes { get { return _bestGenes; } }
	private float _bestFitness;
	public float bestFitness { get { return _bestFitness; } }

	// how often a mutation should take place
	private float _mutationRate;
	public float mutationRate { get { return _mutationRate; } }

	// optimisation, keeps the best DNA in the next generation (without splicing)
	private int _elitism; 

	private List<AIDNA<T>> _newPopulation; // optimisation
	private Random _rand; // save seperately to prevent lots of creation which leads to same sequence (not random)
	private float _fitnessTotal; // current generation's fitnessTotal
	private int _DNACount; // number of DNA in the pool

	// function references as the functions are in different scripts
	private Func<T> _getRandomGene;
	private Func<int, float> _fitnessFunction;

	// constructor
	public AIGeneticAlgorithm(int populationCount, int dnaCount, Random rand, Func<T> getRandomGene, Func<int, float> fitnessFunction, 
		int elitism = 1, float mutationRate = 0.01f)
	{
		// initialisation
		_currGeneration = 1;
		_elitism = elitism;
		_mutationRate = mutationRate;
		_population = new List<AIDNA<T>> (populationCount);
		_newPopulation = new List<AIDNA<T>> (populationCount);
		_rand = rand;
		_DNACount = dnaCount;
		_getRandomGene = getRandomGene;
		_fitnessFunction = fitnessFunction;

		_bestGenes = new T[dnaCount];

		for (int i = 0; i < populationCount; i++) 
			_population.Add (new AIDNA<T> (dnaCount, rand, getRandomGene, fitnessFunction, true));
	}

	public void NewGeneration(int newDNACount = 0, bool crossoverNewDNA = false)
	{
		// amount of DNA to hold in the new population
		int finalCount = _population.Count + newDNACount;

		// if there are no DNA to be generated then return
		if (finalCount <= 0)
			return;

		// if there is an existing population
		if (_population.Count > 0) 
		{
			// recalculate the fitness rating
			CalculateFitness ();

			// sort the by best fitness rating
			_population.Sort (CompareDNA);
		}

		_newPopulation.Clear ();

		// generate a new population
		for (int i = 0; i < finalCount; i++) // finalCount or _population.Count
		{
			// elitism
			if (i < _elitism && i < _population.Count) 
			{
				_newPopulation.Add (_population [i]);
			}
			else if (i < _population.Count || crossoverNewDNA == true) 
			{
				// splice the DNA of the best parents
				AIDNA<T> parentA = ChooseParent ();
				AIDNA<T> parentB = ChooseParent ();

				// splice
				AIDNA<T> child = parentA.Splice (parentB); 

				// change to mutate
				child.Mutate (_mutationRate);

				_newPopulation.Add (child);
			}
			else
			{
				// add a new DNA to the pool
				_newPopulation.Add (new AIDNA<T> (_DNACount, _rand, _getRandomGene, _fitnessFunction, true));
			}
		}

		// keep track of old and new population
		List<AIDNA<T>> tempList = _population;
		_population = _newPopulation;
		_newPopulation = tempList;

		_currGeneration++;
	}

	// Better fitness rating means better DNA
	public int CompareDNA(AIDNA<T> a, AIDNA<T> b)
	{
		if (a.fitnessRating > b.fitnessRating)
			return -1;
		else if (a.fitnessRating < b.fitnessRating)
			return 1;
		else
			return 0;
	}

	public void CalculateFitness()
	{
		_fitnessTotal = 0;

		// find the best DNA in the population
		AIDNA<T> best = _population [0];
		for (int i = 0; i < _population.Count; i++) 
		{
			_fitnessTotal += _population [i].CalculateFitness (i);

			if (_population [i].fitnessRating > best.fitnessRating)
				best = _population [i];
		}

		// store best fitness rating and its genes
		_bestFitness = best.fitnessRating;
		best.genes.CopyTo (_bestGenes, 0);
	}

	private AIDNA<T> ChooseParent()
	{
        // roulette wheel selection
		double rand = _rand.NextDouble () * _fitnessTotal;

		for (int i = 0; i < _population.Count; i++)
		{
			if (rand < _population [i].fitnessRating)
				return _population [i];
			
			rand -= _population [i].fitnessRating;
		}
			
		return null;
	}
}
