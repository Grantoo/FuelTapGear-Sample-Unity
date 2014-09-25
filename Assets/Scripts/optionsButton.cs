using UnityEngine;
using System.Collections;

public class optionsButton : MonoBehaviour 
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
			
			Application.LoadLevel("Options");
		}
	}

}
