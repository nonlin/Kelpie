using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class SmoothMouseLook : MonoBehaviour {

    public float sensitivity = 4.0f;
	[HideInInspector]
	public float sensitivityAmt = 4.0f;//actual sensitivity modified by IronSights Script

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -85f;
    private float maximumY = 85f;

    private float rotationX = 0.0f;
	[HideInInspector]
    public float rotationY = 0.0f;
   
	public float smoothSpeed = 0.35f;
	
	private Quaternion originalRotation;
	private Transform myTransform;
	
	void Start(){         
        if (rigidbody){rigidbody.freezeRotation = true;}
		
		myTransform = transform;//manually set transform for efficiency
		
		originalRotation = myTransform.localRotation;
		//sync the initial rotation of the main camera to the y rotation set in editor
		Vector3 tempRotation = new Vector3(0,Camera.main.transform.eulerAngles.y,0);
		originalRotation.eulerAngles = tempRotation;
		
		sensitivityAmt = sensitivity;//initialize sensitivity amount from var set by player
		
		// Hide the cursor
		Screen.showCursor = false;
    }

    void Update(){
		// Hide the cursor
		Screen.lockCursor = true;
		
		if(Time.timeSinceLevelLoad > 1 && Time.timeScale > 0){
			// Read the mouse input axis
			rotationX += Input.GetAxisRaw("Mouse X") * sensitivityAmt * Time.timeScale;//lower sensitivity at slower time settings
			rotationY += Input.GetAxisRaw("Mouse Y") * sensitivityAmt * Time.timeScale;
			 
			rotationX = ClampAngle (rotationX, minimumX, maximumX);
			rotationY = ClampAngle (rotationY, minimumY, maximumY);
			 
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
			//smooth the mouse input
			myTransform.rotation = Quaternion.Slerp(myTransform.rotation , originalRotation * xQuaternion * yQuaternion, smoothSpeed * Time.smoothDeltaTime * 60 / Time.timeScale);
		}
		
    }
   
	//function used to limit angles
    public static float ClampAngle (float angle, float min, float max){
        angle = angle % 360;
        if((angle >= -360F) && (angle <= 360F)){
            if(angle < -360F){
                angle += 360F;
            }
            if(angle > 360F){
                angle -= 360F;
            }         
        }
        return Mathf.Clamp (angle, min, max);
    }
	
}