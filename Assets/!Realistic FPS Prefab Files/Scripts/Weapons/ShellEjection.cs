//ShellEjection.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections.Generic;
//shell ejection script
public class ShellEjection : MonoBehaviour
{
	//objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject gunObj;
	private Transform myTransform;
	private Transform playerObjTransform;
	private Transform FPSMainTransform;
	public List<AudioClip> shellSounds = new List<AudioClip>();//shell bounce sounds
	//shell states and settings
	private bool parentState = true;
	private bool soundState = true;
	private Vector3 initialScale = Vector3.zero;
	//shell rotation
	private float rotateAmt = 0.0f;//amount that the shell rotates, scaled up after ejection
	[HideInInspector]
	public float shellRotateUp = 0.0f;//amount of vertical shell rotation
	[HideInInspector]
	public float shellRotateSide = 0.0f;//amount of horizontal shell rotation	
	//timers and shell lifetime duration
	private float shellRemovalTime = 0.0f;//time that this shell will be removed from the level
	[HideInInspector]
	public int shellDuration = 0;//time in seconds that shells persist in the world before being removed	
	private float startTime = 0.0f;//time that the shell instance was created in the world

	void Start(){
		//set up external script references
		WeaponBehavior WeaponBehaviorComponent = gunObj.GetComponent<WeaponBehavior>();
		myTransform = transform;//manually set transform for efficiency
		playerObjTransform = playerObj.transform.parent.transform;
		FPSMainTransform = playerObj.transform.parent.transform;
		//initialize shell rotation amounts
		shellRotateUp = WeaponBehaviorComponent.shellRotateUp;
		shellRotateSide = WeaponBehaviorComponent.shellRotateSide;
		shellDuration = WeaponBehaviorComponent.shellDuration;
		//track the time that the shell was ejected
		startTime = Time.time;
		//set initial parent to gun object to inherit player velocity 
		myTransform.parent = gunObj.transform;	
		shellRemovalTime = Time.time + shellDuration;//time that shell will be removed
		rigidbody.maxAngularVelocity = 100;//allow shells to spin faster than default
		//determine if shell rotates clockwise or counter-clockwise at random
		if(Random.value < 0.5f){shellRotateUp *= -1;} 
		initialScale = myTransform.localScale;
	}

	void Update(){
		if(Time.time > shellRemovalTime){
			Object.Destroy(gameObject);
		}

	}
	
	void FixedUpdate(){
		//set up external script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		PlayerWeapons PlayerWeaponsComponent = gunObj.transform.parent.GetComponent<PlayerWeapons>();
		
		//don't add rotation until a short time after shell is ejected for visual effect and stop adding torque after a time
		if(startTime + Time.fixedDeltaTime > Time.time){
			//gradually increase rotation amount for smooth rotation transition
			rotateAmt = 0.1f;
			//apply torque to rigidbody
			rigidbody.AddRelativeTorque(Vector3.up * (Random.Range (rotateAmt * 1.75f,rotateAmt) * shellRotateSide));
			rigidbody.AddRelativeTorque(Vector3.right * (Random.Range (rotateAmt * 4,rotateAmt * 6) * shellRotateUp));
		}
		
		//Check if the player is on a moving platform to determine how to handle shell parenting and velocity
		if(FPSMainTransform == FPSWalkerComponent.initialParent){//if player is not on a moving platform
			//Make the shell's parent the weapon object for a short time after ejection
			//to the link shell ejection position with weapon object for more consistent movement,
			if(((startTime + (0.35f / Time.timeScale) < Time.time && parentState) 
			//don't parent shell if switching weapon
			|| (PlayerWeaponsComponent.switching && parentState)
			//don't parent shell if moving weapon to sprinting position
			|| (FPSWalkerComponent.sprintActive && !FPSWalkerComponent.cancelSprint && parentState))
			&& FPSWalkerComponent.grounded){
				Vector3 tempVelocity = playerObj.transform.rigidbody.velocity;
				tempVelocity.y = 0.0f;
				myTransform.parent = null;
				//add player velocity to shell when unparenting from player object to prevent shell from suddenly changing direction
				if(!FPSWalkerComponent.sprintActive && !FPSWalkerComponent.canRun){//don't inherit parent velocity if sprinting to prevent visual glitches
					rigidbody.AddForce(tempVelocity, ForceMode.VelocityChange);
				}
				parentState = false;
			}
		}else{//if player is on elevator, keep gun object as parent for a longer time to prevent strange shell movements
			if(startTime + 0.5f < Time.time && parentState){
				myTransform.localScale = initialScale;
				myTransform.parent = null;
				//add player velocity to shell when unparenting from player object to prevent shell from suddenly changing direction
				rigidbody.AddForce(FPSMainTransform.rigidbody.velocity, ForceMode.VelocityChange);	
			}		
		}	
	}

	void OnCollisionEnter(Collision collision){
		//set up external script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		//play a bounce sound when shell object collides with a surface
		if(soundState){
			if (shellSounds.Count > 0){
				AudioSource.PlayClipAtPoint(shellSounds[(int)Random.Range(0, (shellSounds.Count))], myTransform.position, 0.75f);
			}
			soundState = false;
		}
		//remove shells if they collide with a moving object like an elevator or are ejected when player is on moving platform
		if(collision.gameObject.tag == "PhysicsObject" || playerObjTransform != FPSWalkerComponent.initialParent){
			Object.Destroy(gameObject);
		}
	}

}


	