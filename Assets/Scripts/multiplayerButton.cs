using UnityEngine;
using System.Collections;

public class multiplayerButton : MonoBehaviour 
{
	public Sprite imgDown;
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

			GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

			gameObject.GetComponent<SpriteRenderer>().sprite = imgDown;

			_fuelHandlerScript.launchPropeller();
		}
	}
	
}