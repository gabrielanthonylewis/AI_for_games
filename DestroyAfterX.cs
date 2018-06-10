using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Destroys the object after x amount of seconds
public class DestroyAfterX : MonoBehaviour 
{
	[SerializeField]
	private float _time = 1.0f; // time until the object is destroyed


	void Start () 
	{
		StartCoroutine ("DestroyTimer");
	}
	
	private IEnumerator DestroyTimer()
	{
		yield return new WaitForSeconds (_time);

		Destroy (this.gameObject);
	}
}
