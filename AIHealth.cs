using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis 
// Q5094111

// Provides manipulation of health and a death state
public class AIHealth : MonoBehaviour 
{
	[SerializeField]
	private int _maxHealth = 5;

    [SerializeField]
    private GameObject horde; // to detach from the horde upon death

	[SerializeField]
	private int _initialHealth = 5;

	private int _currentHealth = 0;

    [SerializeField]
    private bool _cantDie = false;


	void Start()
	{
		// initialisation
		_currentHealth = _initialHealth;
        horde = GameObject.FindGameObjectWithTag("Horde");
	}


	public void Heal(int amount)
	{
		// don't do anything if at max health
		if (_currentHealth == _maxHealth)
			return;
		
		_currentHealth += amount;

		// limit to max health
		if (_currentHealth > _maxHealth)
			_currentHealth = _maxHealth;
	}

    public bool cantDie()
    {
        return _cantDie;
    }
		
	public void Wound(int amount)
	{
		_currentHealth -= amount;

		// if no health then die
		if (_currentHealth <= 0) 
			Die ();
	}

	private void Die()
	{
        if (_cantDie)
            return;

		// tell the spawn system that an AI has been killed (so it can spawn in a new wave)
        GameObject.FindObjectOfType<AISpawner>().AIKilled(this.gameObject);

		// make current tile walkable again
        Node node = GameObject.FindObjectOfType<Grid>().NodeFromWorldPoint(this.transform.position);
        GameObject.FindObjectOfType<Grid>().SetWalkable(node.gridX, node.gridY, true);

		// leave the horde
        horde.GetComponent<HordeScript>().leavehorde(this.gameObject);

		// destroy object from the scene
        Destroy(this.gameObject);
	}

}
