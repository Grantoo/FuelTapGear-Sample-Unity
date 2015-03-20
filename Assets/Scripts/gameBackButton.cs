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
			clickSound.Play();
		
			GameObject _mainLoop = GameObject.Find("MainLoop");
			MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();

			if(_mainloopScript.isGameOver() == true)
			{
				Application.LoadLevel("MainMenu");
			}
		}
	}
	
}