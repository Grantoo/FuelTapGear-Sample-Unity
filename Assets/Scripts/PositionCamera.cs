using UnityEngine;
using System.Collections;

public class PositionCamera : MonoBehaviour 
{
	//just for testing

	public float OthorMult = 1.2f; // Desired scale for iPhone

	private bool firstPass;

	void Start () 
	{
		firstPass = true;
	
/*
		// set the desired aspect ratio (the values in this example are
		// hard-coded for 16:9, but you could make them into public
		// variables instead so you can set them at design time)
		float targetaspect = 3.0f / 4.0f;
		
		// determine the game window's current aspect ratio
		float windowaspect = (float)Screen.width / (float)Screen.height;
		
		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;

		// obtain camera component so we can modify its viewport
		Camera camera = GetComponent<Camera>();
		
		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{  
			Rect rect = camera.rect;
			
			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;
			
			camera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;
			
			Rect rect = camera.rect;
			
			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;
			
			camera.rect = rect;
		}
*/
	}
	
	void Update () 
	{
		if (firstPass == true) 
		{

			#if UNITY_IOS

			if ((UnityEngine.iOS.Device.generation.ToString ()).IndexOf ("iPad") > -1) 
			{
				
			} else {
				float orthoSize = GetComponent<Camera>().orthographicSize;
				GetComponent<Camera>().orthographicSize = orthoSize * OthorMult;
			}
		

			#elif UNITY_ANDROID
				float orthoSize = GetComponent<Camera>().orthographicSize;
				GetComponent<Camera>().orthographicSize = orthoSize * OthorMult;

			#endif


			firstPass = false;
		}



	}
}
