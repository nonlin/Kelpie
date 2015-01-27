//HealthText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HealthText : MonoBehaviour {
	//draw ammo amount on screen
	public float healthGui = 0;
	public Color textColor;
	public float horizontalOffset = 0.0425f;
	public float verticalOffset = 0.075f;
	
	void Start(){
		guiText.material.color = textColor;
	}
	
	void Update (){
		guiText.text = "Health : "+ healthGui.ToString();
		guiText.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
	}
	
}