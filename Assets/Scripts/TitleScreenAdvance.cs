using UnityEngine;
using System.Collections;
using System;

public class TitleScreenAdvance : MonoBehaviour 
{
	
	public float advanceTimerValue = 0.0f;

	private bool firstPass = true;


	void Start () 
	{


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
		//string _split1name = _propellerProductScript.getSplit1Name();

		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "friction = " + _gearFriction.ToString();
		tmesh.renderer.enabled = true;
		
		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "geartype = " + _gearShapeType.ToString();
		tmesh.renderer.enabled = true;
		
		textMesh = GameObject.Find ("DebugText3");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "gametime = " + _gameTime.ToString();
		tmesh.renderer.enabled = true;
	}

}
