//WeaponPickup.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;
//script for weapon pickups
public class WeaponPickup : MonoBehaviour {
	public int weaponNumber = 0;//this number corresponds with the weapon's index in the PlayerWeapons script weaponOrder array
	private GameObject weaponObj;//the GameObject that is a child of FPS Weapons which has the WeaponBehavior script attatched
	public bool removeOnUse = true;//Does this pickup disappear when used/activated by player?
	public AudioClip pickupSound;//sound to play when picking up weapon
	public AudioClip fullSound;//sound to play when ammo is full
	private Transform myTransform;
	
	// Use this for initialization
	void Start () {
		myTransform = transform;//manually set transform for efficiency
		//find the PlayerWeapons script in the FPS Prefab to access weaponOrder array
		PlayerWeapons PlayerWeaponsComponent = Camera.main.transform.parent.transform.parent.GetComponentInChildren<PlayerWeapons>();
		//scan the children of the FPS Weapons object (PlayerWeapon's weaponOrder array) and assign this item's weaponObj to the
		//weapon object whose weaponNumber in its WeaponBehavior script matches this item's weapon number
		for (int i = 0; i < PlayerWeaponsComponent.weaponOrder.Length; i++)	{
			if(PlayerWeaponsComponent.weaponOrder[i].GetComponent<WeaponBehavior>().weaponNumber == weaponNumber){
				weaponObj = PlayerWeaponsComponent.weaponOrder[i];
				break;
			}
		}
		
	}
	
	void PickUpItem (){
		//if player does not have this weapon, pick it up
		if(!weaponObj.GetComponent<WeaponBehavior>().haveWeapon){
			weaponObj.GetComponent<WeaponBehavior>().haveWeapon = true;

			//select the weapon after picking it up
			weaponObj.transform.parent.GetComponent<PlayerWeapons>()
			  .StartCoroutine(weaponObj.transform.parent.GetComponent<PlayerWeapons>()
			    .SelectWeapon(weaponObj.GetComponent<WeaponBehavior>().weaponNumber));
			
			RemovePickup();
			
		}else{//the player already has this weapon
		
			if (weaponObj.GetComponent<WeaponBehavior>().ammo < weaponObj.GetComponent<WeaponBehavior>().maxAmmo) {
			
				if(weaponObj.GetComponent<WeaponBehavior>().ammo + weaponObj.GetComponent<WeaponBehavior>().bulletsPerClip > weaponObj.GetComponent<WeaponBehavior>().maxAmmo){
					//just give player max ammo if they only are a few bullets away from having max ammo
					weaponObj.GetComponent<WeaponBehavior>().ammo = weaponObj.GetComponent<WeaponBehavior>().maxAmmo;	
				}else{
					//give player the bulletsPerClip amount if they already have this weapon
					weaponObj.GetComponent<WeaponBehavior>().ammo += weaponObj.GetComponent<WeaponBehavior>().bulletsPerClip;	
				}
				
				RemovePickup();
			}else{
				//if player has weapon and is at max ammo, just play beep sound
				if(fullSound){AudioSource.PlayClipAtPoint(fullSound, myTransform.position, 0.75f);}	
			}
		}
	}
	
	void RemovePickup (){
		
		//play pickup sound
		if(pickupSound){AudioSource.PlayClipAtPoint(pickupSound, myTransform.position, 0.75f);}
		
		if(removeOnUse){
			//remove this weapon pickup from the scene
			Object.Destroy(gameObject);	
		}
	}
}