using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Randomises the zombie moan sound
[RequireComponent(typeof(AudioSource))]
public class MoanSound : MonoBehaviour 
{
	[SerializeField]
	private AudioSource _AudioSource = null; // component cached for optimisation


	void Start () 
	{
		_AudioSource.time = Random.Range (0.0f, _AudioSource.clip.length); // randomise time
		_AudioSource.pitch = Random.Range (0.55f, 0.7f); // randomise pitch
	}

}
