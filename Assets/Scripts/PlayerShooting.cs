using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour {

	public ParticleSystem muzzleFlash;
	Animator anim;
	public GameObject impactPrefab;
	public GameObject bloodSplatPrefab;
	GameObject currentSplat;
	private float timeStamp ;
	//public GameObject bulletHole;

	//To show name when looked at
	GUIManager guiMan;
	Transform enemyTransform;
	bool showEnemyName = false; 
	string enemyName;
	//For Impact Holes and Impact Effects
	Queue<GameObject> impacts = new Queue<GameObject>();

	//GameObject[] impacts;
	NetworkManager NM;
	int currentImpact = 0;
	int maxImpacts = 20;
	bool shooting = false;
	float damage = 16f; 
	int clipSize = 30;
	bool reloading = false; 
	public Text ammoText;
	public Transform target;
	// Use this for initialization
	void Start () {

		guiMan = GameObject.Find ("NetworkManager").GetComponent<GUIManager> ();
		NM = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
	
		ammoText = GameObject.FindGameObjectWithTag ("Ammo").GetComponent<Text>();
		anim = GetComponentInChildren<Animator> ();
		timeStamp = 0; 

		ammoText.text = clipSize.ToString();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetButton ("Fire1") && !Input.GetKey (KeyCode.LeftShift) && timeStamp <= Time.time && clipSize >= 0) {
				
			muzzleFlash.Emit(1);
			clipSize--;
			ammoText.text = clipSize.ToString();
			anim.SetBool("Fire",true);
			shooting = true; 
			timeStamp = Time.time + 0.1f;
			NM.player.GetComponent<PhotonView>().RPC ("ShootingSound",PhotonTargets.All,true);
		}
		else{
			anim.SetBool("Fire",false);
		}

		if (Input.GetKeyDown (KeyCode.R) && !Input.GetButton ("Fire1") && clipSize < 30 && !reloading) {

			Debug.Log("Reloading");
			reloading = true; 
			NM.player.GetComponent<PhotonView>().RPC ("ReloadingSound",PhotonTargets.All);
			StartCoroutine(Reload());
		}
		if (clipSize <= 0 && Input.GetButtonDown ("Fire1")) {
			//StartCoroutine(EmptyGun());
			NM.player.GetComponent<PhotonView>().RPC ("OutOfAmmo",PhotonTargets.All);
		}

		if (shooting) {
			
			shooting = false; 
			
			RaycastHit hit;
			Debug.Log("Origin: " + transform.position + ", direction: " + transform.forward);
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0));;
			Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

			if(Physics.Raycast(ray, out hit, 50f)){
				//Get HitRotation on what we hit. 
				Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
				Debug.Log ("<color=red>Tag of Hit Object</color> " + hit.transform.tag + " " + hit.transform.name);
				if(hit.transform.tag == "Player" && hit.collider.tag != "FlyByRange"){

					//Play hitmarker sound
					gameObject.GetComponent<AudioSource>().Play();
					//If we hit the head colliderr change the damage
					if(hit.collider.tag == "Head"){

						Debug.Log ("<color=red>HeadShot!</color> " + hit.collider.name);
						damage = 100f; 
					}
					//If we hit the body change the damage
					if(hit.collider.tag == "Body"){

						damage = 16f;
					}
					Debug.Log ("<color=red>Collider Tag</color> " + hit.collider.tag);
					Instantiate (bloodSplatPrefab,hit.point, hitRotation);
					//Tell all we shot a player and call the RPC function GetShot passing damage runs on person shooting
					hit.transform.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, damage, PhotonNetwork.player); 
					Debug.Log ("<color=red>Target Health</color> " + hit.transform.GetComponent<PlayerNetworkMover>().GetHealth());
				}
				else{
					//For objects that are not players 
					// Push a new gameobject at pos and roatation of the object we hit thanks to ray hit
					//Dont want to see any decals on the collider for FlyByRange
					if(hit.collider.tag != "FlyByRange"){

						GameObject CurrentImpact = (GameObject)Instantiate (impactPrefab,hit.point, hitRotation);
						impacts.Enqueue(CurrentImpact);
						CurrentImpact.GetComponent<ParticleSystem>().Emit(1);

					}
				}
				if(impacts.Count >= maxImpacts){
					//Once we've pushed to our impact limit destroy the first gameobject we pushed. FIFO thanks to Queues
					Destroy(impacts.Dequeue());

				}

				if(hit.collider.tag == "FlyByRange"){
					hit.transform.GetComponent<PhotonView>().RPC ("PlayFlyByShots", PhotonTargets.Others); 
				}
			}
		}
	}
	
	IEnumerator Reload(){

		yield return new WaitForSeconds(2.0f);
		clipSize = 30;
		ammoText.text = clipSize.ToString();
		reloading = false; 
	}

	IEnumerator EmptyGun(){

		yield return new WaitForSeconds(1.0f);
		NM.player.GetComponent<PhotonView>().RPC ("OutOfAmmo",PhotonTargets.All);
	}
	
	void FixedUpdate(){
		//For Physic related stuff like adding force to rigidbody apprently 

	}

	void OnDrawGizmosSelected() {

		if (target != null) {
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, target.position);
		}
	}

	void OnGUI(){

		if(showEnemyName)
			guiMan.EnemyName (enemyTransform, enemyName);
	}
	
}
