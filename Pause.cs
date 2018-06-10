using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

//Provides pause menu functionallity
public class Pause : MonoBehaviour 
{
	[SerializeField]
	private GameObject _contents = null; // Canvas contents (specifically the pause menu) to turn off/on

	private bool _paused = false; // paused state


	void Update ()
	{
		// pause/unpause depending on current state
		if (Input.GetButtonDown ("Pause")) 
			SwitchStateTo (!_paused);
	}
		
	public void SwitchStateTo(bool state)
	{
		_paused = state;

		// show/hide UI
		_contents.SetActive (_paused);

		//
		Time.timeScale = System.Convert.ToSingle (!_paused); 

		// show/hide cursor (need to see the cursor to use the UI)
		Cursor.visible = _paused;

		// Free/Lock the cursor
		if (Cursor.lockState == CursorLockMode.Confined || Cursor.lockState == CursorLockMode.Locked)
			Cursor.lockState = CursorLockMode.None;
		else
			Cursor.lockState = CursorLockMode.Locked;
	}

	// Always unpause when the the object is disabled 
	// so that the game isn't stuck on a Time.timeScale of 0
	void OnDisable()
	{
		SwitchStateTo (false);
	}

}
