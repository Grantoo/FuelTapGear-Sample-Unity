using UnityEngine;
using System.Collections;

public class backButton : MonoBehaviour 
{
	
	void Start () 
	{
		
	}
	
	void Update () 
	{
		

	}

	void OnMouseOver () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			Debug.Log ("backButton - left click.");
			
			Application.LoadLevel("MainMenu");
		}
	}

}