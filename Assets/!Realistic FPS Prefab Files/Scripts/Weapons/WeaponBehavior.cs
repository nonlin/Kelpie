//WeaponBehavior.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class WeaponBehavior : MonoBehaviour {
	
	public bool haveWeapon = false;//true if player has this weapon in their inventory
	[HideInInspector]
	public int weaponNumber = 0;//number of this weapon in the weaponOrder array in playerWeapons script
	
	//Other objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject weaponObj;
	public GameObject weaponMesh;
	[HideInInspector]
	public GameObject cameraObj;
	[HideInInspector]
	public GameObject ammoGuiObj;
	private Transform myTransform;
	
	//gun position amounts
	public float weaponUnzoomXPosition = -0.02f;//horizontal modifier of gun position when not zoomed
	public float weaponUnzoomYPosition = 0.0127f;//vertical modifier of gun position when not zoomed
	public float weaponUnzoomXPositionSprint = 0.075f;//horizontal modifier of gun position when sprinting
	public float weaponUnzoomYPositionSprint = 0.0075f;//vertical modifier of gun position when sprinting
	public float weaponZoomXPosition = -0.07f;//horizontal modifier of gun position when zoomed
	public float weaponZoomYPosition = 0.032f;//vertical modifier of gun position when zoomed
	public float zoomFOV = 55.0f;//FOV value to use when zoomed, lower values  can be used with scoped weapons for higher zoom
	public float swayAmountUnzoomed = 1.0f;//sway amount for this weapon when not zoomed
	public float swayAmountZoomed = 1.0f;//sway amount for this weapon when zoomed
	public bool PistolSprintAnim = false;//set to true to use alternate sprinting animation with pistols
	public	float sprintBobAmountX  = 1.0f;//to fine tune horizontal weapon sprint bobbing amounts
	public	float sprintBobAmountY  = 1.0f;//to fine tune vertical weapon sprint bobbing amounts
		
	//Sprinting and Player States
	private bool canShoot = true;//true when player is allowed to shoot
	[HideInInspector]
	public bool shooting = false;//true when shooting
	[HideInInspector]
	public bool sprintAnimState = false;//to control playback of sprinting animation
	[HideInInspector]
	public bool sprintState = false;//to control timing of weapon recovery after sprinting
	private float recoveryTime = 0.000f;//time that sprint animation started playing
	private float horizontal = 0;//player movement
	private float vertical = 0;//player movement
		
	//Shooting
	public int projectileCount  = 1;//amount of projectiles to be fired per shot ( > 1 for shotguns)
	public bool fireModeSelectable = false;//true if weapon can switch between burst and semi-auto
	private bool fireModeState = false;
	public bool semiAuto = false;//true when weapon is in semi-auto mode
	private bool semiState = false;
	public bool unarmed = false;//should this weapon be null/unarmed?
	public float meleeSwingDelay = 0.0f;//this weapon will be treated as a melee weapon when this value is > 0
	private bool swingSide = false;//to control which direction to swing melee weapon
	private float shootStartTime = 0.0f;//time that shot started
	public float range  = 100;//range that weapon can hit targets
	public float fireRate = 0.097f;//time between shots
	public float fireAnimSpeed = 1.0f;//speed to play the firing animation
	public float shotSpread = 0.0f;//defines accuracy cone of fired bullets
	private	float shotSpreadAmt = 0.0f;//actual accuracy amount
	public int force= 200;//amount of physics push to apply to rigidbodies on contact
	public int damage  = 10;//damage to inflict on objects with ApplyDamage(); function
	public LayerMask bulletMask = 0;//only layers to include in bullet hit detection (for efficiency)
		
	//Ammo and Reloading
	public int bulletsPerClip  = 30;//maximum amount of bullets per magazine
	public int bulletsToReload  = 50;//number of bullets to reload per reload cycle (when < bulletsPerClip, allows reloading one or more bullets at a time)
	private int bulletsNeeded = 0;//number of bullets absent in magazine
	public int bulletsLeft = 0;//bullets left in magazine	
	private int bulletsReloaded = 0;//number of bullets reloaded during this reloading cycle
	public int ammo = 150;//ammo amount for this weapon in player's inventory
	public int maxAmmo = 999;//maximum ammo amount player's inventory can hold for this weapon
	public float reloadTime = 1.75f;//time per reload cycle, should be shorter if reloading one bullet at a time and longer if reloading magazine
	private	float reloadStartTime = 0.0f;
	private bool sprintReloadState = false;
	private	float reloadEndTime = 0.0f;//used to allow fire button to cancel a reload if not reloading a magazine and bulletsLeft > 1
	public float reloadAnimSpeed = 1.15f;//speed of reload animation playback
	public float shellRldAnimSpeed = 0.7f;//speed of single shell/bullet reload animation playback
	public float readyAnimSpeed = 1.0f;//speed of ready animation playback
	public float readyTime = 0.6f;//amount of time needed to finish the ready anim after weapon has just been switched to/selected
	private float recoveryTimeAmt = 0.0f;//amount of time needed to recover weapon center position after sprinting
	private float startTime = 0.0f;//track time that weapon was selected to calculate readyTime
	[HideInInspector]
	public float reloadLastTime = 1.2f;//to track when last bullet is reloaded if not reloading magazine, to play chambering animation and sound
	[HideInInspector]
	public	float reloadLastStartTime = 0.0f;
	[HideInInspector]
	public bool lastReload = false;//true when last bullet of a non -magazine reload is being loaded, to play chambering animation and sound 
	private bool noAmmoState = false;//to track ammo depletion and to play out of ammo sound	
	
	//Muzzle Flash
	public Renderer muzzleFlash;//the game object that will be used as a muzzle flash
	private float muzzleFlashReduction = 6.5f;//value to control time that muzzle flash is on screen (lower value make muzzle flash fade slower)
	[HideInInspector]
	public Color muzzleFlashColor  = new Color(1, 1, 1, 0.0f);
		
	//View Kick
	[HideInInspector]
	public Quaternion kickRotation;//rotation used for screen kicks
	public float kickUp = 7.0f;//amount to kick view up when firing (set in editor)
	public float kickSide = 2.0f;//amount to kick view sideways when firing (set in editor)
	private float kickUpAmt = 0.0f;//actual amount to kick view up when firing
	private float kickSideAmt = 0.0f;//actual amount to kick view sideways when firing
		
	//Shell Ejection
	public GameObject shellPrefab;//game object to use as empty casing and eject from shellEjectPosition
	public Vector3 shellEjectDirection = new Vector3(0.0f, 0.0f, 0.0f);//direction of ejected shell casing
	[HideInInspector]
	public Transform shellEjectPosition;//position shell is ejected from (use origin of ShellEjectPos object attatched to weapon)
	public Vector3 shellScale = new Vector3(1.0f, 1.0f, 1.0f);//scale of shell, can be used to make different shaped shells from one model
	public float shellEjectDelay = 0.0f;//delay before ejecting shell (used for bolt action rifles and pump shotguns)
	public float shellForce = 0.2f;//overall movement force of ejected shell
	public float shellUp = 0.75f;//random vertical direction to apply to shellForce
	public float shellSide = 1.0f;//random horizontal direction to apply to shellForce
	public float shellForward = 0.1f;//random forward direction to apply to shellForce
	public float shellRotateUp = 0.25f;//amount of vertical shell rotation
	public float shellRotateSide = 0.25f;//amount of horizontal shell rotation
	public int shellDuration = 5;//time in seconds that shells persist in the world before being removed
		
	//Particle Emitters
	//used for weapon fire and bullet impact effects
	public ParticleEmitter sparkParticles;
	public ParticleEmitter hitSpark;
	public ParticleEmitter tracerParticles;
	public ParticleEmitter slowSmokeParticles;
	public ParticleEmitter muzzleSmokeParticles;
	public ParticleEmitter fastSmokeParticles;
	public ParticleEmitter debrisParticles;
	public GameObject[] BulletMarkObj;
	
	//Audio Sources
	private AudioSource firefx;//use multiple audio sources to play weapon sfx without skipping
	private AudioSource otherfx;
	public AudioClip fireSnd;
	public AudioClip reloadSnd;
	public AudioClip reloadLastSnd;//usually shell reload sound + shotgun pump or rifle chambering sound
	public AudioClip noammoSnd;
	public AudioClip readySnd;
	public AudioClip[] hitSounds;//sound of bullets hitting surfaces
	
	void Start (){
	
		//define external script references
		PlayerWeapons PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		//Access GUIText instance that was created by the PlayerWeapons script
		ammoGuiObj = PlayerWeaponsComponent.ammoGuiObjInstance;
		
		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed){
			
			myTransform = transform;//define transform for efficiency
			
			if(meleeSwingDelay == 0){//initialize muzzle flash color if not a melee weapon
			    muzzleFlash.enabled = false;
			    muzzleFlashColor = muzzleFlash.renderer.material.GetColor("_TintColor");
				//clamp initial ammo amount in clip for non melee weapons
				bulletsLeft = Mathf.Clamp(bulletsLeft,0,bulletsPerClip);
			}else{
				//initial ammo amount in clip for melee weapons
				bulletsLeft = bulletsPerClip;	
			}
			
			if(semiAuto){//make muzzle flash fade out slower when gun is semiAuto
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 3.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}else{
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 6.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}
			
		    //initialize shot timers and animation settings
			shootStartTime = -1.0f;
		    shotSpreadAmt = shotSpread;
			
			animation["RifleSprinting"].speed = -1.5f;//init at this speed for correct rifle switching anim
			if(PistolSprintAnim){animation["PistolSprinting"].speed = -1.5f;}//init at this speed for correct pistol switching anim
			weaponMesh.animation["Fire"].speed = fireAnimSpeed;//initialize weapon mesh animation speeds
			weaponMesh.animation["Reload"].speed = reloadAnimSpeed;
			weaponMesh.animation["Ready"].speed = readyAnimSpeed;
			//If weapon reloads one bullet at a time, use anim called "Neutral" of hand returning to idle position
			//from reloading position to allow smooth anims when single bullet reloading is cancelled by sprinting.
			//The "Neutral" animation's wrap mode also needs to be set to "clamp forever" in the animation import settings. 
			if(bulletsToReload != bulletsPerClip){
				weaponMesh.animation["Neutral"].speed = 1.5f;
			}
	
			
			//limit ammo to maxAmmo value
			ammo = Mathf.Clamp(ammo, 0, maxAmmo);
			//limit bulletsToReload value to bulletsPerClip value
			bulletsToReload = Mathf.Clamp(bulletsToReload, 0, bulletsPerClip);
		}
		
	}
	
	void OnEnable () {
		
		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed){
			
			//define external script references
			PlayerWeapons PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
			Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
			
			if(Time.timeSinceLevelLoad > 2 && PlayerWeaponsComponent.switching){//don't ready weapon on level load, just when switching weapons
				AudioSource []aSources = weaponObj.GetComponents<AudioSource>();//Set up reference to Audio Sources using aSources array
			    AudioSource otherfx = aSources[1] as AudioSource;//Use first audio source for weapon sound effects
				
				StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
				IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
					
				//play weapon readying sound
				otherfx.volume = 1.0f;
				otherfx.pitch = 1.0f * Time.timeScale;
				otherfx.clip = readySnd;
				otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
				
				//track time that weapon was made active to calculate readyTime for syncing ready anim with weapon firing
				startTime = Time.time - Time.deltaTime;
				
				//play weapon readying animation after it has just been selected
				weaponMesh.animation["Ready"].speed = readyAnimSpeed;
				weaponMesh.animation.CrossFade("Ready",0.35f,PlayMode.StopAll);
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//FixedUpdate Actions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void FixedUpdate (){
	
		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			
			//define external script references
			AmmoText AmmoText = ammoGuiObj.GetComponent<AmmoText>();//set reference for main color element of ammo GUIText
			AmmoText[] AmmoText2 = ammoGuiObj.GetComponentsInChildren<AmmoText>();//set reference for shadow background color element of heath GUIText
			PlayerWeapons PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
			FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
			Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
			FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
				
			horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
			vertical = FPSWalkerComponent.inputY;
			
			AudioSource []aSources = weaponObj.GetComponents<AudioSource>();//Set up reference to Audio Sources using aSources array
		    AudioSource otherfx = aSources[1] as AudioSource;//Use first audio source for weapon sound effects
			
			//pass ammo amounts to the ammo GuiText object if not a melee weapon or unarmed
			if(meleeSwingDelay == 0 && !unarmed){
				//pass ammo amount to Gui object to be rendered on screen
			    AmmoText.ammoGui = bulletsLeft;//main color
				AmmoText.ammoGui2 = ammo;
				AmmoText2[1].ammoGui = bulletsLeft;//shadow background color
				AmmoText2[1].ammoGui2 = ammo;
				AmmoText.horizontalOffsetAmt = AmmoText.horizontalOffset;//normal position on screen
				AmmoText.verticalOffsetAmt = AmmoText.verticalOffset;
				AmmoText2[1].horizontalOffsetAmt = AmmoText2[1].horizontalOffset;
				AmmoText2[1].verticalOffsetAmt = AmmoText2[1].verticalOffset;
			}else{
				AmmoText.horizontalOffsetAmt = 5;//make ammo GUIText move off screen if using a melee weapon
				AmmoText.verticalOffsetAmt = 5;
				AmmoText2[1].horizontalOffsetAmt = 5;
				AmmoText2[1].verticalOffsetAmt = 5;	
			}
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed){
			    
			    //Determine if player is reloading last round during a non-magazine reload. 
				if(reloadLastStartTime + reloadLastTime > Time.time){
					lastReload = true;	
				}else{
					lastReload = false;		
				}
				
			    //cancel auto and manual reload if player starts sprinting
			    if(FPSWalkerComponent.sprintActive
				&& !Input.GetKey (FPSPlayerComponent.fire)
				&& !lastReload//allow player to finish chambering last round of a non-magazine reload
				&& !FPSWalkerComponent.cancelSprint
				&& (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0)){
			    	if(IronsightsComponent.reloading){
						IronsightsComponent.reloading = false;
						//use StopCoroutine to completely stop reload() function and prevent
						//"yield return new WaitForSeconds(reloadTime);" from continuing to excecute
						StopCoroutine("Reload");
						if(bulletsToReload != bulletsPerClip){bulletsReloaded = 0;}//reset bulletsReloaded value 
						
						if(bulletsToReload != bulletsPerClip){
							//rewind Neutral animation when sprinting
							weaponMesh.animation["Neutral"].speed = 1.5f;
							weaponMesh.animation.Play("Neutral", PlayMode.StopAll);//play reloading animation	
						}	
					
						//fast forward camera animations to stop playback if sprinting
						Camera.main.animation["CameraReloadMP5"].normalizedTime = 1.0f;
						Camera.main.animation["CameraReloadAK47"].normalizedTime = 1.0f;
						Camera.main.animation["CameraReloadPistol"].normalizedTime = 1.0f;
						Camera.main.animation["CameraReloadSingle"].normalizedTime = 1.0f;
						Camera.main.animation["CameraSwitch"].normalizedTime = 1.0f;
						//if sprint interrupts reload more than half-way through, just give bulletsNeeded
						if(bulletsToReload == bulletsPerClip && reloadStartTime + reloadTime / 2 < Time.time && !sprintReloadState){
							bulletsNeeded = bulletsPerClip - bulletsLeft;
							//we have ammo left to reload
							if(ammo >= bulletsNeeded){
								ammo -= bulletsNeeded;//subtract bullets needed from total ammo
								bulletsLeft = bulletsPerClip;//add bullets to magazine 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, so just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now
							}
							sprintReloadState = true;//only preform this action once at beginning of sprint/reload check
						}else{//if we are less than half way through reload before sprint interrupted, cancel reload
							//stop reload sound from playing
							otherfx.clip = null;
							if(bulletsToReload == bulletsPerClip){
								//rewind reload animation when sprinting
								weaponMesh.animation["Reload"].speed = -reloadAnimSpeed * 1.5f;
								weaponMesh.animation.CrossFade("Reload", 0.35f, PlayMode.StopAll);//play reloading animation
							}		
						}
					}
				}else{
					//Start automatic reload if player is out of ammo and firing time has elapsed to allow finishing of firing animation and sound
					if (bulletsLeft <= 0 
					&& shootStartTime + fireRate < Time.time 	 
					&& canShoot){
						if( ammo > 0 
						&& !IronsightsComponent.reloading 
						&& !PlayerWeaponsComponent.switching 
						&& ((startTime + readyTime) < Time.time)){
							StartCoroutine("Reload");
							//set animation speeds
							//make this check to prevent slow playing of non magazine anim for last bullet in inventory
							if(bulletsToReload == bulletsPerClip){
								weaponMesh.animation["Reload"].speed = reloadAnimSpeed;	
							}
							weaponMesh.animation["Ready"].speed = readyAnimSpeed;
						}
					}	
				}
				
				//don't spawn shell if player started sprinting to avoid unrealistic movement of shell if sprint stops
				if(FPSWalkerComponent.canRun){
					StopCoroutine("SpawnShell");	
				}
				
				//start reload if reload button is pressed
				if (Input.GetKey (FPSPlayerComponent.reload) 
				&& !IronsightsComponent.reloading 
				&& ammo > 0 
				&& bulletsLeft < bulletsPerClip
				&& shootStartTime + fireRate < Time.time
				&& !Input.GetKey (FPSPlayerComponent.fire)){
					StartCoroutine("Reload");
				}
					
				//enable/disable shooting based on various player states
				if (!FPSWalkerComponent.sprintActive
				||FPSWalkerComponent.crouched
				||(FPSPlayerComponent.zoomed && meleeSwingDelay == 0)
				||((Mathf.Abs(horizontal) > 0) && (Mathf.Abs(vertical) < 1))
				||FPSWalkerComponent.cancelSprint
				||(!FPSWalkerComponent.grounded && FPSWalkerComponent.jumping)//don't play sprinting anim while jumping
				||(FPSWalkerComponent.fallingDistance > 1.5f)//don't play sprinting anim while falling  
				||Input.GetKey (FPSPlayerComponent.fire)){
					//not sprinting
					//set sprint recovery timer so gun only shoots after returning to neutral
					if(!sprintState){
						recoveryTime = Time.time;
						sprintState = true;
					}
					canShoot = true;
					sprintReloadState = false;//reset sprintReloadState to allow another sprint reload cancel check
				}else{
					//sprinting
					if (Mathf.Abs(horizontal) != 0 || Mathf.Abs(vertical) > 0.75f){
						sprintState = false;
						if(IronsightsComponent.reloading){
							canShoot = false;
						}else{
							if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
								canShoot = true;
							}else{
								canShoot=false;
							}
						}
					}else{
						//set sprint recovery timer so gun only shoots after returning to center
						if(!sprintState){
							recoveryTime = Time.time;
							sprintState = true;
						}
						canShoot = true;
					}
				}
			
				//Play noammo sound
				if (Input.GetKey(FPSPlayerComponent.fire)){
					if((noAmmoState)
					&& (canShoot)
					&& (bulletsLeft <= 0)
					&& (ammo <= 0)
					&& ((!PistolSprintAnim && animation["RifleSprinting"].normalizedTime < 0.35f)//only play sound when weapon is centered
					 ||(PistolSprintAnim && animation["PistolSprinting"].normalizedTime < 0.35f))){
						otherfx.volume = 1.0f;
						otherfx.pitch = 1.0f;
						otherfx.clip = noammoSnd;
						otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
						shooting = false;
						noAmmoState = false;
					}
				}else{
					noAmmoState = true;
				}
				
				//Change fire mode
				if (Input.GetKey(FPSPlayerComponent.fireMode)){
					if(fireModeState
					&& canShoot
					&& !IronsightsComponent.reloading
					&& ((!PistolSprintAnim && animation["RifleSprinting"].normalizedTime < 0.35f)//only play sound when weapon is centered
					 ||(PistolSprintAnim && animation["PistolSprinting"].normalizedTime < 0.35f))){
						
						if(fireModeSelectable && semiAuto){
							
							semiAuto  = false;
							fireModeState = false;
							if(projectileCount < 2){//make muzzle flash last slightly longer for semiAuto
								muzzleFlashReduction = 6.5f;
							}else{
								muzzleFlashReduction = 2.0f;		
							}
							otherfx.volume = 1.0f;
							otherfx.pitch = 1.0f;
							otherfx.clip = noammoSnd;
							otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
							
						}else if(fireModeSelectable && !semiAuto){
							
							semiAuto  = true;
							fireModeState = false;
							if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
								muzzleFlashReduction = 3.5f;
							}else{
								muzzleFlashReduction = 2.0f;		
							}
							otherfx.volume = 1.0f;
							otherfx.pitch = 1.0f;
							otherfx.clip = noammoSnd;
							otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);	
						}
					}
				}else{
					fireModeState = true;
				}
			    
				//Run weapon sprinting animations
				if(canShoot
				||FPSWalkerComponent.crouched
				||FPSWalkerComponent.midPos < 0.9f//player is crouching
				||IronsightsComponent.reloading
				||FPSWalkerComponent.cancelSprint){
					if(sprintAnimState){//animate weapon up
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
		
						if(!PistolSprintAnim){
							//keep playback at last frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 1
							if(animation["RifleSprinting"].normalizedTime > 1){animation["RifleSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							animation["RifleSprinting"].speed = -1.4f;
							animation.CrossFade("RifleSprinting", 0.35f,PlayMode.StopAll);
						}else{
							if(animation["PistolSprinting"].normalizedTime > 1){animation["PistolSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							animation["PistolSprinting"].speed = -1.75f;
							animation.CrossFade("PistolSprinting", 2.25f,PlayMode.StopAll);	
						}
						//set sprintAnimState to false to only perform these actions once per change of sprinting state checks
						sprintAnimState = false;
					}
				}else{
					if(!sprintAnimState){//animate weapon down
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
						
						if(!PistolSprintAnim){
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(animation["RifleSprinting"].normalizedTime < 0){animation["RifleSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							animation["RifleSprinting"].speed = 1.4f;
							animation.CrossFade("RifleSprinting", 0.35f,PlayMode.StopAll);
						}else{
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(animation["PistolSprinting"].normalizedTime < 0){animation["PistolSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							animation["PistolSprinting"].speed = 1.75f;
							animation.CrossFade("PistolSprinting", 2.25f,PlayMode.StopAll);	
						}
						
						//set sprintAnimState to true to only perform these actions once per change of sprinting state checks
						sprintAnimState = true;
						//rewind reloading animation if reload is interrupted by sprint
						if(PlayerWeaponsComponent.switching && IronsightsComponent.reloading){
							weaponMesh.animation.CrossFade("Reload", 0.35f,PlayMode.StopAll);//play firing animation
						}
					}
				}
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Update Actions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Update (){	
		
		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed){
				FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
				//Fade out muzzle flash alpha 
				if (muzzleFlash.enabled){
					if(muzzleFlashColor.a > 0.0f){
						muzzleFlashColor.a -= muzzleFlashReduction * (Time.deltaTime);
						muzzleFlash.renderer.material.SetColor("_TintColor", muzzleFlashColor);
					}else{
						muzzleFlash.enabled = false;//disable muzzle flash object after alpha has faded
					}	
				}
				
				//Detect firemode (auto or semi auto) and call fire function
				if (Input.GetKey (FPSPlayerComponent.fire)){
					if(semiAuto){
						if(!semiState){
							Fire();
							semiState = true;
						}
					}else{
						Fire();
					}
				}else{
					semiState = false;
				}
				
				//set shooting var to false
				if(shootStartTime + fireRate > Time.time){
					shooting = false;	
				}
				
			}
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Weapon Muzzle Flash
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void MuzzFlash (){
		//enable muzzle flash
		if (muzzleFlash){
			//add random rotation to muzzle flash
			muzzleFlash.transform.localRotation = Quaternion.AngleAxis(Random.value * 360, Vector3.forward);
			muzzleFlash.enabled = true;
			//set muzzle flash color
			muzzleFlashColor.a = Random.Range(0.4f, 0.5f);
			//emit smoke particle effect from muzzle
			if (muzzleSmokeParticles) {
				muzzleSmokeParticles.transform.position = muzzleFlash.transform.position;
				muzzleSmokeParticles.Emit();
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Shell Ejection
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator SpawnShell (){

		if(shellEjectDelay > 0){//delay shell ejection for shotguns and bolt action rifles by shellEjectDelay amount
			yield return new WaitForSeconds(shellEjectDelay);
		}
		//instantiate shell object
		GameObject shell = Instantiate(shellPrefab,shellEjectPosition.position,shellEjectPosition.transform.rotation) as GameObject;
		shell.transform.localScale = shellScale;//scale size of shell object by shellScale amount
		//direction of ejected shell casing, adding random values to direction for realism
		shellEjectDirection = new Vector3((shellSide * 0.7f) + (shellSide * 0.4f * Random.value), 
									  	 (shellUp * 0.6f) + (shellUp * 0.5f * Random.value),
									  	 (shellForward * 0.4f) + (shellForward * 0.2f * Random.value));
		//Apply velocity to shell
		if(shell.rigidbody){
			shell.rigidbody.AddForce((transform.TransformDirection(shellEjectDirection) * shellForce / Time.timeScale), ForceMode.Impulse);
		}
		//Initialize object references for instantiated shell object
		shell.GetComponent<ShellEjection>().playerObj = playerObj;
		shell.GetComponent<ShellEjection>().gunObj = transform.gameObject;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set Up Fire Event
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Fire (){
			
		//do not proceed to fire if out of ammo, have already fired in semi-auto mode, or chambering last round
		if (bulletsLeft <= 0 || (semiAuto && semiState) || lastReload){
			return;
		}
		//only fire at fireRate value
		if(shootStartTime + fireRate > Time.time){ 
			return;	
		}
		
		//initialize script references and audio sources
		PlayerWeapons PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
		AudioSource []aSources = weaponObj.GetComponents<AudioSource>();
		AudioSource otherfx = aSources[1] as AudioSource;
	
		//fire weapon
		//don't allow fire button to interrupt a magazine reload
		if((bulletsToReload == bulletsPerClip && !IronsightsComponent.reloading)
		//allow normal firing when weapon does not reload by magazine
		|| (!IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft > 0)
		//allow fire button to interrupt a non-magazine reload if there are at least 2 shells loaded
		|| (IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft > 1 && reloadEndTime + reloadTime < Time.time)){
			if (canShoot && !PlayerWeaponsComponent.switching){//don't allow shooting when reloading, sprinting, or switching
					
				//make weapon recover faster from sprinting if using the pistol sprint anim 
				//because the gun/rifle style anims have more yaw movement and take longer to return to center
				if(!PistolSprintAnim){recoveryTimeAmt = 0.6f;}else{recoveryTimeAmt = 0.3f;}
				//reset bullets reloaded for non magazine reloading weapons
				if(bulletsToReload != bulletsPerClip){bulletsReloaded = 0;}
				//Check sprint recovery timer so gun only shoots after returning to center.
				//NOTE: If this is set before view rotation can return to neutral (too small a value) 
				//the view recoil while shooting just after sprinting will "twist" strangely 
				//for the first shot.
				if((recoveryTime + recoveryTimeAmt < Time.time) && (startTime + readyTime < Time.time)){
						
					StartCoroutine("FireOneShot");//fire bullet
					StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
					IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
					otherfx.clip = null;//stop playing reload sound effect if cancelling reload to fire
						
					if(meleeSwingDelay == 0){//eject shell and perform muzzle flash if not a melee weapon
						MuzzFlash();
						StartCoroutine("SpawnShell");
					}
					
					//track time that we started firing and keep fire rate as frame rate independent as possible
					shootStartTime = Time.time - (Time.deltaTime/20.0f);
					shooting = true;
				}
			}
		}
	
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Fire Projectile
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator FireOneShot (){
		//do not allow shooting when sprinting
	   	if (canShoot){
			CapsuleCollider capsule = playerObj.GetComponent<CapsuleCollider>();
			//Initialize audio source
			AudioSource []aSources = weaponObj.GetComponents<AudioSource>();
			AudioSource firefx = aSources[0] as AudioSource;
			firefx.clip = fireSnd;//play fire sound
			firefx.pitch = Random.Range(0.96f * Time.timeScale, 1 * Time.timeScale);//add slight random value to firing sound pitch for variety
			firefx.PlayOneShot(firefx.clip, 0.9f / firefx.volume);//play fire sound
				
			if(meleeSwingDelay == 0){//if this is not a melee weapon
				//rewind firing animation and set speed
				weaponMesh.animation.Rewind("Fire");
				weaponMesh.animation["Fire"].speed = fireAnimSpeed;
				weaponMesh.animation.CrossFade("Fire", 0.35f,PlayMode.StopAll);//play firing animation
				//make view recoil with shot
				KickBack();
				bulletsLeft -= 1;//subtract fired bullet from magazine amount
					
			}else{
		
				if(swingSide){//determine which side to swing melee weapon
					Camera.main.animation.Rewind("CameraMeleeSwingRight");//rewind camera swing animation 
					Camera.main.animation["CameraMeleeSwingRight"].speed = 1.7f;//set camera animation speed
					Camera.main.animation.CrossFade("CameraMeleeSwingRight", 0.35f,PlayMode.StopAll);//play camera view animation
					
					weaponMesh.animation["MeleeSwingRight"].speed = fireAnimSpeed;//set weapon swing animation speed
					weaponMesh.animation.Play("MeleeSwingRight",PlayMode.StopAll);//play weapon swing animation
						
					swingSide = false;//set swingSide to false to make next swing from other direction 
				}else{
					Camera.main.animation.Rewind("CameraMeleeSwingLeft");//rewind camera swing animation 
					Camera.main.animation["CameraMeleeSwingLeft"].speed = 1.6f;//set camera animation speed
					Camera.main.animation.CrossFade("CameraMeleeSwingLeft", 0.35f,PlayMode.StopAll);//play camera view animation
					
					weaponMesh.animation["MeleeSwingLeft"].speed = fireAnimSpeed;//set weapon swing animation speed
					weaponMesh.animation.Play("MeleeSwingLeft",PlayMode.StopAll);//play weapon swing animation
						
					swingSide = true;//set swingSide to true to make next swing from other direction 
				}
				//wait for the meleeSwingDelay amount while swinging forward before hitting anything
				yield return new WaitForSeconds(meleeSwingDelay);		
			}
			//fire the number of projectiles defined by projectileCount 
			for(float i = 0; i < projectileCount; i++){
				Vector3 direction = SprayDirection();
				RaycastHit hit;
				
				if(meleeSwingDelay == 0){
					//check for ranged weapon hit
					if(Physics.Raycast(Camera.main.transform.position, direction, out hit, range, bulletMask)){
						HitObject(hit, direction);
					}
				}else{
					//check for melee weapon hit
					//use SphereCast instead of Raycast to simulate swinging arc where melee weapon may contact objects
					if(Physics.SphereCast(Camera.main.transform.position, capsule.radius / 3, direction, out hit, range, bulletMask)){
						HitObject(hit, direction);
					}	
				}
			}
			return false;
		}
		
	}
	
	//weapon or projectile damage and effects for collider that is hit
	void HitObject ( RaycastHit hit, Vector3 direction ){
		// Apply a force to the rigidbody we hit
		if (hit.rigidbody){
			hit.rigidbody.AddForceAtPosition(force * direction, hit.point);
		}
		
		//Call the ApplyDamage function in the hit object			
		hit.collider.SendMessageUpwards("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
		
		//Play sounds of bullets hitting surface/ricocheting
		AudioSource.PlayClipAtPoint(hitSounds[Random.Range(0, hitSounds.Length)], hit.point, 0.75f);
		//Emit tracers for fired bullet
		if(meleeSwingDelay == 0){
			BulletTracers(direction);
		}
		BulletMarks(hit);//draw a bullet mark where the weapon hit
		ImpactEffects(hit);//draw impact effects where the weapon hit
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Calculate angle of bullet fire from muzzle
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	private Vector3 SprayDirection (){
		//Initialize script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		//increase weapon accuracy if player is crouched
		float crouchAccuracy = 1.0f;
		if(FPSWalkerComponent.crouched){
			crouchAccuracy = 0.75f;	
		}else{
			crouchAccuracy = 1.0f;	
		}
		//make firing more accurate when sights are raised and/or in semi auto
		if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread/5 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread/3 * crouchAccuracy;
			}
		}else{
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread/2 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread * crouchAccuracy;
			}
		}
		//apply accuracy spread amount to weapon facing angle
		float vx = (1 - 2 * Random.value) * shotSpreadAmt;
		float vy = (1 - 2 * Random.value) * shotSpreadAmt;
		float vz = 1.0f;
		return myTransform.TransformDirection(new Vector3(vx,vy,vz));
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Reload Weapon
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator Reload (){
		
		if(Time.timeSinceLevelLoad > 2){//prevent any unwanted reloading behavior at level start 
			//Initialize script references
			FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
			Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
			FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
			AudioSource []aSources = weaponObj.GetComponents<AudioSource>();//Initialize audio source
			AudioSource otherfx = aSources[1] as AudioSource;
	
			horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
			vertical = FPSWalkerComponent.inputY;
		
		    if(!(FPSWalkerComponent.sprintActive && (Mathf.Abs(horizontal) > 0.75f || Mathf.Abs(vertical) > 0.75f))//allow reload while walking
		     //allow auto reload when sprint button is held, even if stationary
			|| FPSWalkerComponent.cancelSprint){
				
				if(ammo > 0){//if player has no ammo in their inventory for this weapon, do not proceed with reload
					
					//cancel zooming when reloading
					FPSPlayerComponent.zoomed = false;
					
					//if loading by magazine, start these reloading actions immediately and wait for reloadTime before adding ammo and completing reload
					if(bulletsToReload == bulletsPerClip){
						//play reload sound once at start of reload
						otherfx.volume = 1.0f;
						otherfx.pitch = 1.0f * Time.timeScale;
						otherfx.clip = reloadSnd;
						otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);//play magazine reload sound effect
						
						//determine which weapon is selected and play camera view reloading animation
						if(!PistolSprintAnim){
							if(weaponNumber == 5 || weaponNumber == 6){
								//rewind animation if already playing to allow overlapping playback
								Camera.main.animation.Rewind("CameraReloadAK47");
								//set camera reload animation speed to positive value to play forward because
								//it might have been reversed if we canceled a reload by sprinting
								Camera.main.animation.animation["CameraReloadAK47"].speed = 1.0f;
								Camera.main.animation.CrossFade("CameraReloadAK47", 0.35f,PlayMode.StopAll);
							}else{
								Camera.main.animation.Rewind("CameraReloadMP5");
								Camera.main.animation.animation["CameraReloadMP5"].speed = 1.0f;
								Camera.main.animation.CrossFade("CameraReloadMP5", 0.35f,PlayMode.StopAll);
							}
						}else{
							Camera.main.animation.Rewind("CameraReloadPistol");
							Camera.main.animation.animation["CameraReloadPistol"].speed = 1.0f;
							Camera.main.animation.CrossFade("CameraReloadPistol", 0.35f,PlayMode.StopAll);
						}
						
						//Rewind reloading animation, set speed, and play animation. This can cause sudden/jerky start of reload anim
						//if sprinting very briefly, but is necessary to keep reload animation and sound synchronized.
						weaponMesh.animation.Rewind("Reload");
						weaponMesh.animation["Reload"].speed = reloadAnimSpeed;
						weaponMesh.animation.CrossFade("Reload", 0.35f,PlayMode.StopAll);//play reloading animation
					}
					
					//set reloading var in ironsights script to true
					IronsightsComponent.reloading = true;
					reloadStartTime = Time.time;
					//do not wait for reloadTime if this is not a magazine reload and this is the first bullet/shell to be loaded,
					//otherwise, adding of ammo and finishing reload will wait for reloadTime while animation and sound plays
					if((bulletsToReload != bulletsPerClip && bulletsReloaded > 0) || bulletsToReload == bulletsPerClip){
						// Wait for reload time first, then proceed
						yield return new WaitForSeconds(reloadTime);
					}
					
					//determine how many bullets need to be reloaded
					bulletsNeeded = bulletsPerClip - bulletsLeft;	
					
					//if loading a magazine, update bullet amount and set reloading var to false after reloadTime has elapsed
					if(bulletsToReload == bulletsPerClip){
							
						//set reloading var in ironsights script to false after reloadTime has elapsed
						IronsightsComponent.reloading = false;
				
						//we have ammo left to reload
						if(ammo >= bulletsNeeded){
							ammo -= bulletsNeeded;//subtract bullets needed from total ammo
							bulletsLeft = bulletsPerClip;//add bullets to magazine 
						}else{
							bulletsLeft += ammo;//if ammo left is less than needed to reload, so just load all remaining bullets
							ammo = 0;//out of ammo for this weapon now
						}
							
					}else{//If we are reloading weapon one bullet at a time (or bulletsToReload is less than the magazine amount) run code below
						//determine if bulletsToReload var needs to be changed based on how many bullets need to be loaded						
						if(bulletsNeeded >= bulletsToReload){//bullets needed are more or equal to bulletsToReload amount, so add bulletsToReload amount
							if(ammo >= bulletsToReload){
								bulletsLeft += bulletsToReload;//add bulletsToReload amount to magazine
								ammo -= bulletsToReload;//subtract bullets needed from total ammo
								bulletsReloaded ++;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now
							}
						}else{//if bullets needed are less than bulletsToReload amount, just add the ammo that is needed
							if(ammo >= bulletsNeeded){
								bulletsLeft += bulletsNeeded;	
								ammo -= bulletsNeeded;//subtract bullets needed from total ammo
								bulletsReloaded ++;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now	
							}
						}
							
						if(bulletsNeeded > 0){//if bullets still need to be reloaded and we are not loading a magazine
							StartCoroutine("Reload");//start reload coroutine again to load number of bullets defined by bulletsToReload amount			
						}else{
							IronsightsComponent.reloading = false;//if magazine is full, set reloading var in ironsights script to false
							bulletsReloaded = 0;
							return false;//also stop coroutine here to prevent sound from playing below
						}
							
						if(bulletsNeeded == bulletsToReload || ammo <= 0){//if reloading last round, play normal reloading sound and also chambering effect
							otherfx.clip = reloadLastSnd;//set otherfx audio clip to reloadLastSnd
							weaponMesh.animation["Reload"].speed = 1.0f;
							//track time we started reloading last bullet to allow for additional time to chamber round before allowing weapon firing		
							reloadLastStartTime = Time.time;
							IronsightsComponent.reloading = false;
						}else{
							otherfx.clip = reloadSnd;//set otherfx audio clip to reloadSnd
							weaponMesh.animation["Reload"].speed = shellRldAnimSpeed;
	
						}
						
						//play reloading sound effect	
						otherfx.volume = 1.0f;
						otherfx.pitch = Random.Range(0.95f * Time.timeScale, 1 * Time.timeScale);
						otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
						//play reloading animation
						weaponMesh.animation.Rewind("Reload");
						weaponMesh.animation.CrossFade("Reload", 0.35f,PlayMode.StopAll);
							
						//play camera reload animation 
						Camera.main.animation.Rewind("CameraReloadSingle");
						//set camera reload animation speed to positive value to play forward because
						//it might have been reversed if we canceled a reload by sprinting
						Camera.main.animation.animation["CameraReloadSingle"].speed = 1.0f;
						Camera.main.animation.CrossFade("CameraReloadSingle", 0.35f,PlayMode.StopAll);
							
						reloadEndTime = Time.time;//track time that we finished reload to determine if this reload can be interrupted by fire button
						
					}	
				}
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Draw Bullet Tracers
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void BulletTracers ( Vector3 direction ){
		//Draw Bullet Tracers
		if (tracerParticles) {
			//Set tracer origin to a small amount forward of the end of gun barrel (muzzle flash position)
			tracerParticles.transform.position = muzzleFlash.transform.position + muzzleFlash.transform.forward * 0.5f;
			//add shotSpray/accuracy value to straight-forward rotation to make tracers follow raycast to hit position
			tracerParticles.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
			//emit tracer particle for every shot fired
			tracerParticles.Emit();
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Draw Impact Effects
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void ImpactEffects ( RaycastHit hit  ){
		//draw bullet impact effects
		if (hitSpark){//bright, circular sparks around hit location with very short duration
			hitSpark.transform.position = hit.point + (hit.normal * 0.075f);//align emitter position with contact position and move up from surface slightly
			hitSpark.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);//rotate impact effects so they are perpendicular to surface hit
			hitSpark.Emit();//emit the particle(s)
		}
		if (sparkParticles){//fast, bright sparks that bounce against world colliders 
			sparkParticles.transform.position = hit.point;
			sparkParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			sparkParticles.Emit();
		}
		if (slowSmokeParticles && meleeSwingDelay == 0) {//large puff of smoke that moves upwards slowly and lingers 
			slowSmokeParticles.transform.position = hit.point;
			slowSmokeParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			slowSmokeParticles.Emit();
		}
		if(meleeSwingDelay == 0){
			if (fastSmokeParticles) {//medium size smoke puff that quickly moves upwards from impact point and dissapates  
				fastSmokeParticles.transform.position = hit.point;
				fastSmokeParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
				fastSmokeParticles.Emit();
			}
		}
		if (debrisParticles){//opaque debris that emit from impact point and bounce against world colliders
			debrisParticles.transform.position = hit.point;
			debrisParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			debrisParticles.Emit();
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Draw Bullet Marks
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void BulletMarks ( RaycastHit hit  ){
		if(hit.collider//check only objects with colliders attatched to prevent null reference error
		  && hit.collider.gameObject.layer != 9//don't leave marks on ragdolls
		  && hit.collider.gameObject.tag != "NoHitMark"//don't leave marks on active NPCs or objects with NoHitMark or PickUp tag
		  && hit.collider.gameObject.tag != "PickUp"){
			//create an instance of the bullet mark and place it parallel and slightly above the hit surface to prevent z buffer fighting
			GameObject clone = Instantiate(BulletMarkObj[Random.Range(0, BulletMarkObj.Length)], hit.point + (hit.normal * 0.025f), Quaternion.FromToRotation(Vector3.up, hit.normal)) as GameObject; 	
			//create empty game object for parent of bullet mark to prevent bullet mark object from inheriting hit object's scale
			//we do this to create another layer between the bullet mark object and the hit object which may have been unevenly scaled in editor
			var emptyObject = new GameObject();
			//define transforms for efficiency
			Transform tempObjTransform = emptyObject.transform;
			Transform cloneTransform = clone.transform;
			//save initial scaling of bullet mark prefab object
			Vector3 scale = cloneTransform.localScale;		
			//set parent of empty game object to hit object's transform
			tempObjTransform.parent = hit.transform;
			//set scale of empty game object to (1,1,1) to prepare it for applying the inverse scale of the object that was hit
			tempObjTransform.localScale = Vector3.one;
			//sync empty game object's rotation quaternion with hit object's quaternion for correct scaling of euler angles (use the same orientation of axes)
		    Quaternion tempQuat = hit.transform.rotation;
			tempObjTransform.rotation = tempQuat;
			//calculate inverse of hit object's scale to compensate for objects that have been unevenly scaled in editor
			Vector3 tempScale1 = new Vector3(1.0f / tempObjTransform.parent.transform.localScale.x, 
									  	     1.0f / tempObjTransform.parent.transform.localScale.y, 
				  						     1.0f / tempObjTransform.parent.transform.localScale.z);
			//apply inverse scale of the collider that was hit to empty game object's transform
			tempObjTransform.localScale = tempScale1;
			//set parent of bullet mark object to empy game object and set localScale to (1,1,1)
			cloneTransform.parent = null;
			cloneTransform.parent = tempObjTransform;
			//apply hit mark's initial scale to hit mark instance
			cloneTransform.localScale = scale;
			//randomly scale bullet marks slightly for more natural visual effect
			if(meleeSwingDelay == 0){//not a melee weapon
				float tempScale = Random.Range (-0.25f, 0.25f);//find random scale amount
				cloneTransform.localScale = scale + new Vector3(tempScale, 0, tempScale);//apply random scale to bullet mark object's localScale
			}
			//rotate hit mark randomly for more variation
			cloneTransform.RotateAround(hit.point, hit.normal, Random.Range (-50, 50));
			//destroy bullet mark instance after a time
			Destroy(clone.gameObject, 30); 
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Camera Recoil Kick
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void KickBack (){
		//Initialize script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();	
		FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		//make recoil less when zoomed in and more when zoomed out
		if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
			kickUpAmt = kickUp;//set kick amounts to those set in the editor
			kickSideAmt = kickSide;
		}else{
			if(!FPSWalkerComponent.crouched
			//normal view kick when crouching and not moving
			||(FPSWalkerComponent.crouched && Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)){
				kickUpAmt = kickUp * 1.75f;
				kickSideAmt = kickSide * 1.75f;
			}else{
				//increase view kick to offset increased bobbing 
				//amounts when crouching and moving 
				kickUpAmt = kickUp * 2.75f;
				kickSideAmt = kickSide * 2.75f;
			}
		}
		//Set rotation quaternion to random kick values
		kickRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles - new Vector3(kickUpAmt * 2, Random.Range(-kickSideAmt * 2, kickSideAmt * 2), 0));
		//smooth current camera angles to recoil kick up angles using Slerp
		Camera.main.transform.localRotation = Quaternion.Slerp(myTransform.localRotation, kickRotation, 0.1f);
	}
	
}