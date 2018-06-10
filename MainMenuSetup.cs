using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Ensures that the mouse can be controlled on the main menu
public class MainMenuSetup : MonoBehaviour 
{

	void Start () 
	{
		Time.timeScale = 1.0f; 
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

}
