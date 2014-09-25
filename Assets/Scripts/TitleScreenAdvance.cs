using UnityEngine;
using System.Collections;

public class TitleScreenAdvance : MonoBehaviour 
{
	
	public float advanceTimerValue = 0.0f;

	void Start () 
	{
		
	}
	
	void Update () 
	{
		//not using auto advance yet
		advanceTimerValue += Time.deltaTime;
		if (advanceTimerValue > 10.0f) 
		{
			//Application.LoadLevel("MainMenu");
		}

		if (Input.GetMouseButtonDown (0)) 
		{
			Application.LoadLevel("MainMenu");
		}

	}
}
