using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis 
// Q5094111
// 14/01/2018

// Provides movement functionallity including jumping and sprinting
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour 
{
	[SerializeField]
	private float _speedMultiplier = 1.0f; // movement speed multiplier

	private Rigidbody _Rigidbody = null; // for Rigidbody movement (cache for optimisation)

	private bool _isMoving = false;
	public bool isMoving { get { return _isMoving; } }

	private bool _preventMovement = false;
	public bool preventMovement { set { _preventMovement = value; } }

	private bool _isJumping = false;
	private bool _doAJump = false; // to prevent double jumping

	[SerializeField]
	private float _jumpForce = 5.0f; // the higher the jump force the higher the jump

	[SerializeField]
	private float _sprintMultiplier = 2.0f;


	void Start () 
	{
		// cache components
		_Rigidbody = this.GetComponent<Rigidbody> ();
	}

	private void Update() // for input
	{
		if (Time.timeScale == 0.0f)
			return;

		// jump input
		if (Input.GetButtonDown ("Jump")) 
		{
			if (Mathf.Approximately (_Rigidbody.velocity.y, 0.0f))
				_isJumping = false;

			if (_isJumping == true)
				return;

			_doAJump = true;
		} 
	}

	void FixedUpdate () // for physics
	{
		if (_preventMovement == true)
			return;

		if (Time.timeScale == 0.0f)
			return;
	
		// jump check
		JumpUpdate ();

		// get keyboard/gamepad input
		float inHorizontal = Input.GetAxisRaw ("Horizontal");
		float inVertical = Input.GetAxisRaw ("Vertical");

		if (Camera.main == null)
			return;
		
		// create translation vector relative to the player
		Transform directionReference = this.transform;
		Vector3 translation = directionReference.right * inHorizontal *  _speedMultiplier
							+ directionReference.forward * inVertical * _speedMultiplier;

		// sprint check
		if (Input.GetButton ("Sprint"))
			translation *= _sprintMultiplier;


		_isMoving = (translation != Vector3.zero);

		// move using the Rigidbody so that collisions are calculated
		_Rigidbody.MovePosition (this.transform.position + (translation * Time.fixedDeltaTime));
	}


	private void JumpUpdate()
	{
		if (_doAJump == false)
			return;

		// to prevent double jump
		_doAJump = false;

		// add jump force
		_Rigidbody.AddForce (this.transform.up * _jumpForce, ForceMode.VelocityChange);
		_isJumping = true;
	}
}
