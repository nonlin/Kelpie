//FPSPlayer.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class FPSPlayer : MonoBehaviour {
 	//other objects accessed by this script
	[HideInInspector]
	public GameObject weaponCameraObj;
	[HideInInspector]
	public GameObject weaponObj;
	[HideInInspector]
	public GameObject painFadeObj;
	[HideInInspector]
	public GameObject levelLoadFadeObj;
	[HideInInspector]
	public GameObject healthGuiObj;//this object is instantiated for heath display on hud
	[HideInInspector]
	public GameObject healthGuiObjInstance;
	[HideInInspector]
	public GameObject helpGuiObj;//this object is instantiated for help text display
	[HideInInspector]
	public GameObject helpGuiObjInstance;
	[HideInInspector]
	public GameObject PickUpGuiObj;//this object is instantiated for hand pick up crosshair on hud
	[HideInInspector]
	public GameObject PickUpGuiObjGuiObjInstance;
	[HideInInspector]
	public GameObject CrosshairGuiObj;//this object is instantiated for aiming reticle on hud
	[HideInInspector]
	public GameObject CrosshairGuiObjInstance;
	[HideInInspector]
	public Projector shadow;//to access the player shadow projector 
	private AudioSource[]aSources;//access the audio sources attatched to this object as an array for playing player sound effects
	
	//player hit points
	public float hitPoints = 100.0f;
	public float maximumHitPoints = 200.0f;
	
	//Damage feedback
	private float gotHitTimer = -1.0f;
	public Color PainColor = new Color(0.75f, 0f, 0f, 0.5f);//color of pain screen flash can be selected in editor
	
	//crosshair 
	public bool crosshairEnabled = true;//enable or disable the aiming reticle
	private bool crosshairVisibleState = true;
	private bool crosshairTextureState = false;
	public Texture2D Reticle;//the texture used for the aiming crosshair
	public Texture2D Hand;//the texture used for the pick up crosshair
	private Color handColor = Color.white; 
	private Color reticleColor = Color.white; 
	[HideInInspector]
	public LayerMask rayMask = 0;//only layers to include for crosshair raycast in hit detection (for efficiency)
	
	//button and behavior states
	private bool pickUpBtnState = true;
	[HideInInspector]
	public bool restarting = false;//to notify other scripts that level is restarting
	
	//zooming
	private bool zoomBtnState = true;
	private float zoomStopTime = 0.0f;//track time that zoom stopped to delay making aim reticle visible again
	[HideInInspector]
	public bool zoomed = false;
	private float zoomStart = -2.0f;
	private bool zoomStartState = false;
	private float zoomEnd = 0.0f;
	private bool zoomEndState = false;
	
	//sound effects
	public AudioClip painLittle;
	public AudioClip painBig;
	public AudioClip die;
	
	//player controls set in the inspector
	public KeyCode moveForward;
	public KeyCode moveBack;
	public KeyCode strafeLeft;
	public KeyCode strafeRight;
	public KeyCode jump;
	public KeyCode crouch;
	public KeyCode sprint;
	public KeyCode fire;
	public KeyCode zoom;
	public KeyCode reload;
	public KeyCode fireMode;
	public KeyCode holsterWeapon;
	public KeyCode selectNextWeapon;
	public KeyCode selectPreviousWeapon;
	public KeyCode selectWeapon1;
	public KeyCode selectWeapon2;
	public KeyCode selectWeapon3;
	public KeyCode selectWeapon4;
	public KeyCode selectWeapon5;
	public KeyCode selectWeapon6;
	public KeyCode selectWeapon7;
	public KeyCode selectWeapon8;
	public KeyCode selectWeapon9;
	public KeyCode selectWeapon10;
	public KeyCode use;
	public KeyCode moveObject;
	public KeyCode throwObject;
	public KeyCode showHelp;
	public KeyCode restartScene;
	public KeyCode exitGame;
	
	void Start (){	
		//Set time settings to optimal values
		Time.fixedDeltaTime = 0.01f;
		Time.maximumDeltaTime = 0.3333333f;
		//set up physics to allow bullet casings to bounce on thin surfaces
		Physics.minPenetrationForPenalty = 0.001f;
		
		//Physics Layer Management Setup
		//these are the layer numbers and their corresponding uses/names accessed by the FPS prefab
		//	Weapon = 8;
		//	Ragdoll = 9;
		//	WorldCollision = 10;
		//	Player = 11;
		//	Objects = 12;
		//	NPCs = 13;
		//	GUICameraLayer = 14;
		//	WorldGeometry = 15;
		//	BulletMarks = 16;
		
		//player object collisions
		Physics.IgnoreLayerCollision(11, 12);//no collisions between player object and misc objects like bullet casings
		Physics.IgnoreLayerCollision (12, 12);//no collisions between bullet shells
		Physics.IgnoreLayerCollision(11, 9);//no collisions between player and ragdolls
		//weapon object collisions
		Physics.IgnoreLayerCollision(8, 13);//no collisions between weapon and NPCs
		Physics.IgnoreLayerCollision(8, 12);//no collisions between weapon and Objects
		Physics.IgnoreLayerCollision(8, 11);//no collisions between weapon and Player
		Physics.IgnoreLayerCollision(8, 10);//no collisions between weapon and world collision
		Physics.IgnoreLayerCollision(8, 9);//no collisions between weapon and ragdolls
		

		//Call FadeAndLoadLevel fucntion with fadeIn argument set to true to tell the function to fade in (not fade out and (re)load level)
		GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;
		llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, true);
		
		//create instance of GUIText to display health amount on hud
		healthGuiObjInstance = Instantiate(healthGuiObj,Vector3.zero,transform.rotation) as GameObject;
		//create instance of GUIText to display help text
		helpGuiObjInstance = Instantiate(helpGuiObj,Vector3.zero,transform.rotation) as GameObject;
		//create instance of GUITexture to display crosshair on hud
		CrosshairGuiObjInstance = Instantiate(CrosshairGuiObj,new Vector3(0.5f,0.5f,0.0f),transform.rotation) as GameObject;
		//set alpha of hand pickup crosshair
		handColor.a = 0.5f;
		//set alpha of aiming reticule and make it 100% transparent if crosshair is disabled
		if(crosshairEnabled){
			reticleColor.a = 0.25f;
		}else{
			//make alpha of aiming reticle zero/transparent
			reticleColor.a = 0.0f;
			//set alpha of aiming reticle at start to prevent it from showing, but allow item pickup hand reticle 
			CrosshairGuiObjInstance.GetComponent<GUITexture>().color = reticleColor;
		}
		
		//set reference for main color element of heath GUIText
		HealthText HealthText = healthGuiObjInstance.GetComponent<HealthText>();
		//set reference for shadow background color element of heath GUIText
		//this object is a child of the main health GUIText object, so access it as an array
		HealthText[] HealthText2 = healthGuiObjInstance.GetComponentsInChildren<HealthText>();
		
		//initialize health amounts on GUIText objects
		HealthText.healthGui = hitPoints;
		HealthText2[1].healthGui = hitPoints;
		
	}
	
	void FixedUpdate (){
		
		//set up external script references
		Ironsights IronsightsComponent = GetComponent<Ironsights>();
		FPSRigidBodyWalker FPSWalkerComponent = GetComponent<FPSRigidBodyWalker>();
		PlayerWeapons PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		WeaponBehavior WeaponBehaviorComponent = PlayerWeaponsComponent.weaponOrder[PlayerWeaponsComponent.childNum].GetComponent<WeaponBehavior>();
		
		//Exit application if escape is pressed
		if (Input.GetKey (exitGame)){
			Application.Quit();
		}
		
		//Restart level if v is pressed
		if (Input.GetKey (restartScene)){
			GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;//Create instance of levelLoadFadeObj
			//call FadeAndLoadLevel function with fadein argument set to false 
			//in levelLoadFadeObj to restart level and fade screen out from black on level load
			llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, false);
			//Set parent of shadow projector to main camera's parent because it stops moving after disabling all components on player death or level restart. 
			//FPSPlayer will continue moving for a short while due to momentum and makes shadow move away from player.
			shadow.transform.parent = Camera.main.transform.parent;
			//set restarting var to true to be accessed by FPSRigidBodyWalker script to stop rigidbody movement
			restarting = true;
			// Disable all scripts to deactivate player control upon player death
			Component[] coms = transform.parent.transform.gameObject.GetComponentsInChildren<MonoBehaviour>();
			foreach(var b in coms) {
				MonoBehaviour p = b as MonoBehaviour;
				if (p){
					p.enabled = false;
				}
			}

		}
		
		//toggle or hold zooming state by determining if zoom button is pressed or held
		if(Input.GetKey (zoom)){
			if(!zoomStartState){
				zoomStart = Time.time;//track time that zoom button was pressed
				zoomStartState = true;//perform these actions only once
				zoomEndState = false;
				if(zoomEnd - zoomStart < 0.4f){//if button is tapped, toggle zoom state
					if(!zoomed){
						zoomed = true;
					}else{
						zoomed = false;	
					}
				}
			}
		}else{
			if(!zoomEndState){
				zoomEnd = Time.time;//track time that zoom button was released
				zoomEndState = true;
				zoomStartState = false;
				if(zoomEnd - zoomStart > 0.4f){//if releasing zoom button after holding it down, stop zooming
					zoomed = false;	
				}
			}
		}
		
		//track when player stopped zooming to allow for delay of reticle becoming visible again
		if (zoomed){
			zoomBtnState = false;//only perform this action once per button press
		}else{
			if(!zoomBtnState){
				zoomStopTime = Time.time;
				zoomBtnState = true;
			}
		}
		
		//enable and disable crosshair based on various states like reloading and zooming
		if(IronsightsComponent.reloading || zoomed){
			//don't disable reticle if player is using a melee weapon or if player is unarmed
			if(WeaponBehaviorComponent.meleeSwingDelay == 0 && !WeaponBehaviorComponent.unarmed){
				if(crosshairVisibleState){
					//disable the GUITexture element of the instantiated crosshair object
					//and set state so this action will only happen once.
					CrosshairGuiObjInstance.GetComponent<GUITexture>().enabled = false;
					crosshairVisibleState = false;
				}
			}
		}else{
			//Because of the method that is used for non magazine reloads, an additional check is needed here
			//to make the reticle appear after the last bullet reload time has elapsed. Proceed with no check
			//for magazine reloads.
			if((WeaponBehaviorComponent.bulletsPerClip != WeaponBehaviorComponent.bulletsToReload 
				&& WeaponBehaviorComponent.reloadLastStartTime + WeaponBehaviorComponent.reloadLastTime < Time.time)
			|| WeaponBehaviorComponent.bulletsPerClip == WeaponBehaviorComponent.bulletsToReload){
				//allow a delay before enabling crosshair again to let the gun return to neutral position
				//by checking the zoomStopTime value
				if(!crosshairVisibleState && (zoomStopTime + 0.2f < Time.time)){
					CrosshairGuiObjInstance.GetComponent<GUITexture>().enabled = true;
					crosshairVisibleState = true;
				}
			}
		}
				
		//Pick up items		
		RaycastHit hit;
		if(!IronsightsComponent.reloading//no item pickup when reloading
		&& !WeaponBehaviorComponent.lastReload//no item pickup when when reloading last round in non magazine reload
		&& !PlayerWeaponsComponent.switching//no item pickup when switching weapons
		&& !FPSWalkerComponent.canRun//no item pickup when sprinting
			//there is a small delay between the end of canRun and the start of sprintSwitching (in PlayerWeapons script),
			//so track actual time that sprinting stopped to avoid the small time gap where the pickup hand shows briefly
		&& ((FPSWalkerComponent.sprintStopTime + 0.4f) < Time.time)){
			//raycast a line from the main camera's origin using a point extended forward from camera position/origin as a target to get the direction of the raycast
			if (Physics.Raycast(Camera.main.transform.position, ((Camera.main.transform.position + Camera.main.transform.forward * 5.0f) - Camera.main.transform.position).normalized, out hit, 2.0f, rayMask)) {
				if(hit.collider.gameObject.tag == "PickUp"){//if the object hit by the raycast is a pickup item and has the "Pickup" tag
					
					if (pickUpBtnState && Input.GetKey(use)){
						//run the PickUpItem function in the pickup object's script
						hit.collider.SendMessageUpwards("PickUpItem", SendMessageOptions.DontRequireReceiver);
						pickUpBtnState = false;
					}
					
					if(!crosshairTextureState){
						UpdateReticle(false);//show hand pickup crosshair if raycast hits a pickup item
					}
				}else{
					if(crosshairTextureState){
						UpdateReticle(true);//show aiming reticle crosshair if item is not a pickup item
					}
				}
			}else{
				if(crosshairTextureState){
					UpdateReticle(true);//show aiming reticle crosshair if raycast hits nothing
				}
			}
		}else{
			if(crosshairTextureState){
				UpdateReticle(true);//show aiming reticle crosshair if reloading, switching weapons, or sprinting
			}
		}
		
		//only register one press of E key to make player have to press button again to pickup items instead of holding E
		if (Input.GetKey(use)){
			pickUpBtnState = false;
		}else{
			pickUpBtnState = true;	
		}
	
	}
	
	//set reticle type based on the boolean value passed to this function
	void UpdateReticle( bool reticleType ){
		if(!reticleType){
			CrosshairGuiObjInstance.GetComponent<GUITexture>().texture = Hand;
			CrosshairGuiObjInstance.GetComponent<GUITexture>().color = handColor;
			crosshairTextureState = true;
		}else{
			CrosshairGuiObjInstance.GetComponent<GUITexture>().texture = Reticle;
			CrosshairGuiObjInstance.GetComponent<GUITexture>().color = reticleColor;
			crosshairTextureState = false;	
		}
	}
	
	//add hitpoints to player health
	public void HealPlayer( float healAmt  ){
			
		if (hitPoints <= 0.0f){//Don't add health if player is dead
			return;
		}
		
		//Update health GUIText 
		HealthText HealthText = healthGuiObjInstance.GetComponent<HealthText>();
		HealthText[] HealthText2 = healthGuiObjInstance.GetComponentsInChildren<HealthText>();
		
		//Apply healing
		if(hitPoints + healAmt > maximumHitPoints){ 
			hitPoints = maximumHitPoints;
		}else{
			hitPoints += healAmt;
		}
			
		//set health hud value to hitpoints remaining
		HealthText.healthGui = Mathf.Round(hitPoints);
		HealthText2[1].healthGui = Mathf.Round(hitPoints);
			
		//change color of hud health element based on hitpoints remaining
		if (hitPoints <= 25.0f){
			HealthText.guiText.material.color = Color.red;
		}else if (hitPoints <= 40.0f){
				HealthText.guiText.material.color = Color.yellow;	
		}else{
			HealthText.guiText.material.color = HealthText.textColor;	
		}

	}
	
	//remove hitpoints from player health
	public void ApplyDamage ( float damage  ){
			
		if (hitPoints <= 0.0f){//Don't apply damage if player is dead
			return;
		}
		
		//Update health GUIText 
		HealthText HealthText = healthGuiObjInstance.GetComponent<HealthText>();
		HealthText[] HealthText2 = healthGuiObjInstance.GetComponentsInChildren<HealthText>();

	    Quaternion painKickRotation;//Set up rotation for pain view kicks
	    int painKickUpAmt = 0;
	    int painKickSideAmt = 0;
	
		hitPoints -= damage;//Apply damage
			
		//set health hud value to hitpoints remaining
		HealthText.healthGui = Mathf.Round(hitPoints);
		HealthText2[1].healthGui = Mathf.Round(hitPoints);
			
		//change color of hud health element based on hitpoints remaining
		if (hitPoints <= 25.0f){
			HealthText.guiText.material.color = Color.red;
		}else if (hitPoints <= 40.0f){
				HealthText.guiText.material.color = Color.yellow;	
		}else{
			HealthText.guiText.material.color = HealthText.textColor;	
		}
		
		GameObject pf = Instantiate(painFadeObj) as GameObject;//Create instance of painFadeObj
		pf.GetComponent<PainFade>().FadeIn(PainColor, 0.75f);//Call FadeIn function in painFadeObj to fade screen red when damage taken
			
		//Play pain sound when getting hit
		if (Time.time > gotHitTimer && painBig && painLittle) {
			// Play a big pain sound
			if (hitPoints < 40 || damage > 30) {
				AudioSource.PlayClipAtPoint(painBig, Camera.main.transform.position);
				gotHitTimer = Time.time + Random.Range(.5f, .75f);
			} else {
				//Play a small pain sound
				AudioSource.PlayClipAtPoint(painLittle, Camera.main.transform.position);
				gotHitTimer = Time.time + Random.Range(.5f, .75f);
			}
		}
		
		painKickUpAmt = Random.Range(100, -100);//Choose a random view kick up amount
		if(painKickUpAmt < 50 && painKickUpAmt > 0){painKickUpAmt = 50;}//Maintain some randomness of the values, but don't make it too small
		if(painKickUpAmt < 0 && painKickUpAmt > -50){painKickUpAmt = -50;}
		
		painKickSideAmt = Random.Range(100, -100);//Choose a random view kick side amount
		if(painKickSideAmt < 50 && painKickSideAmt > 0){painKickSideAmt = 50;}
		if(painKickSideAmt < 0 && painKickSideAmt > -50){painKickSideAmt = -50;}
		
		//create a rotation quaternion with random pain kick values
		painKickRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles - new Vector3(painKickUpAmt, painKickSideAmt, 0));
		
		//smooth current camera angles to pain kick angles using Slerp
		Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, painKickRotation, 0.016f );
	
		//Call Die function if player is dead
		if (hitPoints <= 0.0f){
			Die();
		}
	}
	
	void Die (){
		AudioSource.PlayClipAtPoint(die, Camera.main.transform.position);
			
		GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;//Create instance of levelLoadFadeObj
		//call FadeAndLoadLevel function with fadein argument set to false 
		//in levelLoadFadeObj to restart level and fade screen out from black on level load
		llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, false);
		
		//Set parent of shadow projector to main camera's parent because it stops moving after disabling all components on player death or level restart. 
		//FPSPlayer will continue moving for a short while due to momentum and makes shadow move away from player.
		shadow.transform.parent = Camera.main.transform.parent;
		
		// Disable all scripts in prefab object to deactivate player control upon player death
		Component[] coms = transform.parent.transform.gameObject.GetComponentsInChildren<MonoBehaviour>();
		foreach(var b in coms) {
			MonoBehaviour p = b as MonoBehaviour;
			if (p){
				p.enabled = false;
			}
		}
		

		
	}

}