using UnityEngine;
using System.Collections;
using System;

public class playButton : MonoBehaviour 
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
			
			PropellerProduct _propellerProduct = getPropellerProductClass();

			gameObject.GetComponent<SpriteRenderer>().sprite = imgDown;

			_propellerProduct.launchSinglePlayerGame();
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

}