using UnityEngine;
using System.Collections;

public class PlayerNetworkMover : Photon.MonoBehaviour {
	//use events and delegates to know when someone has died, Secure with events
	public delegate void Respawn(float time);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string message);
	public event SendMessage SendNetworkMessage;
	public delegate void Score(string playerName);
	public event Score ScoreStats;

	Vector3 position;
	Quaternion rotation;
	float smoothing = 10f;
	float health = 100f;
	public string playerName; 
	public int kills = 0;
	public int deaths = 0; 
	GameObject[] weapons;
	bool aim = false;
	bool sprint = false;
	bool initialLoad = true;
	bool isShooting = false; 
	public AudioClip AKFire;
	//CharacterController cc;
	AudioSource audio0;
	AudioSource audio1;
	AudioSource[] aSources;
	Animator anim;

	//AudioSource audio;
	// Use this for initialization
	void Start () {
		//cc = GetComponent<CharacterController>();
		aSources = GetComponents<AudioSource> (); 
		audio0 = aSources [0];
		audio1 = aSources [1];
		//cc.enabled = false;
		anim = GetComponentInChildren<Animator> ();
		//audio = GetComponentInChildren<AudioSource> ();
		//If its my player, not anothers
		if (photonView.isMine) {
			gameObject.name = PhotonNetwork.player.name;
			playerName = PhotonNetwork.player.name;
			//enable each script just for the player being spawned and not the others
			rigidbody.useGravity = true; 
			GetComponent<UnitySampleAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
			GetComponentInChildren<PlayerShooting>().enabled = true;
			foreach(Camera cam in GetComponentsInChildren<Camera>()){
				cam.enabled = true; 
			}
			foreach(AudioListener AL in GetComponentsInChildren<AudioListener>()){
				AL.enabled = true; 
			}
			//GetComponent<UnitySampleAssets.Characters.FirstPerson.FirstPersonController>().isDead = false; 
			//foreach(CharacterController CharCon in GetComponentsInChildren<CharacterController>()){
			//	CharCon.enabled = true; 
			//}
			//transform.Find ("FirstPersonCharacter/WeaponsCam/Ak-47").gameObject.layer = 10;
			weapons = GameObject.FindGameObjectsWithTag("AK");
			for(int i = 0; i < weapons.Length; i++){
				weapons[i].layer = 10; 
			}
		}
		else{
			StartCoroutine ("UpdateData");
		}
	}

	IEnumerator UpdateData(){

		if (initialLoad) {
			//jiiter correction incomplete, could check position if accurate to .0001 don't move them 
			initialLoad = false; 
			transform.position = position; 
			transform.rotation = rotation; 
		}
		while (true) {
			//smooths every frame for the dummy players from where they are to where they should be, prevents jitter lose some accuracy I suppose
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
			//Sync Animation States
			anim.SetBool ("Aim", aim); 
			anim.SetBool ("Sprint", sprint); 
			yield return null; 
		}
	}
	//Serilize Data Across the network, we want everyone to know where they are
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){

		if (stream.isWriting) {
			//send to clients where we are
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(health); 
			//Sync Animation States
			stream.SendNext(anim.GetBool ("Aim"));
			stream.SendNext(anim.GetBool ("Sprint"));
			//stream.SendNext(isShooting);
		}
		else{
			//Get from clients where they are
			//Write in teh same order we read, if not writing we are reading. 
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			health = (float)stream.ReceiveNext();
			//Sync Animation States
			aim = (bool)stream.ReceiveNext();
			sprint = (bool)stream.ReceiveNext();
			//isShooting = (bool)stream.ReceiveNext();
		}

	}
	[RPC]
	public void GetShot(float damage, string enemyName){

		health -= damage;
		if (health <= 0 && photonView.isMine) {

			if(SendNetworkMessage != null){
				SendNetworkMessage(PhotonNetwork.player.name + " got owned by " + enemyName);
			}
			//Subscribe to the event so that when a player dies 3 sec later respawn
			if(RespawnMe != null)
				RespawnMe(3f);
			if(ScoreStats != null)
				ScoreStats(PhotonNetwork.player.name);
			//Only owner can remove themselves
			PhotonNetwork.Destroy(gameObject);
		}
	}
	[RPC]
	public void ShootingSound(bool firing){

		if (firing) {
		
			isShooting = true; 
			audio1.clip = AKFire;
			audio1.Play();
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
