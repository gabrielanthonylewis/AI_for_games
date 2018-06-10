using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gabriel Lewis
// Q5094111

// Functionallity to move the camera in both first and third person
public class CameraMove : MonoBehaviour
{
	private enum CameraType
	{
		FirstPerson = 0,
		ThirdPerson = 1,
		Freelook = 2
	};

	[SerializeField]
	private LayerMask _ignoreMask; // for camera clipping avoidance, will avoid this mask

	// for exception handling
	private const CameraType DEFAULT_CAMERA_TYPE = CameraType.ThirdPerson; 

	[SerializeField]
	private CameraType _cameraType = CameraType.ThirdPerson;

	// limits to prevent camera going upside down
	private Vector2 _firstPersonLimits = new Vector2(50.0f, 300.0f);
	private Vector2 _thirdPersonLimits = new Vector2(90.0f, 310.0f);

	[SerializeField]
	private float _horizontalSpeedMultiplier = 1.0f;

	[SerializeField]
	private float _verticalSpeedMultiplier = 1.0f;

	[SerializeField]
	private Transform _cameraHolder = null;

	[SerializeField]
	private Rigidbody _graphicsHolder = null;

	[SerializeField]
	private GameObject _actualGraphicsHolder = null;

	[SerializeField]
	private Transform _player = null;

	[SerializeField]
	private Camera _mainCamera = null;

	[SerializeField]
	private Camera _weaponCamera = null;

    [SerializeField]
    private LayerMask _1stPersonMainCameraIgnore; // so that the gun doesn't visibilty go through the wall
    [SerializeField]
    private LayerMask _3rdPersonMainCameraIgnore; // so that the gun isn't visible through the player

    [SerializeField]
	private PlayerMove _PlayerMove = null; // cached for optimsation

	private bool _smoothingFreeLook = false;

	// the offset is where the camera should be placed in each axis
	private const float _firstPersonZOffset = 0.5f;
	private Vector3 _firstPersonZOffsetVector = new Vector3 (_firstPersonZOffset, _firstPersonZOffset, _firstPersonZOffset);
	private const float _firstPersonYOffset = 1.0f;
	private Vector3 _firstPersonYOffsetVector = new Vector3 (_firstPersonYOffset, _firstPersonYOffset, _firstPersonYOffset);
	private const float _firstPersonXOffset = 0.0f;
	private Vector3 _firstPersonXOffsetVector = new Vector3 (_firstPersonXOffset, _firstPersonXOffset, _firstPersonXOffset);

	private const float _thirdPersonZOffset = -3.5f;
	private Vector3 _thirdPersonZOffsetVector = new Vector3 (_thirdPersonZOffset, _thirdPersonZOffset, _thirdPersonZOffset);
	private const float _thirdPersonYOffset = 1.333f;
	private Vector3 _thirdPersonYOffsetVector = new Vector3 (_thirdPersonYOffset, _thirdPersonYOffset, _thirdPersonYOffset);
	private const float _thirdPersonXOffset = 0.333f;
	private Vector3 _thirdPersonXOffsetVector = new Vector3 (_thirdPersonXOffset, _thirdPersonXOffset, _thirdPersonXOffset);

	private const float _freeLookSpeedMultiplier = 5.0f;

	private bool _isAiming = false;

    [SerializeField]
	private PlayerShoot _PlayerShoot = null;

	[SerializeField]
	private Transform _gunHolder = null;

	// deafult gun position (changed when leaning)
	private Vector3 _firstPersonGunHolderPosition = new Vector3 (0.326f, 0.786f, 0.856f);
	private Vector3 _thirdPersonGunHolderPosition = Vector3.zero;

	// for leaning
	private bool _rightShoulder = true;


	void Start ()
	{
		// initialisation
        _PlayerMove = _player.GetComponent<PlayerMove>();
        _PlayerShoot = _player.GetComponent<PlayerShoot>();

        _thirdPersonGunHolderPosition = _gunHolder.transform.localPosition;

		// hide mouse cursor and lock it to the middle of the screen
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		_player = GameObject.FindGameObjectWithTag ("Player").transform;

		if(_graphicsHolder == null)
			_graphicsHolder = _player.GetComponent<Rigidbody> ();
		
		if (_cameraHolder == null)
			_cameraHolder = this.transform;
		
		InitialiseCamera ();
	}
		
