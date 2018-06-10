using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gabriel Lewis
// Q5094111

// For scene loading and exiting the game
public class ChangeScene : MonoBehaviour 
{

	public void ChangeTo(string name)
	{
		SceneManager.LoadScene (name);
	}

	public void ChangeTo(int id)
	{
		SceneManager.LoadScene (id);
	}

	public void QuitGame()
	{
		Application.Quit ();
	}
}
