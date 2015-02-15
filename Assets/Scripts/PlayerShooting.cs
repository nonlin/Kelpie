using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
	float damage = 8.5f; 
	int clipSize = 30;
	bool reloading = false; 
	public Text ammoText;
	public Transform target;

	// Use this for initialization
	void Start () {


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

		Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
		Debug.DrawRay(transform.position, forward, Color.green);


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

		if (shooting) {
		
			shooting = false; 

			RaycastHit hit;
			if(Physics.Raycast(transform.position, transform.forward, out hit, 50f)){

				Debug.Log ("<color=red>Tag of Hit Object</color> " + hit.transform.tag + " " + hit.transform.name);
				if(hit.transform.tag == "Player"){

					//Tell all we shot a player and call the RPC function GetShot passing damage runs on person shooting
					hit.transform.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, damage, PhotonNetwork.player); 
					Debug.Log ("<color=red>Target Health</color> " + hit.transform.GetComponent<PlayerNetworkMover>().GetHealth());
				}
				
				impacts[currentImpact].transform.position = hit.point;
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

	void OnDrawGizmosSelected() {

		if (target != null) {
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, target.position);
		}
	}
	
}
