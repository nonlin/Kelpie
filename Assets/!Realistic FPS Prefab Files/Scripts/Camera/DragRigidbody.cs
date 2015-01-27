//DragRigidbody.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class DragRigidbody : MonoBehaviour {
	private float spring = 75.0f;
	private float damper = 1.0f;
	private float drag = 10.0f;
	private float angularDrag = 5.0f;
	private float distance = 0.0f;
	private float reachDistance = 2.5f;
	private float throwForce = 500.0f;
	private bool attachToCenterOfMass = false;
	private SpringJoint springJoint;
    private float oldDrag;
    private float oldAngularDrag;
    private bool dragState;
	
	public LayerMask layersToDrag = 0;//only check these layers for draggable objects
       
    void FixedUpdate (){
		
		FPSPlayer FPSPlayerComponent = GetComponent<FPSPlayer>();
	
		// Make sure the user pressed the mouse down
		if(!Input.GetKeyDown(FPSPlayerComponent.moveObject)){
			return;
		}
			
		// We need to actually hit an object
        RaycastHit hit;
        if(!Physics.Raycast(Camera.main.transform.position, ((Camera.main.transform.position + Camera.main.transform.forward * reachDistance) - Camera.main.transform.position).normalized, out hit, reachDistance, layersToDrag)){
            return;
        }
		// We need to hit a rigidbody that is not kinematic
		if(!hit.rigidbody || hit.rigidbody.isKinematic){
			return;
		}
		
		if(!springJoint){
			GameObject go = new GameObject("Rigidbody dragger");
			Rigidbody body = go.AddComponent ("Rigidbody") as Rigidbody;
			springJoint = go.AddComponent ("SpringJoint") as SpringJoint;
			body.isKinematic = true;
		}
		
		springJoint.connectedBody = hit.rigidbody;
		springJoint.transform.position = hit.point;
		
		if(attachToCenterOfMass){
			Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
			anchor = springJoint.transform.InverseTransformPoint(anchor);
			springJoint.anchor = anchor;
		}else{
			springJoint.anchor = Vector3.zero;
		}
		
		springJoint.spring = spring;
		springJoint.damper = damper;
		springJoint.maxDistance = distance;
		
		StartCoroutine ("DragObject", hit.distance);
	}
	
    IEnumerator DragObject ( float distance  ){
		
		FPSPlayer FPSPlayerComponent = GetComponent<FPSPlayer>();
		
        if(!dragState){
            oldDrag = springJoint.connectedBody.drag;
            oldAngularDrag = springJoint.connectedBody.angularDrag;
            dragState = true;
        }
        springJoint.connectedBody.drag = drag;
        springJoint.connectedBody.angularDrag = angularDrag;
		
		while(Input.GetKey(FPSPlayerComponent.moveObject) && springJoint.connectedBody){
            Ray ray = new Ray (Camera.main.transform.position, ((Camera.main.transform.position + Camera.main.transform.forward * reachDistance) - Camera.main.transform.position).normalized);
            springJoint.transform.position = ray.GetPoint(distance);
            if(!Input.GetKey(FPSPlayerComponent.throwObject)){
				//let go of object if we are out of grabbing range
				if( Vector3.Distance(springJoint.connectedBody.transform.position, Camera.main.transform.position) < 3.5f){
					yield return 0;
				}else{
					break;
				}
			}else{//throw object
				float throwForceAmt;
				if(springJoint.connectedBody.mass < 1){
					throwForceAmt = throwForce/2;
				}else{
					throwForceAmt = throwForce * springJoint.connectedBody.mass;
				}
				springJoint.connectedBody.AddForceAtPosition((throwForceAmt * Camera.main.transform.forward), springJoint.transform.position);
				break;
			}
		}
		if (springJoint.connectedBody){//stop dragging object
			DropObject();
		}
	}
	//if dragged object contacts player object, stop dragging to prevent pushing or lifting player
	void OnCollisionStay(Collision col){
		if(springJoint){
			if(springJoint.connectedBody){//stop dragging object
				if(col.gameObject.rigidbody == springJoint.connectedBody){
					DropObject();
				}		
			}
		}
    }
	
	void DropObject(){
		springJoint.connectedBody.drag = oldDrag;
        springJoint.connectedBody.angularDrag = oldAngularDrag;
        springJoint.connectedBody = null;
        dragState = false;
	}

}