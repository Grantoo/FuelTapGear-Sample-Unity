using UnityEngine;
using System.Collections;

public class gearAction : MonoBehaviour 
{
	public GameObject spinTextObj;

	public int buttonDebounce = 0;
	public float minTapDelta = 0.5f;
	public float spinvelocity = 0.0f;
	public float maxspinvelocity = 0.0f;
	private float angle = 0.0f;
	private float friction = 0.995f;


	public void updateSpinText (string str) 
	{
		spinTextObj = GameObject.Find ("speedTextMesh");
		TextMesh tmesh = (TextMesh)spinTextObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void Reset () 
	{
		spinvelocity = 0.0f;
		maxspinvelocity = 0.0f;
		angle = 0.0f;
	}

	void Start () 
	{
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

		updateSpinText ( spinvelocity.ToString ("0.00") + " mps" );



		spinvelocity *= friction;

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
					_mainloopScript.mGameState = MainLoop.eGameState.Running;
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