    void Update()
	{
		// change between first and third person
		if (Input.GetButtonDown ("ChangePerspective")) 
		{
			_cameraType++;
			if ((int)_cameraType >= 2)
				_cameraType = 0;

			InitialiseCamera ();
		}

		// aim input
		_isAiming = Input.GetButton ("Aim");
		if (Input.GetAxis ("Aim") == 1.0f)
			_isAiming = true;

		if (_cameraType == CameraType.ThirdPerson)
        {
			if (_gunHolder != null && !Input.GetButton("LeanRight") && !Input.GetButton("LeanLeft"))
           		 _gunHolder.transform.localPosition = _thirdPersonGunHolderPosition;

			// zoom in if aiming
            if (_isAiming)
			{
				_mainCamera.fieldOfView = Mathf.Lerp (_mainCamera.fieldOfView, 55.0f, 10.0f * Time.deltaTime);

                if(_rightShoulder)
				    _thirdPersonXOffsetVector = Vector3.Lerp (_thirdPersonXOffsetVector, new Vector3 (0.75F, 0.75F, 0.75F), 10.0f * Time.deltaTime);
                else
                    _thirdPersonXOffsetVector = Vector3.Lerp(_thirdPersonXOffsetVector, new Vector3(-0.75F, -0.75F, -0.75F), 10.0f * Time.deltaTime);
            }
            else
            {
				_mainCamera.fieldOfView = Mathf.Lerp (_mainCamera.fieldOfView, 65.0f, 10.0f * Time.deltaTime);
				_thirdPersonXOffsetVector = Vector3.Lerp (_thirdPersonXOffsetVector, new Vector3 (_thirdPersonXOffset, _thirdPersonXOffset, _thirdPersonXOffset), 10.0f * Time.deltaTime);
			}
        }
		else if (_cameraType == CameraType.FirstPerson) 
		{
			// zoom in if aiming
            if (_isAiming)
				_mainCamera.fieldOfView = Mathf.Lerp (_mainCamera.fieldOfView, 55.0f, 10.0f * Time.deltaTime);
			else
				_mainCamera.fieldOfView = Mathf.Lerp (_mainCamera.fieldOfView, 65.0f, 10.0f * Time.deltaTime);
		}



		// lean input
        if (Input.GetButtonDown("LeanLeft"))
            _rightShoulder = false;

		// calculate new offset
        _firstPersonXOffsetVector = Vector3.Lerp(_firstPersonXOffsetVector, new Vector3(0.0f, 0.0f, 0.0f), 10.0f * Time.deltaTime);
    
 		// deal with leaning
        if (_rightShoulder == false)
        {

            if (Input.GetButton("LeanLeft"))
            {
                _thirdPersonXOffsetVector = new Vector3(-1.0f, -1.5f, -1.5f);
                _firstPersonXOffsetVector = new Vector3(-1.0f, -1.0f, -1.0f);

				if (_gunHolder != null && _cameraType == CameraType.FirstPerson )
					_gunHolder.transform.localPosition = Vector3.Lerp(_gunHolder.transform.localPosition, new Vector3(0.326f - 1.5f, 0.786f, 0.856f), 1000.0f * Time.deltaTime);
				else if (_gunHolder != null && _cameraType == CameraType.ThirdPerson)
					_gunHolder.transform.localPosition = Vector3.Lerp(_gunHolder.transform.localPosition, _thirdPersonGunHolderPosition + new Vector3(-1.0f, 0.0f, 0.0f), 1000.0f * Time.deltaTime);
            }
            else
            {
				if (_gunHolder != null && _cameraType == CameraType.FirstPerson)
					_gunHolder.transform.localPosition = Vector3.Lerp(_gunHolder.transform.localPosition, _firstPersonGunHolderPosition, 1000.0f * Time.deltaTime);
            }
        }
        else if (_rightShoulder == true)
        {
            if (Input.GetButton("LeanRight"))
            {
				_thirdPersonXOffsetVector = new Vector3(1.0f, 1.5f, 1.5f);
				_firstPersonXOffsetVector = new Vector3 (1.0f, 1.0f, 1.0f);

				if (_gunHolder != null && _cameraType == CameraType.FirstPerson )
					_gunHolder.transform.localPosition = Vector3.Lerp(_gunHolder.transform.localPosition, new Vector3(0.326f + 1.0f, 0.786f, 0.856f), 1000.0f * Time.deltaTime);
            }
            else
            {
				if (_gunHolder != null && _cameraType == CameraType.FirstPerson)
					_gunHolder.transform.localPosition = Vector3.Lerp(_gunHolder.transform.localPosition, _firstPersonGunHolderPosition, 1000.0f * Time.deltaTime);
            }
        }

        if (Input.GetButtonDown("LeanRight"))
            _rightShoulder = true;
    }

