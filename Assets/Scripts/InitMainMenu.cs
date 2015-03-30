using UnityEngine;
using System.Collections;
using System;

public class InitMainMenu : MonoBehaviour 
{
	
	void Start () 
	{
		Debug.Log ("InitMainMenu!");

		AddNotificationObservers();

		ClearGraphics();

		ResetDebugText();

		//get Fuel Dynamics Handler
		//GameObject _fuelDynamicsHandler = GameObject.Find("FuelDynamicsHandlerObject");
		//FuelDynamicsHandler _fuelDynamicsHandlerScript = _fuelDynamicsHandler.GetComponent<FuelDynamicsHandler>();
		//_fuelHandlerScript.setUserConditions();

		RefreshDebugText();
		RefreshGoldCount(0);
		RefreshOilCount(0);
		RefreshHiScore();

		FuelHandler _fuelHandlerScript = getFuelHandlerClass();
		_fuelHandlerScript.syncUserValues();
		_fuelHandlerScript.SyncChallengeCounts();
		_fuelHandlerScript.SyncTournamentInfo();
		_fuelHandlerScript.SyncVirtualGoods();
		_fuelHandlerScript.updateLoginText();
		_fuelHandlerScript.tryLaunchFuelSDK();
	}

	
	public void AddNotificationObservers()
	{
		NotificationCenter.DefaultCenter.AddObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshChallengeCount");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshTournamentInfo");
		NotificationCenter.DefaultCenter.AddObserver(this, "AddTransOverlay");
		NotificationCenter.DefaultCenter.AddObserver(this, "SubTransOverlay");
	}

	public void RemoveNotificationObservers()
	{
		NotificationCenter.DefaultCenter.RemoveObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshChallengeCount");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshTournamentInfo");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "AddTransOverlay");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "SetTransOverlay");
	}


	public void LaunchGamePlay()
	{
		RemoveNotificationObservers ();
		
		Application.LoadLevel("GamePlay");
	}
	
	
	public void RefreshChallengeCount(Hashtable ccTable)
	{
		// retrieve a value for the given key
		int ccount = (int)ccTable["cc"];    

		//show challenge count pieces and set count values
		GameObject gameObj = GameObject.Find("ccbacking");
		gameObj.renderer.enabled = true;

		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = ccount.ToString();
		tmesh.renderer.enabled = true;
	}


	public void RefreshTournamentInfo(Hashtable tournyTable)
	{
		bool enabled = (bool)tournyTable["enabled"]; 

		/* need to add screen elements for the following: */
		//string tournyname = (string)tournyTable["tournyname"];    
		//string startDate = (string)tournyTable["startDate"];    
		//string endDate = (string)tournyTable["endDate"];    

		GameObject gameObj = GameObject.Find ("Trophy");
		
		if(enabled == false){
			gameObj.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);
		} else {
			gameObj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
		}
	}


	public void RefreshVirtualGoods(Hashtable goodsTable)
	{
		int addGold = (int)goodsTable["addGold"]; 
		int addOil= (int)goodsTable["addOil"]; 

		RefreshGoldCount(addGold);
		RefreshOilCount(addOil);

		VirtualGoodsFanFare ();
	}

	
	public void RefreshGoldCount(int addAmount)
	{
		if (PlayerPrefs.HasKey ("userGold")) {
			var _gold = PlayerPrefs.GetInt("userGold");

			if(addAmount > 0) {
				_gold += addAmount;
				PlayerPrefs.SetInt("userGold", _gold);
			}
			
			GameObject gameObj = GameObject.Find ("goldTextMesh");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _gold.ToString();
		}
	}


	public void RefreshOilCount(int addAmount)
	{
		if (PlayerPrefs.HasKey ("oildrops")) {
			var _oildrops = PlayerPrefs.GetInt("oildrops");
			
			if(addAmount > 0) {
				_oildrops += addAmount;
				PlayerPrefs.SetInt("oildrops", _oildrops);
			}
			
			GameObject gameObj = GameObject.Find ("oilTextMesh");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _oildrops.ToString();
		}
	}


	public void RefreshHiScore()
	{
		if (PlayerPrefs.HasKey ("hiScore")) {
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			int score = _fuelHandlerScript.getMatchData().MatchScore;
			var _score = PlayerPrefs.GetInt ("hiScore");

			if (score > _score) {
				_score = score;
				PlayerPrefs.SetInt ("hiScore", _score);
			}
			
			GameObject gameObj = GameObject.Find ("HiScore");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = _score.ToString ();
		}
	}


	private void VirtualGoodsFanFare()
	{
		GameObject gameObj = GameObject.Find ("VirtualGoodsParticles");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 

		psystem.Play();
	}

	
	public void ResetDebugText()
	{
		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "Dynamics";
		tmesh.renderer.enabled = true;
		
		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "returning";
		tmesh.renderer.enabled = true;
		
		textMesh = GameObject.Find ("DebugText3");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "null";
		tmesh.renderer.enabled = true;

		textMesh = GameObject.Find ("DebugText4");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "split1name";
		tmesh.renderer.enabled = true;
	}


	public void RefreshDebugText()
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
		
		int _gearShapeType = _fuelHandlerScript.GearShapeType;
		float _gearFriction = _fuelHandlerScript.GearFriction;
		int _gameTime = _fuelHandlerScript.GameTime;
		string _split1name = _fuelHandlerScript.Split1Name;

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

		textMesh = GameObject.Find ("DebugText4");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "split1name = " + _split1name.ToString();
		tmesh.renderer.enabled = true;
	}


	void Update () 
	{

	}


	void AddTransOverlay () 
	{
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.transform.position = new Vector3(2.0f, 1.0f, 0.0f);
		gameObj.renderer.enabled = true;
	}


	void SubTransOverlay () 
	{
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.transform.position = new Vector3 (-24.0f, 1.0f, 0.0f);
		gameObj.renderer.enabled = false;
	}


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
	

	private void ClearGraphics()
	{
		//trans overlay
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.renderer.enabled = false;

		//init particles to off
		GameObject particleObj = GameObject.Find ("VirtualGoodsParticles");
		ParticleSystem psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		//hide challenge count pieces
		gameObj = GameObject.Find("ccbacking");
		gameObj.renderer.enabled = false;
		
		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.renderer.enabled = false;
		
		//hide trophy
		gameObj = GameObject.Find ("Trophy");
		gameObj.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);
	}

}
