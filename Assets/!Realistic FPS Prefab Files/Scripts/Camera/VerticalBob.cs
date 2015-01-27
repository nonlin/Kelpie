//VerticalBob.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class VerticalBob : MonoBehaviour {
	[HideInInspector]
	public GameObject playerObj;
	//Variables for vertical aspect of sine bob of camera and weapons
	//This script also makes camera view roll and pitch with bobbing
	private float timer = 0.0f;
	private float timerRoll = 0.0f;
	[HideInInspector]
	public float bobbingSpeed = 0.0f;
	//Vars for smoothing view position
	private float dampOrg = 0.0f;//Smoothed view postion to be passed to CameraKick script
	private float dampTo = 0.0f;
	private Vector3 tempLocalEulerAngles = new Vector3(0,0,0);
	//These two vars controlled from ironsights script
	//to allow different values for sprinting/walking ect.
	[HideInInspector]
	public float bobbingAmount = 0.0f;
	[HideInInspector]
	public float rollingAmount = 0.0f;
	[HideInInspector]
	public float pitchingAmount = 0.0f;
	private float midpoint = 0.0f;//Vertical position of camera during sine bobbing
	[HideInInspector]
	public float translateChange = 0.0f;
	private float translateChangeRoll = 0.0f;
	private float translateChangePitch = 0.0f;
	private float waveslice = 0.0f;
	private float wavesliceRoll = 0.0f;
	private float dampVelocity = 0.0f;
	private Vector3 dampVelocity2;
	private float totalAxes;
	//Player movement sounds
	public AudioClip[] walkSounds;
	//public AudioClip[] runSounds;
	public AudioClip[] climbSounds;

	void Update (){
		//Set up external script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		CameraKick CameraKickComponent = Camera.main.GetComponent<CameraKick>();
	
		waveslice = 0.0f;
		float horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
		float vertical = FPSWalkerComponent.inputY;
		midpoint = FPSWalkerComponent.midPos;//Start bob from view position set in player movement script
	
		if (Mathf.Abs(horizontal) != 0 || Mathf.Abs(vertical) != 0){//Perform bob only when moving
	
			waveslice = Mathf.Sin(timer);
			wavesliceRoll = Mathf.Sin(timerRoll);
				   
			timer = timer + bobbingSpeed * Time.deltaTime;
			timerRoll = timerRoll + (bobbingSpeed / 2.0f) * Time.deltaTime;//Make view roll bob half the speed of view pitch bob
			
			if (timer > Mathf.PI * 2.0f){
				timer = timer - (Mathf.PI * 2.0f);
				if(!FPSWalkerComponent.climbing){
					//Make a short delay before playing footstep sounds to allow landing sound to play
					if (FPSWalkerComponent.grounded && (FPSWalkerComponent.landStartTime + 0.4f) < Time.time){
						audio.clip = walkSounds[Random.Range(0, walkSounds.Length)];//Select a random footstep sound from walkSounds array
						audio.volume = 0.5f;
						audio.Play();
					}
				}else{
					audio.clip = climbSounds[Random.Range(0, climbSounds.Length)];//Select a random climb sound from climbSounds array
					audio.volume = 1.0f;
					audio.Play();
				}
			}
			
			//Perform bobbing of camera roll
			if (timerRoll > Mathf.PI * 2){
				timerRoll = (timerRoll - (Mathf.PI * 2));
				if (!FPSWalkerComponent.grounded){
					timerRoll = 0;//reset timer when airborne to allow soonest resume of footstep sfx
				}
			}
		   
		}else{
			//reset variables to prevent view twitching when falling
			timer = 0.0f;
			timerRoll = 0.0f;
			tempLocalEulerAngles = new Vector3(0,0,0);//reset camera angles to 0 when stationary
		}
	
		if (waveslice != 0){
			translateChange = waveslice * bobbingAmount;
			translateChangePitch = waveslice * pitchingAmount;
			translateChangeRoll = wavesliceRoll * rollingAmount;
			totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
			totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f);
			//needed for smooth return to neutral view pos
			translateChange = totalAxes * translateChange;
			translateChangePitch = totalAxes * translateChangePitch;
			translateChangeRoll = totalAxes * translateChangeRoll;
			//Set position for smoothing function and add jump value
			//divide position by deltaTime for framerate independence
			dampTo = midpoint + (translateChange / Time.deltaTime * 0.01f);
			//camera roll and pitch bob
			tempLocalEulerAngles = new Vector3(translateChangePitch, 0,translateChangeRoll);
		}else{
			//reset variables to prevent view twitching when falling
			dampTo = midpoint + Mathf.Sin(Time.time * 1.25f) * 0.015f;//add small sine bob for camera idle movement
			totalAxes = 0;
			translateChange = 0;
		}
		//use SmoothDamp to smooth position and remove any small glitches in bob amount 
		dampOrg = Mathf.SmoothDamp(dampOrg, dampTo, ref dampVelocity, 0.1f, Mathf.Infinity, Time.deltaTime);
		//Pass bobbing amount and angles to the camera kick script in the camera object after smoothing
		CameraKickComponent.dampOriginY = dampOrg;
		CameraKickComponent.bobAngles = Vector3.SmoothDamp(CameraKickComponent.bobAngles, tempLocalEulerAngles, ref dampVelocity2, 0.1f, Mathf.Infinity, Time.deltaTime);
	}
}