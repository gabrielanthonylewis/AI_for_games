using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gabriel Lewis
// Q5094111

// Shooting functionallity, including reloading, recoil and bullet holes
public class PlayerShoot : MonoBehaviour 
{
	[SerializeField]
	private LayerMask _layerMask; // things that can be shot at

	[SerializeField]
	private ParticleSystem _particleSystem = null;

	[SerializeField]
	private GameObject _bulletHolePrefab = null;

	[SerializeField]
	private AudioClip _metalAudioClip = null; // sound for hitting barricades

	[SerializeField]
	private TextMeshProUGUI _ammoText = null; // ammo UI reference

	[SerializeField]
	private int _damage = 1;

	[SerializeField]
	private AudioSource _AudioSource = null; // to play the sounds (cached as an optimisation)

	[SerializeField]
	private AudioClip _shootSound = null; // sound of shooting

	[SerializeField]
	private AudioClip _shootReload = null; // sound of reloading

	[SerializeField]
	private Transform _gunHolder = null; // to add recoil to the gun itself

	[SerializeField]
	private Transform _cameraHolder = null; // to add vertical recoil to the camera

	[SerializeField]
	private Transform _barrel = null; // where the bullet is shot from

	public bool shooting { get { return _shootRou; } }

	// weapon specs, based on the real weapon
	private const int _RPM = 775;
	private const int _MAG_SIZE = 30;
	private const float _MAX_RANGE = 1000.0f; // note that effective range is 300 or 600 with LDS scopes


	private int _currentMag = 0;

	private float _waitTime = 0.0f;

	// so that only one instance of each coroutine is running at 
	// any one moment in time
	private bool _shootRou = false;
	private bool _reloadRou = false;

	[SerializeField]
	private Vector3 _recoilVector = Vector3.zero; 
	private float _recoilZ = 0.0f;

	[SerializeField]
	private bool _useBarrel = false;

	[SerializeField]
	private Light _shootFlash = null;


	void Start()
	{
		// initialisation
		_currentMag = _MAG_SIZE;
		_ammoText.text = _currentMag + "/" + _MAG_SIZE;
		_waitTime = 1.0f / (_RPM / 60.0f);
	}

	void Update ()
	{
		if (Time.timeScale == 0.0f)
			return;

		// apply recoil to the gun and camera
		_gunHolder.rotation *= Quaternion.Euler (_recoilVector * 10.0f);
		_gunHolder.position += _gunHolder.transform.forward * _recoilZ;
		_cameraHolder.rotation *= Quaternion.Euler (new Vector3(-_recoilVector.y, 0.0f, 0.0f));

		// reload input
		if (_currentMag <_MAG_SIZE && Input.GetButtonDown ("Reload")) 
		{
			_recoilVector = Vector3.zero;
			_recoilZ = 0.0f;
			StartCoroutine ("Reload");
			_particleSystem.Stop ();
		}

		// dont progress if already shooting/reloading
		if (_shootRou || _reloadRou) 
			return;
		
		// shoot input
		if (Input.GetButtonDown ("Shoot") || Input.GetAxis ("Shoot") == 1.0f) 
		{
			// auto reload if mag empty
			if (_currentMag == 0) 
			{
				_recoilVector = Vector3.zero;
				_recoilZ = 0.0f;
				StartCoroutine ("Reload");
				_particleSystem.Stop ();
				return;
			}

			// shoot and show effects
			_particleSystem.Play ();
			StartCoroutine ("Shoot");
		} 
		else
		{
			// reduce recoil to 0
			_recoilVector = Vector3.Lerp (_recoilVector, Vector3.zero, 20.0f * Time.deltaTime);
			_recoilZ = Mathf.Lerp (_recoilZ, 0.0f, 20.0f * Time.deltaTime);

			// stop effects
			_particleSystem.Stop ();
		}
	}

	private IEnumerator Reload()
	{
		if (_reloadRou == true)
			yield break;
		
		_reloadRou = true;

		// play sound
		_AudioSource.PlayOneShot (_shootReload);

		// reload time
		yield return new WaitForSeconds (0.4f);

		// refill mag
		_currentMag = _MAG_SIZE;

		// update text
		_ammoText.text = _currentMag + "/" + _MAG_SIZE;
	
		_reloadRou = false;
	}
		
	private IEnumerator Shoot()
	{
		if (_reloadRou == true)
			yield break;
		
		_shootRou = true;

		RaycastHit hit;

		// where to shoot from (camera or barrel)
		Transform shootPoint = Camera.main.transform;
		if (_useBarrel)
			shootPoint = _barrel;

		// shoot a bullet if there is something to hit
		if (Physics.Raycast (shootPoint.position, shootPoint.forward + new Vector3(_recoilVector.x, 0.0f, 0.0f), out hit, _MAX_RANGE, _layerMask)) 
		{
			// spawn in bullet hole on surface
			GameObject bulletHole = Instantiate (_bulletHolePrefab) as GameObject;
			bulletHole.transform.position = hit.point;
			bulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			bulletHole.transform.SetParent (hit.transform);
			bulletHole.transform.position -= bulletHole.transform.forward * 0.01f;

			// play metal sound if a barricade is hit
			if (hit.transform.tag == "Barricade")
				bulletHole.GetComponent<AudioSource> ().clip = _metalAudioClip;

			// play sound
			bulletHole.GetComponent<AudioSource> ().Play ();

			// apply damage to barricade (if Barricade hit)
			if (hit.transform.gameObject.GetComponent<ObjectHealth> ()) 
				hit.transform.gameObject.GetComponent<ObjectHealth> ().TakeDamage (_damage);

			// apply damage to AI (if AI hit)
			if (hit.transform.gameObject.GetComponent<AIHealth> ()) 
				hit.transform.gameObject.GetComponent<AIHealth> ().Wound (_damage);		
		}

		// calculate a randomised recoil vector
		_recoilVector.x = Mathf.Clamp (_recoilVector.x + Random.Range(-0.1f, 0.1f), -0.1f, 0.1f);
		_recoilVector.y = Mathf.Clamp (_recoilVector.y + Random.Range(0.0f, 0.02f), 0.0f, 0.075f);
		_recoilZ = Random.Range(-0.1f, -0.2f);

		// recoil minised if aiming
		if (Input.GetButton ("Aim")) 
		{
			_recoilVector.x = Mathf.Clamp (_recoilVector.x, -0.02f, 0.02f);
			_recoilVector.y = Mathf.Clamp (_recoilVector.y, 0.0f, 0.01f);
			_recoilZ = Mathf.Clamp (_recoilZ, 0.05f, 0.1f); 
		}

		// reduce ammo
		_currentMag--;

		// update text
		_ammoText.text = _currentMag + "/" + _MAG_SIZE;

		_AudioSource.PlayOneShot (_shootSound);

		_shootFlash.enabled = true;

		// after the shot is ejected the gun can shoot again
		yield return new WaitForSeconds (_waitTime);

		_shootFlash.enabled = false;
		_shootRou = false;
	}
}
