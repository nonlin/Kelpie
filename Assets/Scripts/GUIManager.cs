using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	
	string kills = "";
	string deaths = "";
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ScoreBoard(){

		GUI.Label (new Rect (Screen.height - (Screen.height / 2) + 150, Screen.width - (Screen.width / 2) - 20, 150, 150), "Kills");
		GUI.Label (new Rect (Screen.height - (Screen.height / 2) + 225, Screen.width - (Screen.width / 2) - 20, 150, 150), "Deaths");
		GUILayout.BeginArea(new Rect(Screen.height - (Screen.height/2), Screen.width - (Screen.width/2), 400,500));

		foreach (PhotonPlayer p in PhotonNetwork.playerList) {
		
			GUILayout.BeginHorizontal("Box");
			//Player Names
			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.Label (p.name, GUILayout.Width (150));
			GUILayout.EndVertical();
			//Player Score
			//GUILayout.BeginVertical(GUILayout.Width(75));
			//GUILayout.Label (p.playerName, GUILayout.Width (75));
			//GUILayout.EndVertical();
			//Player Kills
			GUILayout.BeginVertical(GUILayout.Width(75));
			//kills = p.customProperties["K"].ToString();
			GUILayout.Label (p.customProperties["K"].ToString(), GUILayout.Width (75));
			GUILayout.EndVertical();
			//Player Deaths
			GUILayout.BeginVertical(GUILayout.Width(75));
			//deaths = p.customProperties["D"].ToString();
			GUILayout.Label (p.customProperties["D"].ToString(), GUILayout.Width (75));
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			//Debug.Log ("GUI Player Name" + p.playerName);
		}
		GUILayout.EndArea();
	}

	/*void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {

		foreach (PhotonPlayer p in PhotonNetwork.playerList) {
			kills = p.customProperties["K"].ToString();
			deaths = p.customProperties["D"].ToString();
		}
	}*/
}
