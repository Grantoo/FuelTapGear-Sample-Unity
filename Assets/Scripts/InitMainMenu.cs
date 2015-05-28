using UnityEngine;
using System.Collections;
using System;

public class InitMainMenu : MonoBehaviour 
{
	public static bool sComingFromTitle = false;
	public static bool sComingFromGame = false;


	void Start () 
	{
		Debug.Log ("InitMainMenu Start");

		AddNotificationObservers();

		ClearGraphics();

		ResetDebugText();

		RefreshDebugText();
		RefreshGoldCount(0);
		RefreshOilCount(0);
		RefreshHiScore();

		UpdateFBIcon ();

		PropellerProduct _propellerProductScript = getPropellerProductClass();
		_propellerProductScript.tryLaunchFuelSDK();
		_propellerProductScript.updateLoginText ();

		if (sComingFromTitle == true) {
			UpdatePropellerSDK();
			sComingFromTitle = false;
		}
	}

	public void UpdatePropellerSDK()
	{
		PropellerProduct _propellerProductScript = getPropellerProductClass();
		_propellerProductScript.syncUserValues();
		_propellerProductScript.SyncChallengeCounts();
		_propellerProductScript.SyncTournamentInfo();
		_propellerProductScript.SyncVirtualGoods();
		_propellerProductScript.updateLoginText();
	}


