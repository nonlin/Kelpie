using UnityEngine;
using System.Collections;
//script for moving an elevator vertically up and down
public class MovingElevator : MonoBehaviour {
	public Transform pointB;
	private Vector3 pointA;
	public float speed = 1.0f;
	private float direction = 1.0f;
	
	IEnumerator Start (){
		pointA = transform.position;
		while (true) {
		
			if (transform.position.y > pointA.y){
				direction = -1;
			}else{
				if(transform.position.y < pointB.position.y){direction = 1;}
			}
			float delta = Time.deltaTime * 60;
			transform.Translate(0, direction  * speed * delta, 0, Space.World);
			
			yield return 0;
		}
	}
	
}