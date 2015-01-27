//PlayerWeapons.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class PlayerWeapons : MonoBehaviour {

	
	public int firstWeapon = 0;//the weaponOrder index of the first weapon that will be selected when the map loads
	//Define array for storing order of weapons. This array is created in the inspector by dragging and dropping 
	//weapons from under the FPSWeapons branch in the FPS Prefab. Weapon 0 should always be the unarmed/null weapon.
	public GameObject[] weaponOrder;

	//objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject cameraObj;
	[HideInInspector]
	public GameObject weaponObj;
	[HideInInspector]
	public GameObject ammoGuiObj;//this GUI object will be instantiated on level load to display player ammo
	[HideInInspector]
	public GameObject ammoGuiObjInstance;
	private Transform myTransform;
	
	//weapon switching
	private float switchTime = 0.000f;//time that weapon switch started
	[HideInInspector]
	public float sprintSwitchTime = 0.000f;//time that weapon sprinting animation started, set in WeaponBehavior script
	[HideInInspector]
	public bool switching = false;//true when switching weapons
	[HideInInspector]
	public bool sprintSwitching = false;//true when weapon sprinting animation is playing
	[HideInInspector]
	public int childNum = 0;//index of weaponOrder array that corresponds to current weapon 
		
	//sound effects
	public AudioClip changesnd;

	void Start (){

		myTransform = transform;//define transform for efficiency
		
		//Create instance of GUIText to display ammo amount on hud. This will be accessed and updated by WeaponBehavior script.
		ammoGuiObjInstance = Instantiate(ammoGuiObj,Vector3.zero,myTransform.rotation) as GameObject;
		
		//set the weapon order number in the WeaponBehavior scripts
		for(int i = 0; i < weaponOrder.Length; i++)	{
			weaponOrder[i].GetComponent<WeaponBehavior>().weaponNumber = i;
		}
		
		//Select first weapon, if firstWeapon is not in inventory, player will spawn unarmed.
		if(weaponOrder[firstWeapon].GetComponent<WeaponBehavior>().haveWeapon){
			StartCoroutine(SelectWeapon(firstWeapon));
		}else{
			StartCoroutine(SelectWeapon(0));	
		}

	}
	
	void Update (){
	
		//set up external script references
		FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Switch Weapons
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
		if(Time.timeSinceLevelLoad > 2//don't allow weapon switching when level is still loading
		 && !(!FPSWalker.grounded && FPSWalker.sprintActive)//don't allow switching if player is sprinting and airborn
		 && !switching//only allow one weapon switch per time
		 && !weaponOrder[childNum].GetComponent<WeaponBehavior>().shooting//don't switch weapons if shooting
		 && !sprintSwitching){//don't allow weapon switching while sprint anim is active/transitioning
		  	
		  	//Cycle weapons using the mousewheel (cycle through FPS Weapon children) and skip weapons that are not in player inventory.
			//weaponOrder.Length - 1 is the last weapon because the built in array starts counting at zero and weaponOrder.Length starts counting at one. 
			if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(FPSPlayerComponent.selectPreviousWeapon)){//mouse wheel down
				if(childNum != 0){//not starting at zero
					for (int i = childNum; i > -1; i--)	{
						if(weaponOrder[i].GetComponent<WeaponBehavior>().haveWeapon && i != childNum){//check that player has weapon and it is not currently selected weapon
							StartCoroutine(SelectWeapon(i));//run the SelectWeapon function with the next weapon index that was found
							break;
						}else if(i == 0){//reached zero, count backwards from end of list to find next weapon
							for (int n = weaponOrder.Length - 1; n > -1; n--)	{
								if(weaponOrder[n].GetComponent<WeaponBehavior>().haveWeapon && n != childNum){
									StartCoroutine(SelectWeapon(n));
									break;
								}
							}
						}
					}
				}else{//starting at 0
					for (int i = weaponOrder.Length - 1; i > -1; i--)	{
						if(weaponOrder[i].GetComponent<WeaponBehavior>().haveWeapon && i != childNum){
							StartCoroutine(SelectWeapon(i));
							break;
						}
					}
				}
			}else if(Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(FPSPlayerComponent.selectNextWeapon)){//mouse wheel up
				if(childNum < weaponOrder.Length -1){//not starting at last weapon
					for (int i = childNum; i < weaponOrder.Length; i++)	{
						if(weaponOrder[i].GetComponent<WeaponBehavior>().haveWeapon && i != childNum){
							StartCoroutine(SelectWeapon(i));
							break;
						}else if(i == weaponOrder.Length - 1){//reached end of list, count forwards from zero to find next weapon
							for (int n = 0; n < weaponOrder.Length - 1; n++)	{
								if(weaponOrder[n].GetComponent<WeaponBehavior>().haveWeapon && n != childNum){
									StartCoroutine(SelectWeapon(n));
									break;
								}
							}
						}
					}
				}else{//starting at last weapon
					for (int i = 0; i < weaponOrder.Length - 1; i++)	{
						if(weaponOrder[i].GetComponent<WeaponBehavior>().haveWeapon && i != childNum){
							StartCoroutine(SelectWeapon(i));
							break;
						}
					}
				}	
			}
			
			//select weapons with number keys
			if (Input.GetKeyDown(FPSPlayerComponent.holsterWeapon)) {
				if(childNum != 0){StartCoroutine(SelectWeapon(0));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon1)) {
				if(childNum != 1){StartCoroutine(SelectWeapon(1));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon2)) {
				if(childNum != 2){StartCoroutine(SelectWeapon(2));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon3)) {
				if(childNum != 3){StartCoroutine(SelectWeapon(3));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon4)) {
				if(childNum != 4){StartCoroutine(SelectWeapon(4));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon5)) {
				if(childNum != 5){StartCoroutine(SelectWeapon(5));}
			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon6)) {
				if(childNum != 6){StartCoroutine(SelectWeapon(6));}
			}//else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon7)) {
