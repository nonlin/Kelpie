//Ladder.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;
//script for ladder triggers
public class Ladder : MonoBehaviour {

	private bool  triggerState = false;
	public GameObject playerObj;
	
	void OnTriggerEnter ( Collider other  ){
		//on start of a collision with ladder trigger set climbing var to true on FPSRigidBodyWalker script
		FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		if(!triggerState && other.gameObject.tag == "Player"){
			triggerState = true;
			FPSWalker.climbing = true;
		}
	} 
	void OnTriggerExit ( Collider other2  ){
		FPSRigidBodyWalker FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		//on exit of a collision with ladder trigger set climbing var to false on FPSRigidBodyWalker script
		if(other2.gameObject.tag == "Player"){
			triggerState = false;
			FPSWalker.climbing = false;
		}
	}
}