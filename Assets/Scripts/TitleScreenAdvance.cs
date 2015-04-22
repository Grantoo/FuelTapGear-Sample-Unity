using UnityEngine;
using System.Collections;
using System;

public class TitleScreenAdvance : MonoBehaviour 
{
	
	public float advanceTimerValue = 0.0f;

	private bool firstPass = true;


	void Start () 
	{
		RefreshDebugText();


	}


	void Update () 
	{
		advanceTimerValue += Time.deltaTime;
		if (advanceTimerValue > 16.0f) 
		{
			Application.LoadLevel("MainMenu");
		}

		if (firstPass == true && advanceTimerValue > 0.5f) 
		{
			RefreshDebugText();

			firstPass = false;
		}

		if (Input.GetMouseButtonDown (0)) 
		{
			//tell main menu we're coming
			InitMainMenu.sComingFromTitle = true;

			Application.LoadLevel("MainMenu");
		}
	}

	private PropellerProduct getPropellerProductClass()
	{
		GameObject _propellerProductObj = GameObject.Find("PropellerProduct");
		if (_propellerProductObj != null) {
			PropellerProduct _propellerProductScript = _propellerProductObj.GetComponent<PropellerProduct> ();
			if(_propellerProductScript != null) {
				return _propellerProductScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}

	public void RefreshDebugText()
	{
		PropellerProduct _propellerProductScript = getPropellerProductClass();
		int _gameTime = (int)_propellerProductScript.getGameTime ();
		int _gearShapeType = _propellerProductScript.getGearShapeType ();
		float _gearFriction = _propellerProductScript.getGearFriction ();
		int showdebug = _propellerProductScript.getShowDebug();
		bool enabled = false;
		if (showdebug == 1)
			enabled = true;


		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "friction = " + _gearFriction.ToString();
		tmesh.renderer.enabled = enabled;
		
		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "geartype = " + _gearShapeType.ToString();
		tmesh.renderer.enabled = enabled;
		
		textMesh = GameObject.Find ("DebugText3");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "gametime = " + _gameTime.ToString();
		tmesh.renderer.enabled = enabled;

		textMesh = GameObject.Find ("DebugText4");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "gametime = " + _gameTime.ToString();
		tmesh.renderer.enabled = enabled;

		textMesh = GameObject.Find ("Environment");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.renderer.enabled = enabled;

		textMesh = GameObject.Find ("Complete");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.renderer.enabled = enabled;

		textMesh = GameObject.Find ("UserPrefs");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.renderer.enabled = enabled;
	}

}
