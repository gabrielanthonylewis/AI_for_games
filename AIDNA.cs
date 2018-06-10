using System;

// Gabriel Lewis
// Q5094111

// One piece of DNA to be held in the DNA pool
public class AIDNA<T> // Template as DNA can hold any type of gene
{
	// the genes each AI holds in its DNA (will change with every generation)
	private T[] _genes;
	public T[] genes { get { return _genes; } }

	// fitness rating is how good the DNA is
	private float _fitnessRating = 0.0f;
	public float fitnessRating { get { return _fitnessRating; } }

	private Random _rand; // saved seperately to ensure true randomness

	// reference to functions stored on another script
	private Func<T> _getRandomGene; 
	private Func<int, float> _fitnessFunction;

	// constructor
	public AIDNA(int count, Random rand, Func<T> getRandomGene, Func<int, float> fitnessFunction, bool initialiseGenes = true) 
	{
		// initialisation
		_genes = new T[count];
		_fitnessRating = 0.0f;
		_rand = rand;
		_getRandomGene = getRandomGene;
		_fitnessFunction = fitnessFunction;

		if (initialiseGenes == false)
			return;
		
		for (int i = 0; i < _genes.Length; i++) 
			_genes [i] = getRandomGene ();
	}

	public float CalculateFitness(int index)
	{
		_fitnessRating = _fitnessFunction (index);
		return _fitnessRating;
	}

	public AIDNA<T> Splice(AIDNA<T> otherParent)
	{
		AIDNA<T> child = new AIDNA<T> (_genes.Length, _rand, _getRandomGene, _fitnessFunction, false);

		// splice choosing the next gene from a either of the parents 
		// (50% chance of parent A, 50% chance of parent B)
		for (int i = 0; i < _genes.Length; i++) 
		{
			if(_rand.NextDouble() < 0.5)
				child._genes [i] =  _genes[i];
			else
				child._genes [i] = otherParent._genes [i];
		}

		return child;
	}

	public void Mutate(float mutationRate)
	{
		for (int i = 0; i < _genes.Length; i++) 
		{
			if (_rand.NextDouble() < mutationRate) 
				_genes [i] = _getRandomGene ();
		}
	}


};
	

