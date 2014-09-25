using UnityEngine;
using System.Collections;

public class startButton : MonoBehaviour 
{
	private GameObject startbuttonObj;

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
			//get main loop game state
			GameObject _mainLoop = GameObject.Find("MainLoop");
			MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();

			var _gameState = _mainloopScript.mGameState;

			if(_gameState == MainLoop.eGameState.Ready)
			{
				_mainloopScript.mGameState = MainLoop.eGameState.Running;
				_mainloopScript.setStartButtonText("Tap!!");
			}
			else if(_gameState == MainLoop.eGameState.Done)
			{
				_mainloopScript.mGameState = MainLoop.eGameState.Init;
				_mainloopScript.setStartButtonText("Start");
			}
		}
	}

}