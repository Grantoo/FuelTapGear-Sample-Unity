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
		Debug.Log ("OnMouseOver");
		if (Input.GetMouseButtonDown (0)) 
		{
			Debug.Log ("- left click.");
			
			Application.LoadLevel("MainMenu");
		}
	}

}