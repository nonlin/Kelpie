using UnityEngine;
using System.Collections;

public class PlayerDataBase : MonoBehaviour {

	 string playerName;
	 int deaths;
	 int kills;

	public PlayerDataBase(string name, int d, int k){

		playerName = name; 
		deaths = d;
		kills = k; 
	}


}
