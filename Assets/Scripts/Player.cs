using UnityEngine;
using System.Collections;
using System;

public class Player : MonoBehaviour {

	public string playerName;
	public int deaths;
	public int kills;
	public GameObject go; 
	
	public Player(GameObject GO, string name, int death, int kill){

		go = GO; 
		playerName = name; 
		deaths = death;
		kills = kill; 

	}


}