	public void AddNotificationObservers()
	{
		NotificationCenter.DefaultCenter.AddObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.AddObserver(this, "UpdatePropellerSDK");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshChallengeCount");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshTournamentInfo");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshVirtualGoods");
		NotificationCenter.DefaultCenter.AddObserver(this, "AddTransOverlay");
		NotificationCenter.DefaultCenter.AddObserver(this, "SubTransOverlay");
	}

	public void RemoveNotificationObservers()
	{
		NotificationCenter.DefaultCenter.RemoveObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "UpdatePropellerSDK");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshChallengeCount");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshTournamentInfo");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshVirtualGoods");
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

		bool enabled = false;
		if (ccount > 0)
			enabled = true;

		//show challenge count pieces and set count values
		GameObject gameObj = GameObject.Find("ccbacking");
		gameObj.GetComponent<Renderer>().enabled = enabled;

		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = ccount.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;
	}


	public void RefreshTournamentInfo(Hashtable tournyTable)
	{
		Debug.Log ("enabled");
		bool enabled = (bool)tournyTable["running"]; 
		Debug.Log ("tournyname");
		string tournyname = (string)tournyTable["tournyname"];    
		Debug.Log ("startDate");
		string startDate = (string)tournyTable["startDate"];    
		Debug.Log ("endDate");
		string endDate = (string)tournyTable["endDate"];    

		GameObject gameObj = GameObject.Find ("Trophy");
		gameObj.GetComponent<Renderer>().enabled = enabled;

		gameObj = GameObject.Find ("TournyNameText");
		TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = tournyname.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		long t = Convert.ToInt64 (startDate);
		DateTime date = FromUnixTime (t);
		gameObj = GameObject.Find ("StartDateText");
		tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = date.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		t = Convert.ToInt64 (endDate);
		date = FromUnixTime (t);
		gameObj = GameObject.Find ("EndDateText");
		tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = date.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;
	}


	public void RefreshVirtualGoods(Hashtable goodsTable)
	{
		int addGold = (int)goodsTable["addGold"]; 
		int addOil= (int)goodsTable["addOil"]; 
		int showTrophy= (int)goodsTable["showTrophy"]; 

		if (addGold > 0) {
			RefreshGoldCount(addGold);

			GameObject gameObj = GameObject.Find ("VirtualGoodGoldParticles");
			ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
			psystem.Play();
		}

		if (addOil > 0) {
			RefreshOilCount(addOil);

			GameObject gameObj = GameObject.Find ("VirtualGoodOilParticles");
			ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
			psystem.Play();
		}

		if (showTrophy > 0) {
			ShowTrophy();
			
			GameObject gameObj = GameObject.Find ("VirtualGoodTrophyParticles");
			ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
			psystem.Play();
		}

	}

	private void ShowTrophy()
	{
		GameObject gameObj = GameObject.Find ("ShowTrophy");
		gameObj.GetComponent<Renderer>().enabled = true;
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

			PropellerProduct _propellerProductScript = getPropellerProductClass();
			GameMatchData _gameMatchData = _propellerProductScript.getMatchData();
			int score = _gameMatchData.MatchScore;
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



	
	public void ResetDebugText()
	{
		PropellerProduct _propellerProductScript = getPropellerProductClass();
		int showdebug = _propellerProductScript.getShowDebug();
		bool enabled = false;
		if (showdebug == 1)
			enabled = true;

		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "Dynamics";
		tmesh.GetComponent<Renderer>().enabled = enabled;
		
		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "returning";
		tmesh.GetComponent<Renderer>().enabled = enabled;
		
		textMesh = GameObject.Find ("DebugText3");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "null";
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("DebugText4");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "split1name";
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("VersionText");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = enabled;

	}


	public void RefreshDebugText()
	{
		PropellerProduct _propellerProductScript = getPropellerProductClass();
		int _gameTime = (int)_propellerProductScript.getGameTime ();
		int _gearShapeType = _propellerProductScript.getGearShapeType ();
		float _gearFriction = _propellerProductScript.getGearFriction ();
		string _split1name = _propellerProductScript.getSplit1Name();

		int showdebug = _propellerProductScript.getShowDebug();
		bool enabled = false;
		if (showdebug == 1)
			enabled = true;

		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "friction = " + _gearFriction.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "geartype = " + _gearShapeType.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("DebugText3");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "gametime = " + _gameTime.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("DebugText4");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.text = "split1name = " + _split1name.ToString();
		tmesh.GetComponent<Renderer>().enabled = enabled;

		textMesh = GameObject.Find ("VersionText");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = enabled;
	}


	void Update () 
	{

	}


	void AddTransOverlay () 
	{
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.transform.position = new Vector3(2.0f, 1.0f, 0.0f);
		gameObj.GetComponent<Renderer>().enabled = true;
	}


	void SubTransOverlay () 
	{
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.transform.position = new Vector3 (-24.0f, 1.0f, 0.0f);
		gameObj.GetComponent<Renderer>().enabled = false;
	}


	private PropellerProduct getPropellerProductClass()
	{
		GameObject _propellerProductObj = GameObject.Find("PropellerProduct");
		if (_propellerProductObj != null) {
			PropellerProduct _propellerProductScript = _propellerProductObj.GetComponent<PropellerProduct> ();
			if(_propellerProductScript != null) {
				return _propellerProductScript;
			}
			Debug.Log ("getPropellerProductClass Exception : _propellerProductScript = null");
			throw new Exception();
		}
		Debug.Log ("getPropellerProductClass Exception : _propellerProductObj = null");
		throw new Exception();
	}
	

	private void ClearGraphics()
	{
		//trans overlay
		GameObject gameObj = GameObject.Find("transoverlay");
		gameObj.GetComponent<Renderer>().enabled = false;

		//init particles to off
		GameObject particleObj = GameObject.Find ("VirtualGoodGoldParticles");
		ParticleSystem psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		particleObj = GameObject.Find ("VirtualGoodOilParticles");
		psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		//hide challenge count pieces
		gameObj = GameObject.Find("ccbacking");
		gameObj.GetComponent<Renderer>().enabled = false;
		
		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = false;
		
		//hide trophy
		gameObj = GameObject.Find ("Trophy");
		gameObj.GetComponent<Renderer>().enabled = false;

		//parcipitation trophy
		gameObj = GameObject.Find ("ShowTrophy");
		gameObj.GetComponent<Renderer>().enabled = false;

		gameObj = GameObject.Find ("TournyNameText");
		tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = false;
		
		gameObj = GameObject.Find ("StartDateText");
		tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = false;
		
		gameObj = GameObject.Find ("EndDateText");
		tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
		tmesh.GetComponent<Renderer>().enabled = false;


		PropellerProduct _propellerProductScript = getPropellerProductClass();

		bool _isDynamics = _propellerProductScript.isDynamicsOnly ();
		if (_isDynamics == true) 
		{
			gameObj = GameObject.Find("buttonMultiPlayer");
			gameObj.transform.position = new Vector3 (-24.0f, 1.0f, 0.0f);
			gameObj.GetComponent<Renderer>().enabled = false;
		}
	}

	private DateTime FromUnixTime(long unixTime)
	{
		var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		return epoch.AddSeconds(unixTime);
	}


	void UpdateFBIcon () 
	{
		PropellerProduct _propellerProductScript = getPropellerProductClass();
		int fbicon = _propellerProductScript.getFBIcon();
		if (fbicon == 0) 
		{
			GameObject gameObj = GameObject.Find ("FBButton");
			gameObj.transform.position = new Vector3 (13.08f, -5.14f, 5.0f);
			gameObj.GetComponent<Renderer>().enabled = true;
			gameObj = GameObject.Find ("LoginStatusText");
			gameObj.transform.position = new Vector3 (13.08f, -3.41f, 4.0f);
			gameObj.GetComponent<Renderer>().enabled = true;
		}
	}


}
