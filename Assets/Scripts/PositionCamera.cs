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
	}
	
	void Update () 
	{
		if (firstPass == true) 
		{

			#if UNITY_IPHONE

			if ((iPhone.generation.ToString ()).IndexOf ("iPad") > -1) 
			{
				
			} else {
				float orthoSize = camera.orthographicSize;
				camera.orthographicSize = orthoSize * OthorMult;
			}
		

			#elif UNITY_ANDROID
				float orthoSize = camera.orthographicSize;
				camera.orthographicSize = orthoSize * OthorMult;

			#endif


			firstPass = false;
		}



	}
}
