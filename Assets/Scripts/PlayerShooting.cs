using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour {

	public ParticleSystem muzzleFlash;
	Animator anim;
	public GameObject impactPrefab;
	private float timeStamp ;
	//public GameObject bulletHole;

	//To show name when looked at
	GUIManager guiMan;
	Transform enemyTransform;
	bool showEnemyName = false; 
	string enemyName;
	//For Impact Holes and Impact Effects
	GameObject[] impacts;
	GameObject[] impactHole;
	NetworkManager NM;
	int currentImpact = 0;
	int maxImpacts = 5;
	bool shooting = false;
	float damage = 16f; 
	int clipSize = 30;
	bool reloading = false; 
	public Text ammoText;
	public Transform target;
	private Vector3 start;
	private Vector3 line;
	// Use this for initialization
	void Start () {

		start = transform.position;
		line = transform.position;
		guiMan = GameObject.Find ("NetworkManager").GetComponent<GUIManager> ();
		NM = GameObject.Find ("NetworkManager").GetComponent<NetworkManager> ();
		impacts = new GameObject[maxImpacts];
		ammoText = GameObject.FindGameObjectWithTag ("Ammo").GetComponent<Text>();
		for (int i = 0; i < maxImpacts; i++){
			impacts [i] = (GameObject)Instantiate (impactPrefab);
			//impactHole[i] = (GameObject)Instantiate(bulletHole);
		}
		
		for(int i = 0; i < maxImpacts; i++)
			impacts[i].transform.parent = gameObject.transform;

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
				
				Debug.Log ("<color=red>Tag of Hit Object</color> " + hit.transform.tag + " " + hit.transform.name);
				if(hit.transform.tag == "Player"){
					
					if(hit.collider.tag == "Head"){
						Debug.Log ("<color=red>HeadShot!</color> " + hit.collider.name);
						damage = 100f; 
					}
					if(hit.collider.tag == "Body"){
						damage = 16f;
					}
					Debug.Log ("<color=red>Collider Tag</color> " + hit.collider.tag);
					
					//Tell all we shot a player and call the RPC function GetShot passing damage runs on person shooting
					hit.transform.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, damage, PhotonNetwork.player); 
					Debug.Log ("<color=red>Target Health</color> " + hit.transform.GetComponent<PlayerNetworkMover>().GetHealth());
				}
				
				impacts[currentImpact].transform.position = hit.point;
				line = hit.point;
				impacts[currentImpact].GetComponent<ParticleSystem>().Emit(1);
				//impactHole[currentImpact].transform.position = hit.point;
				//impacts[currentImpact].GetComponent<ParticleSystem>().Emit(1);
				if(++currentImpact >= maxImpacts){
					currentImpact = 0; 
				}
				if(currentImpact >= maxImpacts){
					
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
