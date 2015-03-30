using UnityEngine;
using System.Collections;
using System;

public class MainLoop : MonoBehaviour 
{
	
	public enum eGameState 
	{
		Init,
		Ready, 
		FirstTap, 
		Running,
		Done,
		Exit,
	};

	public AudioSource GameOverSFX;
	public int scoreValue = 0;

	
	private float gameTimerValue = 10.0f;
	private eGameState mGameState = eGameState.Init;

	private float gameoverTimer = 0.0f;
	public float gameoverTimeout = 2.0f;

	/*
	 -----------------------------------------------------
			Access to FuelHandler this pointer
	 -----------------------------------------------------
	*/
	private FuelHandler getFuelHandlerClass()
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		if (_fuelHandler != null) {
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler> ();
			if(_fuelHandlerScript != null) {
				return _fuelHandlerScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}

	public void setStartButtonText (string str) 
	{
		GameObject startbuttonObj = GameObject.Find ("startTextMesh");
		TextMesh tmesh = (TextMesh)startbuttonObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}
	public void hideStartButtonText () 
	{
		GameObject startbuttonObj = GameObject.Find ("startTextMesh");
		startbuttonObj.renderer.enabled = false;
	}

	public void updateScoreText (string str) 
	{
		GameObject scoreObj = GameObject.Find ("Score");
		TextMesh tmesh = (TextMesh)scoreObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void updateFinalScoreText (string str) 
	{
		GameObject scoreObj = GameObject.Find ("finalScore");
		TextMesh tmesh = (TextMesh)scoreObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void updateGameTimerText (string str) 
	{
		GameObject gameTimerObj = GameObject.Find ("GameTimer");
		TextMesh tmesh = (TextMesh)gameTimerObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}


	public void updateYourAvatarText (string str) 
	{
		GameObject gameObj = GameObject.Find ("yournameTextMesh");
		TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}
	public void updateTheirAvatarText (string str) 
	{
		GameObject gameObj = GameObject.Find ("theirnameTextMesh");
		TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}
	public void updateMatchRoundText (string str) 
	{
		GameObject gameObj = GameObject.Find ("matchroundTextMesh");
		TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}


	void resetDynamicData () 
	{
		FuelHandler _fuelHandlerScript = getFuelHandlerClass();

		gameTimerValue = (float)_fuelHandlerScript.GameTime;

		int _gearShapeType = _fuelHandlerScript.GearShapeType;
		float _gearFriction = _fuelHandlerScript.GearFriction;

		GameObject _gearAction = GameObject.Find("GearProxy1");
		gearAction _gearActionScript = _gearAction.GetComponent<gearAction>();
		_gearActionScript.Reset (_gearShapeType, _gearFriction);
	}

	public bool isGameOver () 
	{
		if(mGameState == eGameState.Done){
			return true;
		}

		return false;
	}

	public eGameState getGameState () 
	{
		return mGameState;
	}

	public void setGameState (eGameState state) 
	{
		mGameState = state;
	}

	void InitTextMeshObjs () 
	{
		//init tap score
		updateScoreText ( "0" );

		//init game timer
		updateGameTimerText ( gameTimerValue.ToString() + " sec" );
	}


	void Start () 
	{
		mGameState = eGameState.Init;


		//set match data
		FuelHandler _fuelHandlerScript = getFuelHandlerClass();

		GameMatchData _data = _fuelHandlerScript.getMatchData();

		if (_data.ValidMatchData == true) 
		{
			updateYourAvatarText (_data.YourNickname);
			updateTheirAvatarText (_data.TheirNickname);
			updateMatchRoundText ("Round - " + _data.MatchRound.ToString ());

			GameObject _gameObj = GameObject.Find("yourAvatar");
			StartCoroutine(downloadImgX(_data.YourAvatarURL, _gameObj));
			
			_gameObj = GameObject.Find("theirAvatar");
			StartCoroutine(downloadImgX(_data.TheirAvatarURL, _gameObj));
		} 
		else //Single Player
		{
			updateYourAvatarText ("You");
			updateTheirAvatarText ("Computer");
			updateMatchRoundText ("Round - X");
		}

		GameObject _backObj = GameObject.Find("backButton");
		_backObj.renderer.enabled = false;


		//Timeup Popup
		GameObject _timeup = GameObject.Find("TimeUpPopup");
		_timeup.renderer.enabled = false;
		_timeup = GameObject.Find("finalScore");
		_timeup.renderer.enabled = false;

		gameoverTimer = 0.0f;

	}

	IEnumerator DownloadImage(string url, Texture2D tex) 
	{
		WWW www = new WWW(url);
		yield return www;

		tex.LoadImage(www.bytes);
	}


	IEnumerator downloadImgX (string url, GameObject _gameObj)
	{
		Texture2D texture = new Texture2D(1,1);
		WWW www = new WWW(url);
		yield return www;
		www.LoadImageIntoTexture(texture);

		SpriteRenderer sr = _gameObj.GetComponent<SpriteRenderer>();

		Debug.Log ("downloadImgX - texture.width, height : " + texture.width + ", " + texture.height);
		Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		sr.sprite = image;

		if(texture.width <= 50)
		{
			_gameObj.transform.localScale = new Vector3(2.5f, 2.5f, 1.0f);
		}
		else
		{
			_gameObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.0f);
		}
		Debug.Log ("downloadImgX - tried to load image...");
	}


	void Update () 
	{
		switch (mGameState) 
		{
			case eGameState.Init:
			{
				scoreValue = 0;
				gameTimerValue = 10.0f;
		
				resetDynamicData();
				InitTextMeshObjs();

				mGameState = eGameState.Ready;
			}
			break;

			case eGameState.Ready:
			{
				//waiting for first tap
			}
			break;

			case eGameState.FirstTap:
			{
				GameObject _tapsCounter = GameObject.Find("TapsCounter");
				Animator _Animator = _tapsCounter.GetComponent<Animator>();
				_Animator.Play ("tapCountDisplayAnim2");

				_tapsCounter = GameObject.Find("MPSCounter");
				_Animator = _tapsCounter.GetComponent<Animator>();
				_Animator.Play ("tapCountDisplayAnim2");
					
				mGameState = eGameState.Running;
			}
			break;



			case eGameState.Running:
			{
				//update gameTimer
				gameTimerValue -= Time.deltaTime;


				GameObject _gearAction = GameObject.Find("GearProxy1");
				gearAction _gearActionScript = _gearAction.GetComponent<gearAction>();

				if(gameTimerValue <= 0)
				{
					mGameState = eGameState.Done;
					gameTimerValue = 0.0f;

					hideStartButtonText();

					int maxspeed = (int)_gearActionScript.maxspinvelocity;

					FuelHandler _fuelHandlerScript = getFuelHandlerClass();

					_fuelHandlerScript.SetMatchScore(scoreValue, maxspeed);

					GameObject _backObj = GameObject.Find("backButton");
					_backObj.renderer.enabled = true;


					//reset animations
					GameObject _tapsCounter = GameObject.Find("TapsCounter");
					Animator _Animator = _tapsCounter.GetComponent<Animator>();
					_Animator.Play ("tapCountDisplayAnim");

					_tapsCounter = GameObject.Find("MPSCounter");
					_Animator = _tapsCounter.GetComponent<Animator>();
					_Animator.Play ("tapCountDisplayAnim");


					//bring the system to a grinding halt
					_gearActionScript.SetFriction(0.96f);
					_gearActionScript.ClearActiveBonuses();

					GameOverSFX.Play();

					//Timeup Popup
					GameObject _timeup = GameObject.Find("TimeUpPopup");
					_timeup.renderer.enabled = true;

					updateFinalScoreText(scoreValue.ToString());
					_timeup = GameObject.Find("finalScore");
					_timeup.renderer.enabled = true;


					//another complete game session
					if (PlayerPrefs.HasKey ("numSessions")) 
					{
						int numSessions = PlayerPrefs.GetInt ("numSessions");
						numSessions++;
						PlayerPrefs.SetInt("numSessions", numSessions);
					}
				}

				//update game timer
				if(_gearActionScript.Check5secBonus() == true)
				{
					gameTimerValue += 5.0f;
				}

				updateGameTimerText ( gameTimerValue.ToString("0") + " sec" );
		
				updateScoreText ( scoreValue.ToString() );


				GameObject spinTextObj = GameObject.Find ("speedTextMesh");
				TextMesh tmesh = (TextMesh)spinTextObj.GetComponent (typeof(TextMesh)); 
				tmesh.text = _gearActionScript.velocityStr;

			}
			break;

			case eGameState.Done:
			{
				//timeout to return to main menu

				gameoverTimer += Time.deltaTime;
				if(gameoverTimer >= gameoverTimeout)
				{
					Application.LoadLevel("MainMenu");

					mGameState = eGameState.Exit;
				}
			}
			break;

			case eGameState.Exit:
			{

			}
			break;
		}

	}
}
