//PainFade.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;
//script to make screen flash red when damage taken
public class PainFade : MonoBehaviour {
	
	[HideInInspector]
	public GameObject painFadeObj;
	
	public void FadeIn ( Color color ,   float fadeLength  ){
		Texture2D fadeTexture = new Texture2D (1, 1);
		fadeTexture.SetPixel(0, 0, color);
		fadeTexture.Apply();

		painFadeObj.layer = 14;//set fade object's layer to one not ignored by weapon camera
		painFadeObj.AddComponent<GUITexture>();
		painFadeObj.transform.position = new Vector3 (0.5f, 0.5f, 1000);
		painFadeObj.guiTexture.texture = fadeTexture;
		StartCoroutine(DoFade(fadeLength, true));
	}
		
	IEnumerator DoFade ( float fadeLength ,   bool destroyTexture  ){

		//make alpha of color = 0 (transparent for starting fade out)
		//Create a temporary Vector4 (C# does not allow modifying guiTexture color directly, but JS will)
		Vector4 tempColorVec = guiTexture.color; 
   		tempColorVec.w = 0.0f;//store the color's alpha amount as the fourth value of the Vector4
    	guiTexture.color = tempColorVec;//set the guiTexture's color to the value of our temporary color vector
		
		// Fade texture out
		float time = 0.0f;
		while (time < fadeLength){
			time += Time.deltaTime;
			tempColorVec.w = Mathf.InverseLerp(fadeLength, 0.0f, time);
			guiTexture.color = tempColorVec;
			yield return 0;
		}
	
		Destroy (gameObject);
	
		// If we created the texture from code we used DontDestroyOnLoad,
		// which means we have to clean it up manually to avoid leaks
		if (destroyTexture){
			Destroy (guiTexture.texture);
		}
	}
}