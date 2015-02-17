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

	GameObject[] weapons;
	GameObject[] bodys;
	//public GameObject injuryEffect;
	//Animator injuryAnim;

	bool aim = false;
	bool sprint = false;
	bool onGround = true;
	float Forward = 0f;
	float turn = 0f;
	bool initialLoad = true;

	public AudioClip AKFire;
	public AudioClip AKReload;
	public AudioClip AKEmpty;
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
	Animator animEthan;
	PhotonView photonView;
	//ColliderControl colidcon;
	[SerializeField] bool alive;

	//AudioSource audio;
	// Use this for initialization
	void Start () {

		alive = true; 
		photonView = GetComponent<PhotonView> ();
		//Disables my Character Controller interstingly enough. That way I can only enable it for the clien'ts player.  
		transform.GetComponent<Collider>().enabled = false;
		//Use this to get current player this script is attached too
		aSources = GetComponents<AudioSource> (); 
		audio0 = aSources [0];
		audio1 = aSources [1];
		anim = GetComponentInChildren<Animator> ();
		animEthan = transform.Find("char_ethan").GetComponent<Animator> ();
		//injuryAnim = GameObject.Find ("InjuryEffect").GetComponent<Animator>();
		//injuryAnim = injuryEffect.GetComponent<Animator> ();

		//audio = GetComponentInChildren<AudioSource> ();
		//If its my player, not anothers
		Debug.Log ("<color=red>Joined Room </color>" + PhotonNetwork.player.name + " " + photonView.isMine);
		if (photonView.isMine) {

			//Enable CC so we can control character. 
			transform.GetComponent<Collider>().enabled = true;
			//Use for Sound toggle
			_characterController = GetComponent<CharacterController>();
	
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
			/*bodys = GameObject.FindGameObjectsWithTag("Body");
			for(int i = 0; i < bodys.Length; i++){
				if(bodys[i].GetComponentInParent<PlayerNetworkMover>().gameObject.GetInstanceID() == gameObject.GetInstanceID() )
					bodys[i].layer = 11; 
			}*/

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
			animEthan.SetBool("OnGround",onGround);
			animEthan.SetFloat("Forward",Forward);
			animEthan.SetFloat("Turn",turn);
			yield return null; 
		}
	}
	//Serilize Data Across the network, we want everyone to know where they are
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){

		if (stream.isWriting) {
			//send to clients where we are
			stream.SendNext(playerName);
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(health); 
			//Sync Animation States
			stream.SendNext(anim.GetBool ("Aim"));
			stream.SendNext(anim.GetBool ("Sprint"));
			stream.SendNext(animEthan.GetBool("OnGround"));
			stream.SendNext(animEthan.GetFloat("Forward"));
			stream.SendNext(animEthan.GetFloat("Turn"));
			//
			stream.SendNext(alive);
		
		}
		else{
			//Get from clients where they are
			//Write in teh same order we read, if not writing we are reading. 
			playerName = (string)stream.ReceiveNext();
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			health = (float)stream.ReceiveNext();
			//Sync Animation States
			aim = (bool)stream.ReceiveNext();
			sprint = (bool)stream.ReceiveNext();
			onGround = (bool)stream.ReceiveNext();
			Forward = (float)stream.ReceiveNext();
			turn = (float)stream.ReceiveNext();
			//
			alive = (bool)stream.ReceiveNext();
			
		}
																												
	}

	public float GetHealth(){

		return health;
	}

	[RPC]
	public void GetShot(float damage, PhotonPlayer enemy){
		//Take Damage and check for death
		
		health -= damage;
	
		Debug.Log ("<color=green>Got Shot with </color>" + damage + " damage. Is alive: " + alive + " PhotonView is" + photonView.isMine);
		if(health <=0 && alive){
			
			alive = false; 
			Debug.Log ("<color=blue>Checking Health</color>" + health + " Photon State " + photonView.isMine + " Player Name " + PhotonNetwork.player.name);
			if (photonView.isMine) {
				Debug.Log ("<color=red>Death</color>");
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
				foreach(PhotonPlayer p in PhotonNetwork.playerList)
					Debug.Log ("<color=red>PlayerLIst</color>" + p.name);
			}
			else{
				
				if(PhotonNetwork.player == enemy){
					
					int totalKIlls = (int)PhotonNetwork.player.customProperties["K"];
					totalKIlls ++;
					ExitGames.Client.Photon.Hashtable setPlayerKills = new ExitGames.Client.Photon.Hashtable() {{"K", totalKIlls}};
					Debug.Log ("<color=red>KillCounter Called at </color>" + totalKIlls);
					PhotonNetwork.player.SetCustomProperties(setPlayerKills);
					
				}
			}
		}
		//injuryEffect.SetActive (true);
		//injuryAnim.SetBool ("Hit", true);
		StartCoroutine( WaitForAnimation (2.0f));
	}


	[RPC]
	public void ShootingSound(bool firing){
		
		if (firing) {
			
				audio1.clip = AKFire;
				audio1.Play();
			}
		}

	[RPC]
	public void ReloadingSound(){
		
		//if (firing) {
			
			//isShooting = true; 
			audio1.clip = AKReload;
			audio1.Play();
		//}
	}

	[RPC]
	public void OutOfAmmo(){
		
		//if (firing) {
		
		//isShooting = true; 
		audio1.clip = AKEmpty;
		audio1.Play();
		//}
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

	
	[RPC]
	public void PlayFootStepAudio()
	{
		//if (!_characterController.isGrounded) return;
		// pick & play a random footstep sound from the array,
		// excluding sound at index 0
		int n = Random.Range(1, _footstepSounds.Length);
		audio.clip = _footstepSounds[n];
		audio.PlayOneShot(audio.clip);
		// move picked sound to index 0 so it's not picked next time
		_footstepSounds[n] = _footstepSounds[0];
		_footstepSounds[0] = audio.clip;
	}

	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.K)){

			//health = 0;
			gameObject.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, 25f, PhotonNetwork.player);
			Debug.Log (health);
		}
	}

	private IEnumerator WaitForAnimation ( float waitTime )
	{
		yield return new WaitForSeconds(waitTime);
		//injuryAnim.SetBool ("Hit", false);
		//injuryEffect.SetActive (false);
	}
}
