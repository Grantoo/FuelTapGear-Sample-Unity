using UnityEngine;
using System.Collections;



public class FBLoginButton : MonoBehaviour 
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
			Debug.Log ("FBLoginButton - left click.");
			
			GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

			
			_fuelHandlerScript.LoginButtonPressed();

			_fuelHandlerScript.updateLoginText();

		}
	}
	
	
}