	void FixedUpdate()
	{
		// For testing
		if (Application.isEditor == true) 
		{
			if (Input.GetKeyDown (KeyCode.Backspace)) 
			{
				_cameraType = CameraType.Freelook;
				InitialiseFreelook ();
			}
		}
			
		float timeStep = Time.fixedDeltaTime;
		Vector2 mouseInput = new Vector2(Input.GetAxis ("Mouse X"), Input.GetAxis("Mouse Y"));

		float aimingReduction = Mathf.Clamp((System.Convert.ToSingle (!_isAiming)), 0.5f, 1.0f);
		Vector3 newRotation = new Vector3(_cameraHolder.localRotation.eulerAngles.x + (-mouseInput.y * aimingReduction * _verticalSpeedMultiplier) * timeStep,
			_cameraHolder.localRotation.eulerAngles.y + (mouseInput.x * aimingReduction * _horizontalSpeedMultiplier) * timeStep,
										  0.0f);

		// Apply different functionallity depending on the perspective
		switch (_cameraType) 
		{
			case CameraType.FirstPerson:

				UpdateFirstPersonCamera (newRotation, mouseInput, timeStep);

				if (_smoothingFreeLook)
					return;
			
				UpdateGraphics (newRotation, timeStep);	
				UpdateGunGraphics (timeStep);
				break;

			case CameraType.ThirdPerson:
			
				UpdateThirdPersonCamera (newRotation, mouseInput, timeStep);

				if (_smoothingFreeLook)
					return;
				
				if (_PlayerMove.isMoving || _isAiming == true || _PlayerShoot.shooting == true)
					UpdateGraphics (newRotation, timeStep);

				UpdateGunGraphics (timeStep);
				break;

			case CameraType.Freelook:

				UpdateFreelookCamera (newRotation, mouseInput, timeStep);
				break;
		}


		if (_graphicsHolder == null)
			return;

		// rotate gun model
		_graphicsHolder.transform.rotation = Quaternion.Lerp(_graphicsHolder.transform.rotation, new Quaternion(_graphicsHolder.transform.rotation.x, 
			_cameraHolder.rotation.y, _graphicsHolder.transform.rotation.z, _cameraHolder.rotation.w), 15.0f * Time.fixedDeltaTime);
	}


