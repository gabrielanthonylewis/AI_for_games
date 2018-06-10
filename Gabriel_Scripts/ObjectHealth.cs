using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Barricade health, deals with destruction and deletion
public class ObjectHealth : MonoBehaviour 
{
	[SerializeField]
	private int _health = 10; 

	[SerializeField]
	private GameObject[] _physicalHealth; // to show visual destruction

	private int _obstacleLayer = 12; // the Barricade layer

	[SerializeField]
	private Grid _grid = null;

	[SerializeField]
	private int _destroyEveryX = 0;

	[SerializeField]
	private int _nextDestroy = 0; // when the next visual destruction should take place

	[SerializeField]
	private int _currDestroy = 0; // the current physical destruction level


	void Start()
	{
		// initialisation
		if (_physicalHealth.Length > 0) 
		{
			_destroyEveryX = Mathf.CeilToInt(_health / (_physicalHealth.Length + 1.0f));
			_nextDestroy = _destroyEveryX;
		}
	}

	public void TakeDamage(int damage)
	{
		_health -= damage;
		_nextDestroy -= damage;

		// deal with physical destruction if next destroy is 0
		if (_destroyEveryX != 0 && _nextDestroy <= 0) 
		{
			int justincase = 0; // so there is not an infinte loop
			while (_nextDestroy != _destroyEveryX) 
			{
				justincase++;
				if (justincase > 20)
					break;
				
				if (_nextDestroy + _destroyEveryX > _destroyEveryX)
					break;

				// update next destroy
				_nextDestroy += _destroyEveryX;

				// destroy part of the object (visual destruction)
				if(_currDestroy < _physicalHealth.Length)
					Destroy(_physicalHealth [_currDestroy]);
				
				_currDestroy++;
			}
		}

		// destroy whole object
		if (_health <= 0) 
		{
			// if obstacle then set to walkable
			if (this.gameObject.layer == _obstacleLayer) 
			{
				Node[] tiles = _grid.TilesAt (this.transform.position, this.transform.localScale);
				foreach (Node tile in tiles) 
					_grid.SetWalkable (tile.gridX, tile.gridY, true);
			}

			// destroy the object in the scene
			Destroy (this.gameObject);
		}
	}
}
