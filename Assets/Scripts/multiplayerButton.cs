using UnityEngine;
using System.Collections;

public class multiplayerButton : MonoBehaviour 
{
	public Sprite imgDown;

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
			Debug.Log ("multiplayerButton - left click.");
			
			GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

			gameObject.GetComponent<SpriteRenderer>().sprite = imgDown;

			_fuelHandlerScript.launchPropeller();
		}
	}
	
}