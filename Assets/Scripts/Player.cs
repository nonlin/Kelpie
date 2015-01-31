using UnityEngine;
using System.Collections;
using System;

public class Player : IComparable<Player> {

	public string playerName;
	public int deaths;
	public int kills;
	public GameObject go; 
	public int ID;                             
	
	public Player(GameObject GO, string name, int death, int kill, int id){

		go = GO; 
		playerName = name; 
		deaths = death;
		kills = kill; 
		ID = id; 
	}

	public int CompareTo(Player other){

		return ID; 
	}
}
