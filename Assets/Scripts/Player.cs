using UnityEngine;
using System.Collections;
using System;

public class Player : IComparable<Player> {

	public string playerName;
	public int deaths;
	public int kills;
	
	public Player(string name, int death, int kill){
		
		playerName = name; 
		deaths = death;
		kills = kill; 

	}

	public int CompareTo(Player other)
	{
		if(other == null)
		{
			return 1;
		}
		
		//Return the difference in power.
		return kills - other.kills;
	}
}
