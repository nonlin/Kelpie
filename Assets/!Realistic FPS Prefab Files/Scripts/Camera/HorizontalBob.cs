//HorizontalBob.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HorizontalBob : MonoBehaviour {
	[HideInInspector]
	public GameObject playerObj;
	//variables for horizontal sine bob of camera and weapons
	private float timer = 0.0f;
	[HideInInspector]
	public float bobbingSpeed = 0.0f;
	[HideInInspector]
	public float bobbingAmount = 0.0f;
	[HideInInspector]
	public float translateChange = 0.0f;
	[HideInInspector]
	public float waveslice = 0.0f;
	private float dampVelocity = 0.0f;
	private float totalAxes;
	[HideInInspector]
	public float dampOrg = 0.0f;//Smoothed view postion to be passed to CameraKick script
	private float dampTo = 0.0f;
	
	void Update (){
		//set up external script references
		FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		CameraKick CameraKickComponent = Camera.main.GetComponent<CameraKick>();
	
		waveslice = 0.0f;
		float horizontal = FPSWalker.inputX;//get input from player movement script
		float vertical = FPSWalker.inputY;
	
		if (Mathf.Abs(horizontal) != 0 || Mathf.Abs(vertical) != 0){//perform bob only when moving
			waveslice = Mathf.Sin(timer);
			timer = timer + bobbingSpeed * Time.deltaTime;
			if (timer > Mathf.PI * 2.0f) {
			  timer = timer - (Mathf.PI * 2.0f);
			}
		}else{
			timer = 0.0f;//reset timer when stationary to start bob cycle from neutral position
		}
	
		if (waveslice != 0){
		   translateChange = waveslice * bobbingAmount;
		   totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
		   totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f);
		   translateChange = totalAxes * translateChange;
			//set position for smoothing function
			dampTo = translateChange / Time.deltaTime * 0.01f;//divide position by deltaTime for framerate independence
		}else{
			//reset variables to prevent view twitching when falling
			dampTo = 0.0f;
			totalAxes = 0.0f;
			translateChange = 0.0f;
		}
		//use SmoothDamp to smooth position and remove any small glitches in bob amount 
		dampOrg = Mathf.SmoothDamp(dampOrg, dampTo, ref dampVelocity, 0.1f, Mathf.Infinity, Time.deltaTime);
		//pass bobbing amount to the camera kick script in the camera object after smoothing
		CameraKickComponent.dampOriginX = dampOrg;
	}
}