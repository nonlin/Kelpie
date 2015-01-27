using UnityEngine;
using System.Collections;
//script for moving a platform horizontally back and fourth
public class MovingPlatform : MonoBehaviour {
	public Transform pointB;
	private Vector3 pointA;
	public float speed = 1.0f;
	private float direction = 1.0f;

	IEnumerator Start (){
	    pointA = transform.position;
	    while (true) {
	    
			if (transform.position.z < pointA.z){
				direction = 1;
			}else{
				if(transform.position.z > pointB.position.z){direction = -1;}
			}
			float delta = Time.deltaTime * 60;
			transform.Translate(direction  * -speed * delta,0,direction  * speed * delta, Space.World);
				
			yield return 0;
	    }
	}
	
}