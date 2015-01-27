//CameraKick.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class CameraKick : MonoBehaviour {
	//other objects accessed by this script
	[HideInInspector]
	public GameObject gun;//this variable updated by PlayerWeapons script
	[HideInInspector]
	public GameObject playerObj;
	private Transform myTransform;
	//camera angles
	[HideInInspector]
	public float CameraYawAmt = 0.0f;//this value is modified by animations and added to camera angles
	[HideInInspector]
	public float CameraPitchAmt = 0.0f;//this value is modified by animations and added to camera angles
	[HideInInspector]
	public float CameraRollAmt = 0.0f;//this value is modified by animations and added to camera angles
	[HideInInspector]
	public Vector3 bobAngles = new Vector3(0,0,0);//view bobbing angles are sent here from the HeadBob script
	private float returnSpeed = 4.0f;//speed that camera angles return to neutral
	//to move gun and view down slightly on contact with ground
	private bool  landState = false;
	private float landStartTime = 0.0f;
	private float landElapsedTime = 0.0f;
	private float landTime = 0.35f;
	private float landAmt = 20.0f;
	private float landValue = 0.0f;
	//weapon position
	private float gunDown = 0.0f;
	[HideInInspector]
	public float dampOriginX = 0.0f;//Player X position is passed from the GunBob script
	[HideInInspector]
	public float dampOriginY = 0.0f;//Player Y position is passed from the HeadBob script

	void Start (){
		myTransform = transform;//store this object's transform for optimization
	}
	
	void Update (){
		//define external script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		Ironsights IronsightsComponent = playerObj.GetComponent<Ironsights>();
		FPSPlayer FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
     
		//side to side bobbing/moving of camera (stored in the dampOriginX) needs to added to the right vector
		//of the transform so that the bobbing amount is correctly applied along the X and Z axis.
		//If added just to the x axis, like done with the vertical Y axis, the bobbing does not rotate
		//with camera/mouselook and only moves on the world axis.  
		Vector3 tempPosition = playerObj.transform.position + (playerObj.transform.right * dampOriginX) + new Vector3(0.0f, dampOriginY, 0.0f);
		Camera.main.transform.parent.transform.position = tempPosition;
		Camera.main.transform.position = tempPosition;
		
		//initialize camera position quickly before fade out on level load
		if(Time.timeSinceLevelLoad < 1){returnSpeed = 32.0f;}else{returnSpeed = 4.0f;};
		//apply a force to the camera that returns it to neutral angles (Quaternion.identity) 
		//over time after being changed by code or by animations
		myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, Quaternion.identity, Time.smoothDeltaTime * returnSpeed);
		
		//store camera angles in temporary vector and add yaw and pitch from animations 
		Vector3 tempCamAngles = new Vector3(Camera.main.transform.localEulerAngles.x - bobAngles.x + CameraPitchAmt* Time.deltaTime * 75.0f ,
										Camera.main.transform.localEulerAngles.y + CameraYawAmt * Time.deltaTime * 75.0f,
										Camera.main.transform.localEulerAngles.z - bobAngles.z + CameraRollAmt* Time.deltaTime * 75.0f); 
		
		//apply tempCamAngles to camera angles
		Camera.main.transform.localEulerAngles = tempCamAngles;
		
		//Track time that player has landed from jump or fall for gun kicks
		landElapsedTime = Time.time - landStartTime;
		
		if(FPSWalkerComponent.fallingDistance < 1.25f && !FPSWalkerComponent.jumping){
			if(!landState){
				//init timer amount
				landStartTime = Time.time;
				//set landState only once
				landState = true;
			}
		}else{
			if(landState){
				//if recoil time has elapsed
				if(landElapsedTime >= landTime){ 
					//reset shootState
					landState = false;
				}
			}
		}
	
		//perform jump of gun when landing
		if(landElapsedTime < landTime){
			//only rise for a third of landing time for quick rising and slower lowering
			if(landElapsedTime > landTime / 2.0f){//move up view and gun
				landValue += landAmt * Time.deltaTime;
			}else{//for remaining 2 thirds of landing time, move down view and gun
				landValue -= landAmt* Time.deltaTime;
			}
		}else{
			//reset vars
			landValue = 0.0F;
		}
	
		//make landing kick less when zoomed
		if (!FPSPlayerComponent.zoomed){gunDown = landValue / 96.0f;}else{gunDown = landValue / 192.0f;}
		//pass value of gun kick to IronSights script where it will be added to gun position
		IronsightsComponent.jumpAmt = gunDown;
	}
	
}