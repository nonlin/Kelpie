using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

	public ParticleSystem muzzleFlash;
	Animator anim;
	public GameObject impactPrefab;
	private float timeStamp ;
	//public GameObject bulletHole;

	GameObject[] impacts;
	GameObject[] impactHole;
	NetworkManager NM;
	int currentImpact = 0;
	int maxImpacts = 5;
	bool shooting = false;
	float damage = 25f; 

	// Use this for initialization
	void Start () {
		NM = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
		impacts = new GameObject[maxImpacts];
		for (int i = 0; i < maxImpacts; i++){
			impacts [i] = (GameObject)Instantiate (impactPrefab);
			//impactHole[i] = (GameObject)Instantiate(bulletHole);
		}

		anim = GetComponentInChildren<Animator> ();
		timeStamp = 0; 
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetButton ("Fire1") && !Input.GetKey (KeyCode.LeftShift) && timeStamp <= Time.time) {
				
			muzzleFlash.Emit(1);
			//anim.Play("Fire");
			//anim.SetTrigger("Fire");
			anim.SetBool("Fire",true);
			shooting = true; 
			timeStamp = Time.time + 0.1f;
			NM.player.GetComponent<PhotonView>().RPC ("ShootingSound",PhotonTargets.All,true);
		}
		else{
			anim.SetBool("Fire",false);

		}


	}

	void FixedUpdate(){

		if (shooting) {
		
			shooting = false; 

			RaycastHit hit;
			if(Physics.Raycast(transform.position, transform.forward, out hit, 50f)){

				if(hit.transform.tag == "Player"){

					//Tell all we shot a player and call the RPC function GetShot passing damage runs on person shooting
					hit.transform.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, damage, PhotonNetwork.player); 
					Debug.Log ("<color=red>Target Health</color> " + hit.transform.GetComponent<PlayerNetworkMover>().GetHealth());
				}
				else

				impacts[currentImpact].transform.position = hit.point;
				impacts[currentImpact].GetComponent<ParticleSystem>().Emit(1);
				//impactHole[currentImpact].transform.position = hit.point;
				//impacts[currentImpact].GetComponent<ParticleSystem>().Emit(1);
				if(++currentImpact >= maxImpacts){
					currentImpact = 0; 
				}
			}
		}
	}


}
