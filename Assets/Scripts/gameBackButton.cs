using UnityEngine;
using System.Collections;

public class gameBackButton : MonoBehaviour 
{
	public AudioSource clickSound;

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
			Debug.Log ("gameBackButton - left click.");
			
			clickSound.Play();
			//get main loop game state
			
			GameObject _mainLoop = GameObject.Find("MainLoop");
			MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();
			
			var _gameState = _mainloopScript.mGameState;
			
			if(_gameState == MainLoop.eGameState.Done)
			{
				Application.LoadLevel("MainMenu");
			}
			
		}
	}
	
}