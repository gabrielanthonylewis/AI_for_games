using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Triggers the End Game state and UI
public class EndGameTrigger : MonoBehaviour 
{
	[SerializeField]
	private GameObject _EndGameCanvas = null; // end game UI to turn on


	// when the player enters the trigger area
	// stop time and show the end game UI
	void OnTriggerEnter(Collider other)
	{
		if (other.tag != "Player")
			return;

		Time.timeScale = 0.0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

		_EndGameCanvas.SetActive (true);
	}

	// ensure when the object is destroyed the game is not stuck on a
	// Time.timeScale of 0
	void OnDisable()
	{
		Time.timeScale = 1.0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
