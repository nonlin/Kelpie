using UnityEngine;
using System.Collections;
//script to fade out decals like bullet marks smoothly
public class FadeOutDecals : MonoBehaviour {

	float startTime;
	
	void Start (){
		startTime = Time.time;
	}
	
	void Update (){
		
		if(startTime + 10 < Time.time){
			Vector4 tempColorVec = renderer.material.color; 
	   		tempColorVec.w -= 1 * Time.deltaTime;//store the color's alpha amount as the fourth value of the Vector4
	    	renderer.material.color = tempColorVec;//set the guiTexture's color to the value(s) of our temporary color vector
		}
		
	}
}