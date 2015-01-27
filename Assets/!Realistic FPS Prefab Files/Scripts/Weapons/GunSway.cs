//GunSway.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class GunSway : MonoBehaviour {
	//other objects accessed by this script
	[HideInInspector]
	public GameObject cameraObj;
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject weaponObj;
	private Transform myTransform;
	//velocities for smoothing functions
	private float dampSpeed = 0.001f;
	private float dampVelocity1 = 0.0f;
	private float dampVelocity2 = 0.0f;
	private float dampVelocity6 = 0.0f;
	//angle target vectors
	private Vector3 targetRotation;
	private Vector3 targetRotation2;
	private float targetRotationRoll = 0.0f;
	//to manage Z axis
	private float zAxis1 = 0.0f;
	private float zAxis2 = 0.0f;
	//passed on to ironsights script to sway gun
	private float localSide = 0.0f;
	private float localRaise = 0.0f;
	private float swingAmt = 0.035f;
	private float swingSpeed = 9.0f;
	//rolling of weapon
	private float localRoll = 0.0f;
	private float rollSpeed = 0.0f;
	//set amounts of gun angle bobbing
	private float gunBobRoll = 10.0f;
	private float gunBobYaw = 16.0f;
	//vars to change swaying and bobbing amounts from the inspector
	public float swayAmount = 1.0f;
	public float rollSwayAmount = 1.0f;
	public float walkBobYawAmount = 1.0f;
	public float walkBobRollAmount = 1.0f;
	public float sprintBobYawAmount = 1.0f;
	public float sprintBobRollAmount = 1.0f;
	
	void Start (){
		myTransform = transform;//define transform for efficiency
		//initialize bobbing amounts 
		walkBobYawAmount = Mathf.Clamp01(walkBobYawAmount);
		walkBobRollAmount = Mathf.Clamp01(walkBobRollAmount);
		sprintBobYawAmount = Mathf.Clamp01(sprintBobYawAmount);
		sprintBobRollAmount = Mathf.Clamp01(sprintBobRollAmount);
		gunBobRoll *= walkBobRollAmount;
		gunBobYaw *= walkBobYawAmount;
	}
	
	void Update (){
			
		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			
			//set up external script references
			FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
			Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
			WeaponBehavior WeaponBehaviorComponent = IronsightsComponent.gunObj.GetComponent<WeaponBehavior>();
			HorizontalBob HorizontalBob = playerObj.GetComponent<HorizontalBob>();
			FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		
			if(FPSWalkerComponent.canRun){//sprinting
				//set sway amounts for sprinting
				swingAmt = 0.02f * swayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				swingSpeed = 0.000025f * swayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				rollSpeed = 0.0f * rollSwayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				//smoothly change bobbing amounts for sprinting
				if(gunBobYaw < 12.0f * sprintBobYawAmount){gunBobYaw += 60.0f * Time.deltaTime;}
				if(gunBobRoll < 15.0f * sprintBobRollAmount){gunBobRoll += 60.0f * Time.deltaTime;}
				
			}else{//walking
				//smoothly change bobbing amounts for walking
				if(gunBobYaw > -16.0f * walkBobYawAmount){gunBobYaw -= 60.0f  *Time.deltaTime;}
				if(gunBobRoll > 10.0f * walkBobRollAmount){gunBobRoll -= 60.0f * Time.deltaTime;}
				//set sway amounts based on player state
				if(!FPSPlayerComponent.zoomed || IronsightsComponent.reloading || WeaponBehaviorComponent.meleeSwingDelay != 0){
					swingAmt = 0.035f * swayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
					swingSpeed = 0.00015f * swayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
					rollSpeed = 0.025f * rollSwayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				}else{
					swingAmt = 0.025f * swayAmount * WeaponBehaviorComponent.swayAmountZoomed;
					swingSpeed = 0.0001f * swayAmount * WeaponBehaviorComponent.swayAmountZoomed;
					rollSpeed = 0.075f * rollSwayAmount * WeaponBehaviorComponent.swayAmountZoomed;
				}
			}
			
			//use both these z axes for independent animation of camera X and Y position and bobbing of camera roll 
			zAxis1 = Camera.main.transform.localEulerAngles.z;
			zAxis2 = cameraObj.transform.localEulerAngles.z;
		    //Set target angles to main camera angles
		    targetRotation.x = cameraObj.transform.localEulerAngles.x;
		    targetRotation.y = cameraObj.transform.localEulerAngles.y;
			targetRotation.z = Mathf.MoveTowardsAngle(zAxis1,zAxis2,Time.deltaTime / 16);
			
			//sway gun
			//find angle delta and reduce to smaller value
			localSide = Mathf.DeltaAngle(targetRotation2.y,targetRotation.y) * -(swingSpeed / Time.deltaTime);
			//pass sway value to ironsights script where gun position is calculated
			IronsightsComponent.side = Mathf.Clamp(localSide, -swingAmt, swingAmt);
			
			//find angle delta and reduce to smaller value
			localRaise = Mathf.DeltaAngle(targetRotation2.x,targetRotation.x) * (swingSpeed / Time.deltaTime);
			//pass sway value to ironsights script where gun position is calculated
			IronsightsComponent.raise = Mathf.Clamp(localRaise, -swingAmt, swingAmt);
			
			//calculate roll swaying of weapon and smooth with lerpAngle to minimize snapping of gun during extreme rotations
			localRoll = Mathf.LerpAngle(localRoll, Mathf.DeltaAngle(targetRotationRoll,targetRotation.y)* -rollSpeed * 3.0f, Time.deltaTime * 5.0f);
			
			//dampen angles of gun with main camera angles for swaying effects
			targetRotation2.x = Mathf.SmoothDampAngle(targetRotation2.x, targetRotation.x, ref dampVelocity1, dampSpeed, Mathf.Infinity, Time.deltaTime);
			targetRotation2.y = Mathf.SmoothDampAngle(targetRotation2.y, targetRotation.y, ref dampVelocity2, dampSpeed, Mathf.Infinity, Time.deltaTime);
			targetRotationRoll = Mathf.SmoothDampAngle(targetRotationRoll, targetRotation.y, ref dampVelocity6, 0.075f, Mathf.Infinity, Time.deltaTime);
			
			//set gun angles
			//set temporary vector to smoothed angles of gun and add yaw and roll bobbing and roll swaying
			Vector3 tempEulerAngles = new Vector3(
				(targetRotation.x),
				(targetRotation.y + (HorizontalBob.dampOrg * gunBobYaw)),
				(targetRotation.z + localRoll + (HorizontalBob.dampOrg * -gunBobRoll))
			);
			//finally, set actual weapon angles to the temporary vector's angles
			myTransform.localEulerAngles = tempEulerAngles;
		}
	}
	
}