using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Attached to the boss enemy, waves are spawned and genetics are assigned
public class AISpawner : MonoBehaviour 
{
	[System.Serializable]
	public struct EvolutionTraits
	{
		public float muscleArm;
		public float muscleLeg;
	};

	[SerializeField] 
	private int _enemyCount = 2;// numer of 
	[SerializeField] 
	private float _mutationRate =  0.01f; // how often mutation occurs

	[SerializeField]
	private int _elitism = 0; // how many of the best should be kept the same

	private float _maxArm = 5.0f; // max damage 
	private float _maxLeg = 5.0f; // max damage

	private System.Random _random = new System.Random(); // ensure true randomness by only creating once
	private AIGeneticAlgorithm<EvolutionTraits> _genticAlgorithm;

	// DEBUG INFO
	[SerializeField]
	private int currentGeneration = 0;
	[SerializeField]
	private List<EvolutionTraits>  debugPopulation = new List<EvolutionTraits>();
	//END DEBUG INFO

	[SerializeField]
	private GameObject _enemyPrefab = null; // prefab to spawn in

	[SerializeField]
	private Grid _Grid = null; // cached reference to the Grid

	private List<GameObject> _AIPopulation = new List<GameObject>(); // reference to each physical member of the population


	void Start()
	{
		// create the genetic algorithm with the specified parameters
		_genticAlgorithm = new AIGeneticAlgorithm<EvolutionTraits>(_enemyCount, 1, _random, GetRandomGene, FitnessFunction, _elitism, _mutationRate);

		// represents the horde for debugging
		debugPopulation.Clear();
		for (int i = 0; i < _genticAlgorithm.population.Count; i++) 
			debugPopulation.Add(_genticAlgorithm.population[i].genes[0]);

		// spawn one wave of enemies
		SpawnWave ();
	}


	public void AIKilled(GameObject aiObject)
	{
		// remove from the physical population
		_AIPopulation.Remove (aiObject);

		// spawn a new wave only once there are no enemies left
		if (_AIPopulation.Count <= 0)
			SpawnWave ();
	}

	private void SpawnWave()
	{
		if (_genticAlgorithm.bestFitness == 1.0f) 
		{
			Debug.Log ("!!!!!BEST DNA REACHED!!!!!!!");
		}
		else 
		{
			// generate a new generation of DNA
			_genticAlgorithm.NewGeneration();

			currentGeneration++;

			// update debug representation
			debugPopulation.Clear();
			for (int i = 0; i < _genticAlgorithm.population.Count; i++) 
				debugPopulation.Add(_genticAlgorithm.population[i].genes[0]);
		}

		// clear the population ready to spawn in new enemies
		_AIPopulation.Clear ();

		// keep track of spawn in locations so that there is no spawning
		// on a node that already contains an enemy
		List<Node> spawnedInNodes = new List<Node> ();

		for (int i = 0; i < _genticAlgorithm.population.Count; i++) 
		{
			// get the traits from the population
			AIDNA<EvolutionTraits> traits =	_genticAlgorithm.population [i];

			// spawn in the enemy using the prefab
			GameObject newAI = Instantiate (_enemyPrefab);

			// apply to speed and damage from the traits/genes
			newAI.GetComponent<SimpleEnemyState> ().damage = Mathf.RoundToInt(traits.genes [0].muscleArm);
			newAI.GetComponent<SimpleEnemyState> ().speed = traits.genes [0].muscleLeg;


			// spawn enemy at an adjacent tile from the Boss (this.transform)
			List<Node> neighbours = _Grid.GetNeighbours (_Grid.NodeFromWorldPoint (this.transform.position));
			foreach (Node node in neighbours) 
			{
				// node must be walkable and not have been spawned on by another enemy
				if (node.walkable == true && spawnedInNodes.Contains(node) == false)
				{
					// update position
					newAI.transform.position = new Vector3(node.worldPosition.x, 1.0f, node.worldPosition.z);

					_AIPopulation.Add (newAI);
					spawnedInNodes.Add (node);
					break;
				}
			}
		}
	}


	// For debugging
    private void PrintPopulation()
    {
        for (int i = 0; i < _genticAlgorithm.population.Count; i++)
        {
			Debug.Log (_genticAlgorithm.population [i].genes [0].muscleArm); 
               // + " , " + _genticAlgorithm.population[i].genes[0].muscleLeg);
        }
    }

	// For when a mutation occurs
	private EvolutionTraits GetRandomGene()
	{
		EvolutionTraits traits = new EvolutionTraits();

		// pick random genes keeping in mind the max limit
		traits.muscleArm = ((float)_random.NextDouble () * _maxArm);
		traits.muscleLeg = Mathf.Max(2.0f, ((float)_random.NextDouble() * _maxLeg));

        return traits;
	}

	// Calculates the fitness rating for a gene in the population
	private float FitnessFunction(int index)
	{
		float score = 0.0f;

		// score between 0 and 100%
		AIDNA<EvolutionTraits> dna = _genticAlgorithm.population[index];

        //one strong enemy is just as effective as many weak but fast enemies.
		// for this reason both the strengths are taken into one equation equally.
		score += dna.genes [0].muscleArm / _maxArm;
		score += dna.genes [0].muscleLeg / _maxLeg;
		score /= 2.0f; // as it is out of 1

		return score;
	}
}
