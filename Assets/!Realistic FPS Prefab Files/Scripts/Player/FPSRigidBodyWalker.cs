//FPSRigidBodyWalker.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class FPSRigidBodyWalker : MonoBehaviour {
	
	//objects accessed by this script
	[HideInInspector]
	public GameObject weaponObj;
	[HideInInspector]
	public GameObject CameraObj;
	
	//track player input
	[HideInInspector]
	public float inputXSmoothed = 0.0f;//binary inputs smoothed using lerps
	[HideInInspector]
	public float inputYSmoothed = 0.0f;
	[HideInInspector]
	public int inputX = 0;//1 = button pressed 0 = button released
	[HideInInspector]
	public int inputY = 0;
		
	//player movement speed amounts
	public float runSpeed = 9.0f;
	public float walkSpeed = 4.0f;
	public float jumpSpeed = 3.0f;
	private float limitStrafeSpeed = 0.0f;
	public float backwardSpeedPercentage = 0.6f;//percentage to decrease movement speed while moving backwards
	public float crouchSpeedPercentage = 0.55f;//percentage to decrease movement speed while crouching
	private float crouchSpeedAmt = 1.0f;
	public float strafeSpeedPercentage = 0.8f;//percentage to decrease movement speed while strafing directly left or right
	private float speedAmtY = 1.0f;//current player speed per axis which is applied to rigidbody velocity vector
	private float speedAmtX = 1.0f;
	[HideInInspector]
	public bool zoomSpeed = false;//to control speed of movement while zoomed, handled by Ironsights script and true when zooming
	public float zoomSpeedPercentage = 0.6f;//percentage to decrease movement speed while zooming
	private float zoomSpeedAmt = 1.0f;
	private float speed = 6.0f;//combined axis speed of player movement
		
	//rigidbody physics settings
	public int gravity = 15;//additional gravity that is manually applied to the player rigidbody
	public int slopeLimit = 40;//the maximum allowed ground surface/normal angle that the player is allowed to climb
	private int maxVelocityChange = 5;//maximum rate that player velocity can change
	private Transform myTransform;
	private Vector3 moveDirection = Vector3.zero;//movement velocity vector, modified by other speed factors like walk, zoom, and crouch states
	[HideInInspector]
	public bool grounded = false;//true when capsule cast hits ground surface
	private bool rayTooSteep = false;//true when ray from capsule origin hits surface/normal angle greater than slopeLimit, compared with capsuleTooSteep
	private bool capsuleTooSteep = false;//true when capsule cast hits surface/normal angle greater than slopeLimit, compared with rayTooSteep
	[HideInInspector]
	public Vector3 velocity = Vector3.zero;//total movement velocity vector
	private CapsuleCollider capsule;
	[HideInInspector]
	public Transform initialParent;//store original parent for use when unparenting player rigidbody from moving platforms
	private bool parentState = false;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
		
	//falling
	[HideInInspector]
	public float airTime = 0.0f;//total time that player is airborn
	private bool airTimeState = false;
	public float fallingDamageThreshold = 5.5f;//Units that player can fall before taking damage
	private float fallStartLevel;//the y coordinate that the player lost grounding and started to fall
	[HideInInspector]
	public float fallingDistance;//total distance that player has fallen
	private bool falling = false;//true when player is losing altitude
		
	//climbing (ladders or other climbable surfaces)
	[HideInInspector]
	public bool climbing = false;//true when playing is in contact with ladder trigger
	public float climbSpeed = 4.0f;//speed that player moves upward when climbing
	[HideInInspector]
	public float climbSpeedAmt = 4.0f;//actual rate that player is climbing
	
	//jumping
	public float antiBunnyHopFactor = 0.35f;//to limit the time between player jumps
	[HideInInspector]
	public bool jumping = false;//true when player is jumping
	private float jumpTimer = 0.0f;//track the time player began jump
	private bool jumpfxstate = true;
	private bool jumpBtn = true;//to control jump button behavior
	[HideInInspector]
	public float landStartTime = 0.0f;//time that player landed from jump
		
	//sprinting
	[HideInInspector]
	public bool canRun = true;//true when player is allowed to sprint
	[HideInInspector]
	public bool sprintActive = false;//true when sprint button is ready
	private bool sprintBtnState = false;
	[HideInInspector]
	public bool cancelSprint = false;//true when sprint is canceled by other player input
	[HideInInspector]
	public float sprintStopTime = 0.0f;//track when sprinting stopped for control of item pickup time in FPSPlayer script 
	private bool sprintStopState = true;
	
	//crouching	
	[HideInInspector]
	public float midPos = 0.9f;//camera vertical position
	[HideInInspector]
	public bool crouched = false;//true when player is crouching
	private bool crouchState = false;
	private bool crouchHit = false;//true when object above player prevents standing up from crouch
		
	//sound effeects
	public AudioClip landfx;//audiosource attatched to this game object with landing sound effect
	public AudioClip jumpfx;//audiosource attatched to this game object with jumping sound effect
	public LayerMask clipMask;//mask for reducing the amount of objects that ray and capsule casts have to check
	
	void Start (){
		myTransform = transform;//define transform for efficiency
		initialParent = myTransform.parent;//track parent of rigidbody on level load
		//clamp movement modifier percentages
		backwardSpeedPercentage = Mathf.Clamp01(backwardSpeedPercentage);
		crouchSpeedPercentage = Mathf.Clamp01(crouchSpeedPercentage);
		strafeSpeedPercentage = Mathf.Clamp01(strafeSpeedPercentage);
		zoomSpeedPercentage = Mathf.Clamp01(zoomSpeedPercentage);
	}
	
	void Awake (){
		//Initialize rigidbody
		rigidbody.freezeRotation = true;
		rigidbody.useGravity = true;
		capsule = GetComponent<CapsuleCollider>();
	}
	void FixedUpdate (){
		RaycastHit hit;
		//set up external script references
		SmoothMouseLook SmoothMouseLookComponent = CameraObj.GetComponent<SmoothMouseLook>();
		FPSPlayer FPSPlayerComponent = GetComponent<FPSPlayer>();
			
		//set the vertical bounds of the capsule used to detect player collisions
		Vector3 p1 = myTransform.position;//bottom of player capsule
		Vector3 p2 = p1 + Vector3.up * capsule.height/2;//top of player capsule
		
		//track rigidbody velocity
		velocity = rigidbody.velocity;
			
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Player Input
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		//track movement buttons and set input vars
		//Input.Axis is not used here to have have more control over button states and to avoid glitches 
		//such as pressing opposite movement buttons simultaneously causing player to move very slow in one direction
		if (Input.GetKey (FPSPlayerComponent.moveForward)){inputY = 1;}
		if (Input.GetKey (FPSPlayerComponent.moveBack)){inputY = -1;}
		if (!Input.GetKey (FPSPlayerComponent.moveBack) && !Input.GetKey (FPSPlayerComponent.moveForward)){inputY = 0;}
		if (Input.GetKey (FPSPlayerComponent.moveBack) && Input.GetKey (FPSPlayerComponent.moveForward)){inputY = 0;}
		if (Input.GetKey (FPSPlayerComponent.strafeLeft)){inputX = -1;}
		if (Input.GetKey (FPSPlayerComponent.strafeRight)){inputX = 1;}
		if (!Input.GetKey (FPSPlayerComponent.strafeLeft) && !Input.GetKey (FPSPlayerComponent.strafeRight)){inputX = 0;}
		if (Input.GetKey (FPSPlayerComponent.strafeLeft) && Input.GetKey (FPSPlayerComponent.strafeRight)){inputX = 0;}
		
		//Smooth our movement states using Mathf.Lerp
		inputXSmoothed = Mathf.Lerp(inputXSmoothed,inputX,Time.deltaTime * 6.0f);
	    inputYSmoothed = Mathf.Lerp(inputYSmoothed,inputY,Time.deltaTime * 6.0f);
			
		//This is the start of the large block that performs all movement actions while grounded	
		if (grounded) {
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Landing
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//reset airTimeState var so that airTime will only be set once when player looses grounding
			airTimeState = true;
			
			if (falling){//reset falling state and perform actions if player has landed from a fall
				
				fallingDistance = 0;
				landStartTime = Time.time;//track the time when player landed
		       	falling = false;
		        
		        if((fallStartLevel - myTransform.position.y)>2.0f){
		        	//play landing sound effect when falling and not landing from jump
		        	if(!jumping){
						//play landing sound
						AudioSource.PlayClipAtPoint(landfx, Camera.main.transform.position);
						//make camera jump when landing for better feeling of player weight	
						if (Camera.main.animation.IsPlaying("CameraLand")){
							//rewind animation if already playing to allow overlapping playback
							Camera.main.animation.Rewind("CameraLand");
						}
						Camera.main.animation.CrossFade("CameraLand", 0.35f,PlayMode.StopAll);
					}
		        }
		        
		        //track the distance of the fall and apply damage if over falling threshold
		        if (myTransform.position.y < fallStartLevel - fallingDamageThreshold){
		        	CalculateFallingDamage(fallStartLevel - myTransform.position.y);
		        }
	    	}
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Crouch Mode Handling
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		    	
	    	//set crouched variable that other scripts will access to check for crouching
	    	if(Input.GetKey (FPSPlayerComponent.crouch)){
	    		if(!crouchState){
	    			if(!crouched){
	    				crouched = true;
	    				sprintActive = false;//cancel sprint if crouch button is pressed
	    			}else{
						if(!Physics.CapsuleCast (p1, p2, capsule.radius * 0.9f, transform.up, out hit, 0.4f, clipMask.value)){
	    					crouched = false;
						}
	    			}
	    			crouchState = true;
	    		}
	    	}else{
	    		crouchState = false;
	    		if(sprintActive || climbing){
	    			crouched = false;//cancel crouch if sprint button is pressed
	    		}
	    	}
	    	//cancel crouch if jump button is pressed
	    	if(Input.GetKey (FPSPlayerComponent.jump) && crouched && !Physics.CapsuleCast (p1, p2, capsule.radius * 0.9f, transform.up, out hit, 0.4f, clipMask.value)){
    			crouched = false;
    			landStartTime = Time.time;//set land time to time jump is pressed to prevent uncrouching and then also jumping
    		}
				
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Sprinting
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//set sprintActive based on sprint button input
			if(Input.GetKey (FPSPlayerComponent.sprint)){
				if(sprintBtnState){
					if(!sprintActive && !crouchHit){//only allow sprint to start or cancel crouch if player is not under obstacle
						sprintActive = true;
					}else{
						sprintActive = false;//pressing sprint button again while sprinting stops sprint
					}
					sprintBtnState = false;
				}
			}else{
				sprintBtnState = true;
				if(!canRun){
					sprintActive = false;
				}
			}
			
			//cancel a sprint in certain situations
			if((sprintActive && Input.GetKey (FPSPlayerComponent.fire))
			||(sprintActive && Input.GetKey (FPSPlayerComponent.reload))
			||(sprintActive && Input.GetKey (FPSPlayerComponent.zoom))//cancel sprint if zoom button is pressed
			||(FPSPlayerComponent.zoomed && Input.GetKey (FPSPlayerComponent.fire))
			||climbing){
				cancelSprint = true;
			}
			
			//reset cancelSprint var so it has to pressed again to sprint
			if(!sprintActive && cancelSprint){
				if(!Input.GetKey (FPSPlayerComponent.zoom)){
					cancelSprint = false;
				}
			}
		
			//determine if player can run 
			if(inputY != 0.0f 
			&& sprintActive
			&& !crouched
			&& (!cancelSprint || (cancelSprint && FPSPlayerComponent.zoomed)) 
			&& grounded){
			 	canRun = true;
				FPSPlayerComponent.zoomed = false;//cancel zooming when sprinting
				sprintStopState = true;
			}else{
				if(sprintStopState){
					sprintStopTime = Time.time;
					sprintStopState = false;
				}
			 	canRun = false;
			}	
				
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Player Movement Speeds
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
			//check that player can run and set speed 
			if(canRun){
				if(speed < runSpeed){
					speed += 12 * Time.deltaTime;//gradually accelerate to run speed
				}
			}else{
				if(speed > walkSpeed){
					speed -= 16 * Time.deltaTime;//gradually decelerate to walk speed
				}
			}
					
			//check if player is zooming and set speed 
			if(zoomSpeed){
				if(zoomSpeedAmt > zoomSpeedPercentage){
					zoomSpeedAmt -= Time.deltaTime;//gradually decrease variable to zooming limit value
				}
			}else{
				if(zoomSpeedAmt < 1.0f){
					zoomSpeedAmt += Time.deltaTime;//gradually increase variable to neutral
				}
			}
			
			//check that player can crouch and set speed
			//also check midpos because player can still be under obstacle when crouch button is released 
			if(crouched || midPos < 0.9f){
				if(crouchSpeedAmt > crouchSpeedPercentage){
					crouchSpeedAmt -= Time.deltaTime;//gradually decrease variable to crouch limit value
				}
			}else{
				if(crouchSpeedAmt < 1.0f){
					crouchSpeedAmt += Time.deltaTime;//gradually increase variable to neutral
				}
			} 
			
			//limit move speed if backpedaling
			if (inputY >= 0){
				if(speedAmtY < 1.0f){
					speedAmtY += Time.deltaTime;//gradually increase variable to neutral
				}
			}else{
				if(speedAmtY > backwardSpeedPercentage){
					speedAmtY -= Time.deltaTime;//gradually decrease variable to backpedal limit value
				}
			}
			
			//allow limiting of move speed if strafing directly sideways and not diagonally
			if (inputX == 0 || inputY != 0){
				if(speedAmtX < 1.0f){
					speedAmtX += Time.deltaTime;//gradually increase variable to neutral
				}
			}else{
				if(speedAmtX > strafeSpeedPercentage){
					speedAmtX -= Time.deltaTime;//gradually decrease variable to strafe limit value
				}
			}
				
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Jumping
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
			if(jumping){
				//play landing sound effect after landing from jump and reset jumpfxstate
				if(jumpTimer + 0.25f < Time.time){
						//play landing sound
						AudioSource.PlayClipAtPoint(landfx, Camera.main.transform.position);
						
						if (Camera.main.animation.IsPlaying("CameraLand")){
							//rewind animation if already playing to allow overlapping playback
							Camera.main.animation.Rewind("CameraLand");
						}
						Camera.main.animation.CrossFade("CameraLand", 0.35f,PlayMode.StopAll);
						
						jumpfxstate = true;
				}
				//reset jumping var (this check must be before jumping var is set to true below)
				jumping = false;
				//allow a small amount of time for capsule to become un-grounded before setting
				//jump button state to false to prevent continuous jumping if jump button is held.
				if(jumpTimer + 0.25f < Time.time){
					jumpBtn = false;
				}
			}
			
			//determine if player is jumping and set jumping variable
			if (Input.GetKey (FPSPlayerComponent.jump) 
			&& !FPSPlayerComponent.zoomed
			&& jumpBtn//check that jump button is not being held
			&& !crouched
			&& landStartTime+antiBunnyHopFactor < Time.time//check for bunnyhop delay before jumping
			&& !rayTooSteep){//do not jump if ground normal is greater than slopeLimit
				
				if(!jumping){
					jumping = true;
					//track the time we began to jump
					jumpTimer = Time.time;
				}
				//apply the jump velocity to the player rigidbody
				rigidbody.velocity = new Vector3(velocity.x, Mathf.Sqrt(2 * jumpSpeed * gravity), velocity.z);
			}
			
			//set jumpBtn to false to prevent continuous jumping while holding jump button.
			if (!Input.GetKey (FPSPlayerComponent.jump) && landStartTime+antiBunnyHopFactor<Time.time){
				jumpBtn = true;
			}
					
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Crouching
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			if(Time.timeSinceLevelLoad > 0.5f){			
				//crouch
				if(crouched){
			    		if(midPos > 0.45f){midPos -= 5 * Time.deltaTime;}//decrease camera height to crouch height
						if(capsule.height > 1.25f){capsule.height -= 5 * Time.deltaTime;}//decrease capsule height to crouch height
				}else{
					if(!Input.GetKey (FPSPlayerComponent.jump)){
	            		if(midPos < 0.9f){midPos += 2.25f * Time.deltaTime;}//increase camera height to standing height
	         			if(capsule.height < 2.0f){capsule.height += 2.25f * Time.deltaTime;}//increase camera height to standing height
					}
				}	
			}
			
		}else{//Player is airborn////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//keep track of the time that player lost grounding for air manipulation and moving gun while jumping
			if(airTimeState){
				airTime = Time.time;
				airTimeState = false;
			}
				
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Falling
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//subtract height we began falling from current position to get falling distance
			fallingDistance = fallStartLevel - myTransform.position.y;
		
			if (!falling){
			    falling = true;			
			    //start tracking altitude (y position) for fall check
			    fallStartLevel = myTransform.position.y;
			    
			    //check jumpfxstate var to play jumping sound only once
			    if(jumping && jumpfxstate){
					//play jumping sound
					AudioSource.PlayClipAtPoint(jumpfx, Camera.main.transform.position);
					jumpfxstate = false;
				}	    
			}	
		}
			
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Climbing
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		//make player climb up ladders or other surfaces
		if(climbing){//climbing var is managed by a ladder script attatched to a trigger that is placed near a ladder
			if(inputY > 0){//only climb if player is moving forward
				//make player climb up or down based on the pitch of the main camera (check mouselook script pitch)
				climbSpeedAmt = 1 + (climbSpeed * (SmoothMouseLookComponent.rotationY / 48));
				climbSpeedAmt = Mathf.Clamp(climbSpeedAmt, -climbSpeed, climbSpeed);//limit vertical speed to climb speed
				//apply climbing velocity to the player's rigidbody
				rigidbody.velocity = new Vector3(velocity.x, climbSpeedAmt, velocity.z);
			}else{
				//if not moving forward, do not add extra upward velocity, but allow the player to move off the ladder
				rigidbody.velocity = new Vector3(velocity.x, 0, velocity.z);
			}
		}
			
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Player Ground Check
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	    //cast capsule shape down to see if player is about to hit anything or is resting on the ground
	    if (Physics.CapsuleCast (p1, p2, capsule.radius * 0.9f, -transform.up, out hit, 0.75f, clipMask.value) || climbing){
	        grounded = true;
	    }else{
	    	grounded = false;
	    }
		
		//check that angle of the normal directly below the capsule center point is less than the movement slope limit 
		if (Physics.Raycast(myTransform.position, -transform.up, out hit, 2.6f, clipMask.value)) {
			if(Vector3.Angle ( hit.normal, Vector3.up ) > slopeLimit){
				rayTooSteep = true;	
			}else{
				rayTooSteep = false;	
			}
		}
			
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Player Velocity
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	    
		//limit speed if strafing diagonally
		limitStrafeSpeed = (inputX != 0.0f && inputY != 0.0f)? .7071f : 1.0f;
		
		//align localEulerAngles and eulerAngles y values with cameras' to make player walk in the direction the camera is facing 
		Vector3 tempLocalEulerAngles = new Vector3(0,CameraObj.transform.localEulerAngles.y,0);//store angles in temporary vector for C#
		myTransform.localEulerAngles = tempLocalEulerAngles;//apply angles from temporary vector to player object
		Vector3 tempEulerAngles = new Vector3(0, CameraObj.transform.eulerAngles.y,0);//store angles in temporary vector for C#
		myTransform.eulerAngles = tempEulerAngles;//apply angles from temporary vector to player object
		
		//apply velocity to player rigidbody and allow a small amount of air manipulation
		//to allow jumping up on obstacles when jumping from stationary position with no forward velocity
		if((grounded || climbing || ((airTime + 0.3f) > Time.time)) && FPSPlayerComponent.hitPoints > 0 && !FPSPlayerComponent.restarting){
			//Check both capsule center point and capsule base slope angles to determine if the slope is too high to climb.
			//If so, bypass player control and apply some extra downward velocity to help capsule return to more level ground.
			if(!capsuleTooSteep || (capsuleTooSteep && !rayTooSteep)){
				// We are grounded, so recalculate movedirection directly from axes	
				moveDirection = new Vector3(inputXSmoothed*limitStrafeSpeed, 0.0f, inputYSmoothed*limitStrafeSpeed);
				//realign moveDirection vector to world space
				moveDirection = myTransform.TransformDirection(moveDirection);
				//apply speed limits to moveDirection vector
				moveDirection = moveDirection * speed * speedAmtX * speedAmtY * crouchSpeedAmt * zoomSpeedAmt;
		
				//apply a force that attempts to reach target velocity
				Vector3 velocityChange = moveDirection - velocity;
				//limit max speed
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
		
				//apply ladder climbing speed to velocityChange vector and set y velocity to zero if not climbing ladder
				if(climbing && inputY > 0){
					velocityChange.y = climbSpeedAmt;
				}else{
					velocityChange.y = 0;
				}
				
				//finally, add movement velocity to player rigidbody velocity
				rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
				
			}else{
				//If slope is too high below both the center and base contact point of capsule, apply some downward velocity to help
				//the capsule fall to more level ground. Check the slope angle at two points on the collider to prevent it from 
				//getting stuck when player control is bypassed and to have more control over the slope angle limit.
				rigidbody.AddForce(new Vector3 (0, -2, 0), ForceMode.VelocityChange);
			}
		}else{
			//if player is dead or restarting level set velocity to zero to prevent rigidbody from moving when camera is stopped
			if(FPSPlayerComponent.hitPoints <= 0 || FPSPlayerComponent.restarting){	
				rigidbody.velocity = Vector3.zero;	
			}
		}
		
		if(!climbing){
			//apply gravity manually for more tuning control except when climbing a ladder to avoid unwanted downward movement
	    	rigidbody.AddForce(new Vector3 (0, -gravity * rigidbody.mass, 0));
	    	rigidbody.useGravity = true;
		}else{
			rigidbody.useGravity = false;
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Rigidbody Collisions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void TrackCollision ( Collision col  ){
	    //define a height of about a fourth of the capsule height to check for collisions with platforms
		float minimumHeight = (capsule.bounds.min.y + capsule.radius);
		//check the collision points within our predefined height range  
		foreach(ContactPoint c in col.contacts){
			if (c.point.y < minimumHeight) {
				//check that we want to collide with this object (check for "PhysicsObject" tag) and that it's surface is not too steep 
				if(!parentState && col.gameObject.tag == "PhysicsObject" && Vector3.Angle ( c.normal, Vector3.up ) < 70){
					//set player object parent to platform transform to inherit it's movement in addition to player movement
					myTransform.parent = col.transform;
					parentState = true;//only set parent once to prevent rapid parenting and de-parenting that breaks functionality
				}
				//check that angle of the surface that the capsule base is touching is less than the movement slope limit  
				if(Vector3.Angle ( c.normal, Vector3.up ) > slopeLimit){
					capsuleTooSteep = true;	
				}else{
					capsuleTooSteep = false;	
				}
			}
		}
		
	}
	
	void OnCollisionExit ( Collision col  ){
		//unparent if we are no longer standing on our parent
		myTransform.parent = initialParent;
		//return parentState to false so we may check for collisions again
		parentState = false;
		capsuleTooSteep = false;	
	}
	
	void OnCollisionStay ( Collision col  ){
	   TrackCollision (col);
	}
	
	void OnCollisionEnter ( Collision col  ){
	   TrackCollision (col);
	}
	
	void CalculateFallingDamage ( float fallDistance  ){
	    GetComponent<FPSPlayer>().ApplyDamage(fallDistance * 2);   
	}
}