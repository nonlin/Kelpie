using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class NetworkManager : MonoBehaviour {

	[SerializeField] Text connectionText;
	[SerializeField] Transform[] spawnPoints;
	[SerializeField] Camera sceneCamera;

	[SerializeField] GameObject lobbyWindow;
	[SerializeField] InputField userName;
	[SerializeField] InputField roomName;
	[SerializeField] InputField roomList;
	[SerializeField] InputField messageWindow;
	[SerializeField] Text textKills;
	[SerializeField] Text textDeaths;

	public GameObject player;
	Queue<string> messages;
	const int messageCount = 7;
	PhotonView photonView;
	public bool spawning = false; 


	ExitGames.Client.Photon.Hashtable setPlayerKills = new ExitGames.Client.Photon.Hashtable() {{"K", 0}};
	ExitGames.Client.Photon.Hashtable setPlayerDeaths = new ExitGames.Client.Photon.Hashtable() {{"D", 0}};
	//ExitGames.Client.Photon.Hashtable setPlayerHealth= new ExitGames.Client.Photon.Hashtable() {{"H", 100}};
	// Use this for initialization
	void Start () {
	
		photonView = GetComponent<PhotonView> ();//Initillze PhotonView
		messages = new Queue<string> (messageCount);//Specify Size for garbage Collection 
		PhotonNetwork.sendRate = 30;
		PhotonNetwork.sendRateOnSerialize = 15;
		PhotonNetwork.logLevel = PhotonLogLevel.Full;//So we see everything in output
		//connect to Server with setup info and sets game version
		PhotonNetwork.ConnectUsingSettings ("0.4");
		StartCoroutine("UpdateConnectionString");
		PhotonNetwork.player.SetCustomProperties(setPlayerKills);
		PhotonNetwork.player.SetCustomProperties(setPlayerDeaths);
		//PhotonNetwork.player.SetCustomProperties(setPlayerHealth);
		//Game Managing Stuff

	}
	void Update(){

		if (Input.GetKey (KeyCode.Escape)) {

			Screen.lockCursor = false;
		}

	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){


	}

	// Update is called once per frame
	IEnumerator UpdateConnectionString () {

		while(true){
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null; 
		}

	}


	void OnJoinedLobby(){

		lobbyWindow.SetActive (true);
	}

	void OnReceivedRoomListUpdate(){

		roomList.text = "";
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		foreach(RoomInfo room in rooms)
			roomList.text += room.name + "\n";
	}

	public void JoinRoom(){

		PhotonNetwork.player.name = userName.text;
		RoomOptions rm = new RoomOptions (){isVisible = true, maxPlayers = 10};
		//Create a room called lobby, with rm settings using default lobby type
		PhotonNetwork.JoinOrCreateRoom (roomName.text, rm, TypedLobby.Default);

	}

	void OnJoinedRoom(){

		lobbyWindow.SetActive (false);
		StopCoroutine ("UpdateConnectionString");
		connectionText.text = "";
		StartSpawnProcess (0f);
		AddMessage ("Player " + PhotonNetwork.player.name + " has joined.");
		Screen.showCursor = false;
		Screen.lockCursor = true;

	}

	void StartSpawnProcess (float respawnTime){
		//Show Lobby cam on death vs blank screen
		sceneCamera.enabled = true; 
		StartCoroutine ("SpawnPlayer", respawnTime);
		AddMessage ("Player " + PhotonNetwork.player.name + " has spawned.");
		//playerStats.Add(new Player(PhotonNetwork.player.name, 0, 0)); 

	}

	IEnumerator SpawnPlayer(float respawnTime){
		int i = 0;
		yield return new WaitForSeconds(respawnTime);

		int index = Random.Range (0, spawnPoints.Length);
		//Create/Spawn player on network
		player = PhotonNetwork.Instantiate ("FPSPlayer", spawnPoints[index].position, spawnPoints[index].rotation, 0);
		//Once Player dies on network it will call Respawn me which will then call StartSpawn
		player.GetComponent<PlayerNetworkMover> ().RespawnMe += StartSpawnProcess;
		//player.GetComponent<PlayerNetworkMover> ().ScoreStats += onDeath;
		player.GetComponent<PlayerNetworkMover> ().SendNetworkMessage += AddMessage;//"Subscribe" to it
		sceneCamera.enabled = false;
		//Add player that just spawned to player list. 
	
	}

	void AddMessage(string message){

		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}

	[RPC]
	void AddMessage_RPC(string message){
		//Update queues for all clients
		messages.Enqueue (message);
		if (messages.Count > messageCount) { messages.Dequeue ();}
		//then write the messages to display on clients screen
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + "\n"; 		
		}
	}

	/*void onDeath(string name){

		Debug.Log ("DEATH!");
		for(int i = 0; i < players.Count; i++){
			
			if(players[i].GetComponent<PlayerNetworkMover>().playerName == name){
				players[i].GetComponent<PlayerNetworkMover>().deaths++;
				photonView.RPC ("Score_RPC", PhotonTargets.All, players[i].GetComponent<PlayerNetworkMover>().kills, players[i].GetComponent<PlayerNetworkMover>().deaths++);
				Debug.Log (players[i].GetComponent<PlayerNetworkMover>().playerName+ " Death Count: " + players[i].GetComponent<PlayerNetworkMover>().deaths++);
			}
		}
	}*/

	[RPC]
	void Score_RPC(string killss, string deathss){

		textKills.text = killss; 
		textDeaths.text = deathss; 
	}



	void OnApplicationQuit() {
		PhotonNetwork.Disconnect ();
	}

	public void QuitGame(){

		Application.Quit();
	}
	void OnPhotonPlayerDisconnected(PhotonPlayer playerDC){
		//Remove DC player from list of players

		AddMessage ("Player " + playerDC.name + " disconnected.");
	}
}
