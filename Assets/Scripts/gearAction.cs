using UnityEngine;
using System.Collections;

public class gearAction : MonoBehaviour 
{
	public Sprite img0, img1, img2, img3, img4, img5;

	public GameObject spinTextObj;

	public int buttonDebounce = 0;
	public float minTapDelta = 0.5f;
	public float spinvelocity = 0.0f;
	public float maxspinvelocity = 0.0f;
	public string velocityStr = "0mps";
	private float angle = 0.0f;
	private float friction = 0.995f;


	public void updateSpinText (string str) 
	{
		spinTextObj = GameObject.Find ("speedTextMesh");
		TextMesh tmesh = (TextMesh)spinTextObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void Reset (int _gearType, float _friction) 
	{
		spinvelocity = 0.0f;
		maxspinvelocity = 0.0f;
		angle = 0.0f;
		friction = _friction;

		GameObject shadow = GameObject.Find ("GearShadow");

		switch (_gearType) 
		{
			case 0:
				gameObject.GetComponent<SpriteRenderer>().sprite = img0;
				shadow.GetComponent<SpriteRenderer>().sprite = img0;
			break;
			case 1:
				gameObject.GetComponent<SpriteRenderer>().sprite = img1;
				shadow.GetComponent<SpriteRenderer>().sprite = img1;
			break;
			case 2:
				gameObject.GetComponent<SpriteRenderer>().sprite = img2;
				shadow.GetComponent<SpriteRenderer>().sprite = img2;
			break;
			case 3:
				gameObject.GetComponent<SpriteRenderer>().sprite = img3;
				shadow.GetComponent<SpriteRenderer>().sprite = img3;
			break;
			case 4:
				gameObject.GetComponent<SpriteRenderer>().sprite = img4;
				shadow.GetComponent<SpriteRenderer>().sprite = img4;
			break;
			case 5:
				gameObject.GetComponent<SpriteRenderer>().sprite = img5;
				shadow.GetComponent<SpriteRenderer>().sprite = img5;
			break;
		}
	}

	void Start () 
	{

		GameObject MultTimesTwo = GameObject.Find ("MultTimesTwo");
		GameObject bonus = (GameObject) Instantiate(MultTimesTwo, transform.position, transform.rotation);

	}
	
	void Update () 
	{

		if (Input.GetMouseButtonUp (0)) 
		{
			buttonDebounce = 0;
		}

		angle -= spinvelocity;
		transform.rotation = Quaternion.Euler(0, 0, angle);

		GameObject shadow = GameObject.Find ("GearShadow");
		shadow.transform.rotation = Quaternion.Euler(0, 0, angle);

		//updateSpinText ( spinvelocity.ToString ("0.00") + " mps" );
		velocityStr = maxspinvelocity.ToString ("0.00") + " mps";


		spinvelocity *= friction;


		UpdateBackGroundGears ();

	}

	void UpdateBackGroundGears () 
	{
		GameObject gear = GameObject.Find ("OrangeGear");
		gearMount _gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmBlueGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmBlueGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmOrangeGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);
		
		gear = GameObject.Find ("SmOrangeGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmOrangeGearTrans");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGearTrans");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGearTrans2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);




	}


	void OnMouseOver () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			if(buttonDebounce == 0)
			{
				GameObject _mainLoop = GameObject.Find("MainLoop");
				MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();
			
				var _gameState = _mainloopScript.mGameState;


				if(_gameState == MainLoop.eGameState.Ready)
				{
					//first tap, tell mainloop to start
					_mainloopScript.mGameState = MainLoop.eGameState.FirstTap;
					_gameState = _mainloopScript.mGameState;

					_mainloopScript.setStartButtonText("Tap! Fast, fast, faster!!");
				}

				if(_gameState == MainLoop.eGameState.Running)
				{
					_mainloopScript.scoreValue++;
			
					buttonDebounce = 1;

					//increase spin speed
					var currentTime = Time.deltaTime;
					var increase = minTapDelta - currentTime;

					if(currentTime < minTapDelta)
					{
						increase = minTapDelta - currentTime;
					}
					else
					{
						increase = 0.0f;
					}

					spinvelocity += increase;

					if(spinvelocity > maxspinvelocity)
					{
						maxspinvelocity = spinvelocity;
					}

					//Debug.Log ("Gear Tap - spinvelocity = " + spinvelocity);
				}
			}
		}
	}
}




