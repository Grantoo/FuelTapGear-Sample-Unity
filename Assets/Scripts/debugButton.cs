using UnityEngine;
using System.Collections;

public class debugButton : MonoBehaviour 
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
			Debug.Log ("debugButton - left click.");
			
			GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
			
			
			//_mainmenuScript.LaunchDashBoardWithResults();


			_fuelHandlerScript.launchPropeller();
		}
	}
	
}