//				if(childNum != 7){StartCoroutine(SelectWeapon(7));}
//			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon8)) {
//				if(childNum != 8){StartCoroutine(SelectWeapon(8));}
//			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon9)) {
//				if(childNum != 9){StartCoroutine(SelectWeapon(9));}
//			}else if (Input.GetKeyDown(FPSPlayerComponent.selectWeapon10)) {
//				if(childNum != 9){StartCoroutine(SelectWeapon(10));}
//			}
			
		}
		
		//check timer for switch to prevent shooting
		//this var checked in "WeaponBehavior" script in the Fire() function 
		if(switchTime + 0.87f > Time.time){
			switching = true;
		}else{
			switching = false;
		}
		
		//define time that sprinting anim is active/transitioning to disable weapon switching
		if(sprintSwitchTime + 0.44f > Time.time){
			sprintSwitching = true;
		}else{
			sprintSwitching = false;
		}
		
		//align weapon parent origin with player camera origin
		Vector3 tempGunPosition = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y,Camera.main.transform.position.z);
		myTransform.position = tempGunPosition;
		
	}
	
	public IEnumerator SelectWeapon ( int index  ){
		
		//do not proceed with selecting weapon if player doesn't have it in their inventory
		//but make an exception for the null/unarmed weapon for when the player presses the holster button
		if(!weaponOrder[index].GetComponent<WeaponBehavior>().haveWeapon && index != 0){
			return false;
		}
		
		if(index != 0){//if a weapon is selected, prevent unarmed/null weapon from being selected in selection cycle 
			weaponOrder[0].GetComponent<WeaponBehavior>().haveWeapon = false;
		}
	
		//set up external script references
		Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
		CameraKick CameraKickComponent = Camera.main.GetComponent<CameraKick>();
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		
		//cancel zooming when switching
		FPSPlayerComponent.zoomed = false;
		
		//cancel reloading when switching
		IronsightsComponent.reloading = false;//set IronSights Reloading var to false
		weaponOrder[childNum].GetComponent<WeaponBehavior>().StopCoroutine("Reload");//stop the Reload function if it is running
		
		//make timer active during switch to prevent shooting 
		switchTime = Time.time - Time.deltaTime;
		
		if(Time.timeSinceLevelLoad > 2){
			//play weapon switch sound if not the first call to this function after level load
			AudioSource.PlayClipAtPoint(changesnd, Camera.main.transform.position);
		
			//play camera weapon switching animation
			Camera.main.animation.Rewind("CameraSwitch");
			Camera.main.animation.CrossFade("CameraSwitch", 0.35f,PlayMode.StopAll);
		}
		
		//if weapon uses rifle sprinting animation, set speed and play animation
		if(!weaponOrder[childNum].GetComponent<WeaponBehavior>().PistolSprintAnim){
			//animate previous weapon down
			if(!FPSWalkerComponent.canRun){
				weaponOrder[childNum].animation["RifleSprinting"].normalizedTime = 0;
				weaponOrder[childNum].animation["RifleSprinting"].speed = 1.5f;
				weaponOrder[childNum].animation.CrossFade("RifleSprinting", 0.00025f,PlayMode.StopAll);
			}else{
				//if player is sprinting, keep weapon in sprinting position during weapon switch
				weaponOrder[childNum].animation["RifleSprinting"].normalizedTime = 1;
			}
		}else{//weapon uses pistol sprinting animation
			//animate previous weapon down
			if(!FPSWalkerComponent.canRun){
				weaponOrder[childNum].animation["PistolSprinting"].normalizedTime = 0;
				weaponOrder[childNum].animation["PistolSprinting"].speed = 1.5f;
				weaponOrder[childNum].animation.CrossFade("PistolSprinting", 0.00025f,PlayMode.StopAll);
			}else{
				//if player is sprinting, keep weapon in sprinting position during weapon switch
				weaponOrder[childNum].animation["PistolSprinting"].normalizedTime = 1;
			}
		}
		
		if(Time.timeSinceLevelLoad > 2){
			if(weaponOrder[childNum].GetComponent<WeaponBehavior>().meleeSwingDelay == 0){
				//move weapon down while switching
				IronsightsComponent.switchMove = -0.4f;
			}else{
				//move melee weapons down further while switching because they take more vertical screen space than guns
				IronsightsComponent.switchMove = -1.2f;
			}
			
			//wait for weapon down animation to play before switching weapons and animating weapon up
			yield return new WaitForSeconds(0.2f);
			
		}
		
		//immediately switch weapons (activate called weaponOrder index and deactivate all others)
		for (int i = 0; i < weaponOrder.Length; i++)	{
			if (i == index){
			
				
				#if UNITY_3_5
					// Activate the selected weapon
					weaponOrder[i].SetActiveRecursively(true);
				#else
					// Activate the selected weapon
					weaponOrder[i].SetActive(true);
				#endif
				
				//get current weapon value from index
				childNum = index;
			
				//synchronize current and previous weapon's y pos for correct offscreen switching, use localPosition not position for correct transforms
				weaponOrder[i].transform.localPosition = weaponOrder[i].transform.localPosition + new Vector3(0, weaponOrder[i].transform.localPosition.y - 0.3f, 0);
				
				if(Time.timeSinceLevelLoad > 2){
					//move weapon up when switch finishes
					IronsightsComponent.switchMove = 0;
				}
				
				//if weapon uses rifle sprinting animation set speed and animate 
				if(!weaponOrder[i].GetComponent<WeaponBehavior>().PistolSprintAnim){
					//animate selected weapon up by setting time of animation to it's end and playing in reverse
					if(!FPSWalkerComponent.canRun){
						weaponOrder[i].animation["RifleSprinting"].normalizedTime = 1.0f;	
						weaponOrder[i].animation["RifleSprinting"].speed = -1.5f;
						weaponOrder[i].animation.CrossFade("RifleSprinting", 0.00025f,PlayMode.StopAll);
					}else{
						//if player is sprinting, keep weapon in sprinting position during weapon switch
						weaponOrder[i].animation["RifleSprinting"].normalizedTime = 1.0f;
						if(Time.timeSinceLevelLoad > 1){//set time and also play animation after delay to prevent visual glitches in sprint anim while switching
							weaponOrder[i].animation["RifleSprinting"].speed = 1.5f;
							weaponOrder[i].animation.CrossFade("RifleSprinting", 0.00025f,PlayMode.StopAll);	
						}
					}
				}else{//weapon uses pistol sprinting animation
					//animate selected weapon up by setting time of animation to it's end and playing in reverse
					if(!FPSWalkerComponent.canRun){
						weaponOrder[i].animation["PistolSprinting"].normalizedTime = 1.0f;	
						weaponOrder[i].animation["PistolSprinting"].speed = -1.5f;
						weaponOrder[i].animation.CrossFade("PistolSprinting", 0.00025f,PlayMode.StopAll);
					}else{
						//if player is sprinting, keep weapon in sprinting position during weapon switch
						weaponOrder[i].animation["PistolSprinting"].normalizedTime = 1.0f;
						if(Time.timeSinceLevelLoad > 1){//set time and also play animation after delay to prevent visual glitches in sprint anim while switching
							weaponOrder[i].animation["PistolSprinting"].speed = 1.5f;
							weaponOrder[i].animation.CrossFade("PistolSprinting", 0.00025f,PlayMode.StopAll);	
						}
					}	
				}
			
				//update transform reference of active weapon object in other scipts
				IronsightsComponent.gun = weaponOrder[i].transform;
				//update active weapon object reference in other scipts
				IronsightsComponent.gunObj = weaponOrder[i];
				CameraKickComponent.gun = weaponOrder[i];
	
			}else{
				
				//reset transform of deactivated gun to make it in neutral position when selected again
				//use weapon parent transform.position instead of Camera.main.transform.position
				//or Camera.main.transform.localPosition to avoid positioning bugs due to camera pos changing with walking bob and kick 
				weaponOrder[i].transform.position = myTransform.position;
				
				if(!weaponOrder[i].GetComponent<WeaponBehavior>().PistolSprintAnim){//weapon uses rifle sprinting animation
					//reset animation
					weaponOrder[i].animation["RifleSprinting"].normalizedTime = 1.0f;
				}else{//weapon uses pistol sprinting animation
					//reset animation
					weaponOrder[i].animation["PistolSprinting"].normalizedTime = 1.0f;
				}
				//synchronize sprintState var in WeaponBehavior script
				weaponOrder[i].GetComponent<WeaponBehavior>().sprintState = true;
				
				#if UNITY_3_5
					// Activate the selected weapon
					weaponOrder[i].SetActiveRecursively(false);
				#else
					// Activate the selected weapon
					weaponOrder[i].SetActive(false);
				#endif
			}	
		}	
	}
}