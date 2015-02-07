using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNetworkMover : Photon.MonoBehaviour {
	//use events and delegates to know when someone has died, Secure with events
	public delegate void Respawn(float time);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string message);
	public event SendMessage SendNetworkMessage;

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
	[SerializeField] private AudioClip _jumpSound; // the sound played when character leaves the ground.
	[SerializeField] private AudioClip _landSound; // the sound played when character touches back on ground.
	[SerializeField] private AudioClip[] _footstepSounds;
	private CharacterController _characterController;
	private float _stepCycle = 0f;
	private float _nextStep = 0f;
	//CharacterController cc;
	AudioSource audio0;
	AudioSource audio1;
	AudioSource[] aSources;
	Animator anim;
	ColliderControl colidcon;
	bool alive = true;

	//AudioSource audio;
	// Use this for initialization
	void Start () {

		colidcon = gameObject.GetComponent<ColliderControl> ();
		PhotonNetwork.isMessageQueueRunning = true;
		//Disables my Character Controller interstingly enough. That way I can only enable it for the clien'ts player.  
		transform.GetComponent<Collider>().enabled = false;
		//Use this to get current player this script is attached too
		aSources = GetComponents<AudioSource> (); 
		audio0 = aSources [0];
		audio1 = aSources [1];
		//cc.enabled = false;
		anim = GetComponentInChildren<Animator> ();
		//audio = GetComponentInChildren<AudioSource> ();
		//If its my player, not anothers
		if (photonView.isMine) {

			//Enable CC so we can control character. 
			transform.GetComponent<Collider>().enabled = true;
			//Use for Sound toggle
			_characterController = GetComponent<CharacterController>();
			//gameObject.name = PhotonNetwork.player.name;
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
			weapons = GameObject.FindGameObjectsWithTag("AK");
			for(int i = 0; i < weapons.Length; i++){
				//If the weapon we find has the same ID as the player its attached to, set the tag to layer 10
				if(weapons[i].GetComponentInParent<PlayerNetworkMover>().gameObject.GetInstanceID() == gameObject.GetInstanceID() )
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

			//stream.SendNext(deaths);

		
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
			
		}
																												
	}

	public float GetHealth(){

		return health;
	}

	[RPC]
	public void GetShot(float damage, PhotonPlayer enemy){
		//Take Damage and check for death
		 
		health -= damage;
		if(health <=0 && alive){

			alive = false; 
			Debug.Log ("<color=blue>Checking Health</color>" + health);
			if (photonView.isMine) {

				if(SendNetworkMessage != null){
					SendNetworkMessage(PhotonNetwork.player.name + " got owned by " + enemy.name);
				}
				//Subscribe to the event so that when a player dies 3 sec later respawn
				if(RespawnMe != null)
					RespawnMe(3f);
				//Only owner can remove themselves
				//Create deaths equal to stored hashtable deaths, increment, Set
				int totalDeaths = (int)PhotonNetwork.player.customProperties["D"];
				totalDeaths ++;
				ExitGames.Client.Photon.Hashtable setPlayerDeaths = new ExitGames.Client.Photon.Hashtable() {{"D", totalDeaths}};
				PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
				//Destroy Object on network
				Debug.Log ("<color=green> Collider State After</color>"+transform.GetComponent<Collider>().enabled.ToString());
				PhotonNetwork.Destroy(gameObject);

			}
			else{

				foreach(PhotonPlayer p in PhotonNetwork.playerList)
					if(PhotonNetwork.player == enemy){

						int totalKIlls = (int)PhotonNetwork.player.customProperties["K"];
						totalKIlls ++;
						ExitGames.Client.Photon.Hashtable setPlayerKills = new ExitGames.Client.Photon.Hashtable() {{"K", totalKIlls}};
						Debug.Log ("<color=red>KillCounter Called at </color>" + totalKIlls);
						PhotonNetwork.player.SetCustomProperties(setPlayerKills);
						break; 
					}
			}
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

	[RPC]
	public void PlayLandingSound()
	{
		audio.clip = _landSound;
		audio.Play();
		_nextStep = _stepCycle + .5f;
		
	}
	
	[RPC]
	public void PlayJumpSound()
	{
		audio.clip = _jumpSound;
		audio.Play();
	}
	

	// Update is called once per frame
	void Update () {
	
	}
}
