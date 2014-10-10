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
			
			GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

			
			//_mainmenuScript.LaunchDashBoardWithResults();
			
			
			_fuelHandlerScript.launchSinglePlayerGame();
		}
	}
}