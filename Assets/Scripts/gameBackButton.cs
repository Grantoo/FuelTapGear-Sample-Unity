using UnityEngine;
using System.Collections;

public class gameBackButton : MonoBehaviour 
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
			
			//get main loop game state
			
			GameObject _mainLoop = GameObject.Find("MainLoop");
			MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();
			
			var _gameState = _mainloopScript.mGameState;
			
			if(_gameState == MainLoop.eGameState.Ready)
			{
				Application.LoadLevel("MainMenu");
			}
			
		}
	}
	
}