using UnityEngine;
using System.Collections;

public class playButton : MonoBehaviour 
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
			Debug.Log ("playButton - left click.");
			
			Application.LoadLevel("GamePlay");
		}
	}
}