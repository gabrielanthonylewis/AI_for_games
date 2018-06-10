using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Gabriel Lewis
// Q5094111

// Scroll the scrollbar down over time (automatically)
public class AutoScroll : MonoBehaviour 
{
	[SerializeField]
	private ScrollRect _ScrollRect = null;

	private float _speed = 0.03f; // how fast he scrollbar moves


	void Update () 
	{
		// dont go below the bottom
		if (_ScrollRect.verticalNormalizedPosition <= 0.0f) 
			return;

		// move scrollbar down
		_ScrollRect.verticalNormalizedPosition -= Time.deltaTime * _speed;
	}
}
