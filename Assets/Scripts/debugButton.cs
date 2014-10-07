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
			
			GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
			mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();
			
			
			_mainmenuScript.LaunchDashBoardWithResults();
		}
	}
	
}