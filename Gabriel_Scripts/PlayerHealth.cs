using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMesh Pro asset (for clear Text)

// Gabriel Lewis
// Q5094111

// Provides manipulation of health and a death state
public class PlayerHealth : MonoBehaviour 
{
	[SerializeField]
	private GameObject _deathCanvas = null; // the Canvas to show/hide

	[SerializeField]
	private int _initialHealth = 10;

	[SerializeField]
	private TextMeshProUGUI _healthText = null; // to update with the current health

	[SerializeField]
	private AudioSource _AudioSource = null; // to play sounds such as getting bit

	[SerializeField]
	private AudioClip _biteClip = null; // sound to be played when getting bit

	private int _currentHealth;


	void Start () 
	{
		// initialise
		_currentHealth = _initialHealth;
		_healthText.text = _currentHealth.ToString ();
	}


	public void ReduceHealth(int amount)
	{
		_currentHealth -= amount;

		// health cannot be below 0
		if (_currentHealth < 0)
			_currentHealth = 0;
	
		// update text
		_healthText.text = _currentHealth.ToString ();

		//play bite sound
		_AudioSource.PlayOneShot (_biteClip);

		// if health is 0 then the player is dead
		if (_currentHealth <= 0) 
			Die ();
	}

	public void AddHealth(int amount)
	{
		_currentHealth += amount;

		// update text
		_healthText.text = _currentHealth.ToString ();
	}

	private void Die()
	{
		// show Death Screen UI
		_deathCanvas.SetActive (true);

		// show and free mouse to use on the UI
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Destroy the player from the scene
		Destroy (this.gameObject);
	}
}
