using UnityEngine;
using System.Collections;

public class Ammo : MonoBehaviour {

	public AudioClip pickupSound;//sound to play when picking up weapon
	//public AudioClip fullSound;//sound to play when ammo is full
	// Use this for initialization
	void Start () {

		audio.clip = pickupSound;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void OnPickUp(){

		//ammoSource.clip = pickupSound;
		AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.75f);
		//if(Time.deltaTime + pickupSound.length <= Time.deltaTime)
			PhotonNetwork.Destroy (gameObject);
	}
	void OnDestroy() {

	}
}
