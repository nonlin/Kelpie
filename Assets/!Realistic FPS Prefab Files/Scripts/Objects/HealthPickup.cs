//HealthPickup.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;
//script for health pickup items
public class HealthPickup : MonoBehaviour {
	private GameObject playerObj;
	public float healthToAdd = 25.0f;
	public bool removeOnUse = true;//Does this pickup disappear when used/activated by player?
	public AudioClip pickupSound;//sound to playe when picking up this item
	public AudioClip fullSound;//sound to play when health is full
	private Transform myTransform;
	
	// Use this for initialization
	void Start () {
		myTransform = transform;//manually set transform for efficiency
		//assign this item's playerObject value by traversing object tree upwards from main camera and then downwards from FPS Main root
		playerObj = Camera.main.transform.parent.transform.parent.GetComponentInChildren<FPSPlayer>().gameObject;
	}
	
	void PickUpItem (){
	FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
	
		if (FPSPlayerComponent.hitPoints < FPSPlayerComponent.maximumHitPoints){
			//heal player
			FPSPlayerComponent.SendMessage("HealPlayer", healthToAdd,SendMessageOptions.DontRequireReceiver);
			
			if(pickupSound){AudioSource.PlayClipAtPoint(pickupSound, myTransform.position, 0.75f);}
			
			if(removeOnUse){
				//remove this pickup from the scene
				Object.Destroy(gameObject);
			}
			
		}else{
			//player is already at max health, just play beep sound effect
			if(fullSound){AudioSource.PlayClipAtPoint(fullSound, myTransform.position, 0.75f);}		
		}
	}
}