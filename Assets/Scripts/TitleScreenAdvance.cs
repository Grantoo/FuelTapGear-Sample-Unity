using UnityEngine;
using System.Collections;

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
			Application.LoadLevel("MainMenu");
		}
	}

	
	public void RefreshDebugText()
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
		
		int _gearShapeType = _fuelHandlerScript.GearShapeType;
		float _gearFriction = _fuelHandlerScript.GearFriction;
		int _gameTime = _fuelHandlerScript.GameTime;
		
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
