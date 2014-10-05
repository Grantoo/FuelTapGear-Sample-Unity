using UnityEngine;
using System.Collections;

public class MainLoop : MonoBehaviour 
{
	
	public enum eGameState 
	{
		Init,
		Ready, 
		Running,
		Done,
	};

	public int scoreValue = 0;
	public float gameTimerValue = 5.0f;

	//access to other game objects and their scripts
	public GameObject scoreObj;
	public GameObject gameTimerObj;
	private GameObject startbuttonObj;

	public eGameState mGameState = eGameState.Init;


	public void setStartButtonText (string str) 
	{
		startbuttonObj = GameObject.Find ("startTextMesh");
		TextMesh tmesh = (TextMesh)startbuttonObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void updateScoreText (string str) 
	{
		scoreObj = GameObject.Find ("Score");
		TextMesh tmesh = (TextMesh)scoreObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void updateGameTimerText (string str) 
	{
		gameTimerObj = GameObject.Find ("GameTimer");
		TextMesh tmesh = (TextMesh)gameTimerObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	void resetGear () 
	{
		GameObject _gearAction = GameObject.Find("GearProxy1");
		gearAction _gearActionScript = _gearAction.GetComponent<gearAction>();

		_gearActionScript.Reset ();
	}




	void InitTextMeshObjs () 
	{
		//init tap score
		updateScoreText ( "0" );

		//init game timer
		updateGameTimerText ( "5.000000" );

		//init start button text
		setStartButtonText ("Start");
	}

	void Start () 
	{
		mGameState = eGameState.Init;

		//InitTextMeshObjs();
	}
	
	void Update () 
	{
		switch (mGameState) 
		{
			case eGameState.Init:
			{
				scoreValue = 0;
				gameTimerValue = 5.0f;

				InitTextMeshObjs();
				resetGear();

				mGameState = eGameState.Ready;
			}
			break;

			case eGameState.Ready:
			{
			}
			break;

			case eGameState.Running:
			{
				//update gameTimer
				gameTimerValue -= Time.deltaTime;

				if(gameTimerValue <= 0)
				{
					mGameState = eGameState.Done;
					gameTimerValue = 0.0f;
					setStartButtonText ("Reset");
				}

				//update game timer
				updateGameTimerText ( gameTimerValue.ToString() );
		
				//update tap score
				updateScoreText ( scoreValue.ToString() );

			}
			break;

			case eGameState.Done:
			{
			}
			break;

		}

	}
}