	private void UpdateFreelookCamera(Vector3 newRotation, Vector2 mouseInput, float timeStep)
	{
		// rotate
		if (_cameraHolder != null) 
		{
			newRotation.x = ApplyCameraLimits(newRotation.x, _firstPersonLimits.x, _firstPersonLimits.y, (mouseInput.y <= 0.0f));
			_cameraHolder.localRotation = Quaternion.Euler (newRotation);
		}

		// update position
		Vector2 positionInput = new Vector2(Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		this.transform.position += (this.transform.right * positionInput.x * _freeLookSpeedMultiplier
									+ this.transform.forward * positionInput.y * _freeLookSpeedMultiplier) * timeStep;
	}

	// moves and rotates third person camera depending on input
	private void UpdateThirdPersonCamera(Vector3 newRotation, Vector2 mouseInput, float timeStep)
	{
		if (_cameraHolder == null)
			return;
		if (_player == null)
			return;


		// snap back to normal camera if just used free look
		if (Input.GetButtonUp ("ThirdPersonFreeLook") == true) 
			_smoothingFreeLook = true;

		if (_smoothingFreeLook) 
		{
			_cameraHolder.localRotation = Quaternion.Slerp(_cameraHolder.localRotation, _player.localRotation, 15.0f * timeStep);

			// position
			Vector3 newPos = _player.position + Vector3.Scale (_player.right, _thirdPersonXOffsetVector)
				+ Vector3.Scale (_player.up, _thirdPersonYOffsetVector)
				+ Vector3.Scale (_cameraHolder.forward, _thirdPersonZOffsetVector);
			Vector3 newPosSlerp = Vector3.Slerp (_cameraHolder.position, newPos, 50.0f * timeStep);

			// prevent camera clipping through walls
			RaycastHit hit;
			if (Physics.Raycast (_player.transform.position, (newPos - _player.transform.position).normalized, out hit, Vector3.Distance(_player.transform.position, newPos), _ignoreMask)) 
			{
				if (hit.transform.gameObject.GetInstanceID () != _player.gameObject.GetInstanceID ())
					_cameraHolder.position = Vector3.Slerp (_cameraHolder.position, hit.point + hit.normal / 4.0f, 5.0f * timeStep);
			}
			else
				_cameraHolder.position = newPosSlerp;


			if (Mathf.Approximately (_cameraHolder.localRotation.eulerAngles.y, _player.localRotation.eulerAngles.y))
				_smoothingFreeLook = false;

			return;
		}

		// clamp camera
		newRotation.x = ApplyCameraLimits(newRotation.x, _thirdPersonLimits.x, _thirdPersonLimits.y, (mouseInput.y < 0.0f));
		// rotation
		_cameraHolder.localRotation = Quaternion.Slerp (_cameraHolder.localRotation, Quaternion.Euler (newRotation), 100.0f * timeStep);

		// position
		Vector3 newPos2 = _player.position + Vector3.Scale (_player.right, _thirdPersonXOffsetVector)
		             + Vector3.Scale (_player.up, _thirdPersonYOffsetVector)
		             + Vector3.Scale (_cameraHolder.forward, _thirdPersonZOffsetVector);


		// prevent camera clipping through walls
		RaycastHit hit2;
		if (Physics.Raycast (_player.transform.position, (newPos2 - _player.transform.position).normalized, out hit2, Vector3.Distance(_player.transform.position, newPos2), _ignoreMask)) 
		{
			if (hit2.transform.gameObject.GetInstanceID () != _player.gameObject.GetInstanceID ()) 
			{
				_cameraHolder.position = Vector3.Slerp (_cameraHolder.position, hit2.point + hit2.normal / 4.0f, 5.0f * timeStep);

				// see through player if too close to camera
				if (Vector3.Distance (_cameraHolder.position, _player.transform.position) < 1.0f)
					_actualGraphicsHolder.SetActive (false);
			}
		}
		else
		{
			_cameraHolder.position = Vector3.Slerp (_cameraHolder.position, newPos2, 15.0f * timeStep);
			_actualGraphicsHolder.SetActive (true);
		}
	}

	// moves and rotates first person camera depending on input
	private void UpdateFirstPersonCamera(Vector3 newRotation, Vector2 mouseInput, float timeStep)
	{
		// snap back to normal camera if just used free look
		if (Input.GetButtonUp ("ThirdPersonFreeLook") == true) 
			_smoothingFreeLook = true;

		if (_smoothingFreeLook) 
		{
			_cameraHolder.localRotation = Quaternion.Slerp(_cameraHolder.localRotation, _player.localRotation, 15.0f * timeStep);

			// position
			_cameraHolder.position = Vector3.Slerp (_player.position, _player.position + Vector3.Scale (_player.right, _firstPersonXOffsetVector)
				+ Vector3.Scale (_player.up, _firstPersonYOffsetVector)
				+ Vector3.Scale (_player.forward, _firstPersonZOffsetVector), 50.0f * timeStep);

			if (Mathf.Approximately (_cameraHolder.localRotation.eulerAngles.y, _player.localRotation.eulerAngles.y))
				_smoothingFreeLook = false;

			return;
		}

		if (_cameraHolder == null)
			return;

		// clamp camera
		newRotation.x = ApplyCameraLimits(newRotation.x, _firstPersonLimits.x, _firstPersonLimits.y, (mouseInput.y <= 0.0f));
		// update camera rotation
		_cameraHolder.localRotation = Quaternion.Euler(newRotation);

        if (_player == null)
            return;

		// update camera position
		_cameraHolder.position = _player.position + Vector3.Scale(_player.right, _firstPersonXOffsetVector) 
												+ Vector3.Scale(_player.up, _firstPersonYOffsetVector)
												+ Vector3.Scale(_player.forward, _firstPersonZOffsetVector);
	}

	//  rotates the graphics 
	private void UpdateGraphics(Vector3 newRotation, float timeStep)
	{
		if (_graphicsHolder == null)
			return;

		if (Input.GetButton ("ThirdPersonFreeLook") == true)
			return;

		// Rigidbody rotation for collision purposes
		// Smoothed for aesthetics
		//_graphicsHolder.MoveRotation (Quaternion.Slerp(_graphicsHolder.transform.rotation, Quaternion.Euler (0.0f, newRotation.y, 0.0f), 15.0f * timeStep));
		//_graphicsHolder.MoveRotation (Quaternion.Slerp(_graphicsHolder.transform.rotation, Quaternion.Euler (0.0f, newRotation.y, 0.0f), 15.0f * timeStep));
	}

	private void UpdateGunGraphics(float timeStep)
	{
		if (_gunHolder == null)
			return;

		// tilt further if leaning
		Vector3 leanRotation = Vector3.zero;
		if (Input.GetButton ("LeanRight")) 
			leanRotation.z = -15.0f;
		else if (Input.GetButton ("LeanLeft")) 
			leanRotation.z = 15.0f;

		// rotation
		_gunHolder.transform.localRotation = Quaternion.Slerp(_gunHolder.transform.localRotation,
			Quaternion.Euler (_mainCamera.transform.rotation.eulerAngles.x + leanRotation.x,
				_gunHolder.transform.localRotation.y + leanRotation.y,
				_gunHolder.transform.localRotation.z + leanRotation.z), 
			25.0f * timeStep);
	}

	// clamps camera, downCondition varies
	private float ApplyCameraLimits(float value, float min, float max, bool downCondition)
	{
		if (value < max && value > min) 
		{
			if (downCondition == true)
				return min;
			else
				return max;
		}

		return value;
	}

	#region Initialisation
	private void InitialiseCamera()
	{
		switch (_cameraType) 
		{
			default:
				Debug.LogError ("'_cameraType' isn't assigned! Defaulted to " + DEFAULT_CAMERA_TYPE.ToString()); 
				_cameraType = DEFAULT_CAMERA_TYPE; 
				goto case DEFAULT_CAMERA_TYPE;

			case CameraType.FirstPerson:
				InitialiseFirstPerson ();
				break;

			case CameraType.ThirdPerson:
				InitialiseThirdPerson ();
				break;

			case CameraType.Freelook:
				InitialiseFreelook ();
				break;
		}
	}

	private void InitialiseFirstPerson()
	{
		_PlayerMove.preventMovement = false;

		_cameraHolder.localRotation = _graphicsHolder.transform.localRotation;
		_cameraHolder.position = _player.position
			+ Vector3.Scale(_player.right, _firstPersonXOffsetVector)
			+ Vector3.Scale (_player.up, _firstPersonYOffsetVector)
			+ Vector3.Scale (_player.forward, _firstPersonZOffsetVector);

		_weaponCamera.fieldOfView = 75.0f;
		_weaponCamera.gameObject.SetActive (true);
        _mainCamera.cullingMask = _1stPersonMainCameraIgnore;
        _mainCamera.fieldOfView = 75.0f;

		_gunHolder.transform.localPosition = _firstPersonGunHolderPosition;
	}

	private void InitialiseThirdPerson()
	{
		_PlayerMove.preventMovement = false;

		_cameraHolder.localRotation = _graphicsHolder.transform.localRotation;

		_cameraHolder.position = _player.position 
			+ Vector3.Scale(_player.right, _thirdPersonXOffsetVector)
			+ Vector3.Scale (_player.up, _thirdPersonYOffsetVector)
			+ Vector3.Scale (_player.forward, _thirdPersonZOffsetVector); 
		
		_weaponCamera.fieldOfView = 65.0f;
        _weaponCamera.gameObject.SetActive (false);


        _mainCamera.cullingMask = _3rdPersonMainCameraIgnore;

		_mainCamera.fieldOfView = 65.0f;

		_gunHolder.transform.localPosition = _thirdPersonGunHolderPosition;
	}

	private void InitialiseFreelook()
	{
		_PlayerMove.preventMovement = true;

		_weaponCamera.fieldOfView = 65.0f;
		_mainCamera.fieldOfView = 65.0f;
	}
	#endregion
}
