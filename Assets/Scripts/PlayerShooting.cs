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
	static List<GameObject> impacts = new List<GameObject>();
	List<GameObject>.Enumerator e;
	GameObject CurrentImpact;
	//GameObject[] impacts;
	NetworkManager NM;
	int currentImpact = 0;
	int maxImpacts = 20;
	bool shooting = false;
	float damage = 16f; 
	int clipSize = 30;
	public int clipAmount = 3;
	bool reloading = false; 
	public Text ammoText;
	public Transform target;
	bool enumDeclared = false;
	// Use this for initialization
	void Start () {

		guiMan = GameObject.Find ("NetworkManager").GetComponent<GUIManager> ();
		NM = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
	
		ammoText = GameObject.FindGameObjectWithTag ("Ammo").GetComponent<Text>();
		anim = GetComponentInChildren<Animator> ();
		timeStamp = 0; 


	}
	
	// Update is called once per frame
	void Update () {
		//Always update this text
		ammoText.text = clipAmount.ToString() + "/" +clipSize.ToString();

		if (Input.GetButton ("Fire1") && !Input.GetKey (KeyCode.LeftShift) && timeStamp <= Time.time && clipSize >= 0) {
				
			muzzleFlash.Emit(1);
			clipSize--;
			ammoText.text = clipAmount.ToString() + "/" +clipSize.ToString();
			anim.SetBool("Fire",true);
			shooting = true; 
			timeStamp = Time.time + 0.1f;
			NM.player.GetComponent<PhotonView>().RPC ("ShootingSound",PhotonTargets.All,true);
		}
		else{
			anim.SetBool("Fire",false);
		}

		if (Input.GetKeyDown (KeyCode.R) && !Input.GetButton ("Fire1") && clipSize < 30 && clipAmount != 0 && !reloading) {

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
			
			RaycastHit[] hits;
			bool flyByTrue = true;
			hits = Physics.RaycastAll(transform.position, transform.forward, 100.0F);
			RaycastHit temphit;
			Debug.Log("Origin: " + transform.position + ", direction: " + transform.forward);
			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0));;
			Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
			foreach(RaycastHit hit in hits){
			//if(Physics.Raycast(ray, out hit, 50f)){
				//Get HitRotation on what we hit. 
				Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
				Debug.Log ("<color=red>Tag of Hit Object</color> " + hit.transform.tag + " " + hit.transform.name + " " + hits.Length);
				if(hit.collider.tag == "Body"){
					flyByTrue = false;
					//Play hitmarker sound
					gameObject.GetComponent<AudioSource>().Play();
					//If we hit the head colliderr change the damage
					if(hit.collider.name == "Head"){

						Debug.Log ("<color=red>HeadShot!</color> " + hit.collider.name);
						damage = 100f; 
					}
					//If we hit the body change the damage
					if(hit.collider.name == "Torso"){

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
					if(hit.collider.tag != "FlyByRange" && impacts.Count < maxImpacts){

						CurrentImpact = (GameObject)Instantiate (impactPrefab,hit.point, hitRotation);
						impacts.Add(CurrentImpact);
						CurrentImpact.GetComponent<ParticleSystem>().Emit(1);

					}
				}
				//Just need to set the Enum once after its set, we can't call it again until we are ready to reset again to loop back.
				if(impacts.Count >= maxImpacts && !enumDeclared){
					enumDeclared = true;
					e = impacts.GetEnumerator();
				}
				//But now we still need know when to iterate through the list of impacts
				if(impacts.Count >= maxImpacts && hit.collider.tag != "FlyByRange" && hit.collider.tag != "Body"){

					if(e.MoveNext()){
						//This is why we bothered to use enum. Now we don't have to create and destroy, instead we interate through the list
						//to move the already created impact to a new impact point. 
						CurrentImpact = e.Current;
						CurrentImpact.transform.position = hit.point;
						CurrentImpact.transform.rotation = hitRotation;
					}
					else{
						//Reset
						e = impacts.GetEnumerator();
					}

				}
				temphit = hit;
	

			}
			if(temphit.collider.tag == "FlyByRange" && flyByTrue){
				Debug.Log ("<color=green>FlyRange Sound</color>");
				temphit.transform.GetComponent<PhotonView>().RPC ("PlayFlyByShots", PhotonTargets.Others); 
			}
		//}
		}
	}
	
	IEnumerator Reload(){

		yield return new WaitForSeconds(2.0f);
		clipSize = 30;
		clipAmount--;
		ammoText.text = clipAmount.ToString() + "/" + clipSize.ToString();
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
