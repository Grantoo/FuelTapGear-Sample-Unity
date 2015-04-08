using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Facebook.MiniJSON;
using SimpleJSON;



public class DynamicsHandler : MonoBehaviour 
{
	public static DynamicsHandler Instance { get; private set; }

	public float GearFriction { get; set; }
	public int GearShapeType { get; set; }
	public int GameTime { get; set; }
	public string Split1Name { get; set; }

	private GameMatchData m_matchData;

	public GameMatchData getMatchData()
	{
		return m_matchData;
	}

	/*
	 -----------------------------------------------------
							Awake
	 -----------------------------------------------------
	*/
	void Awake ()
	{
		Debug.Log ("Awake");
		
		if (Instance != null && Instance != this) 
		{
			//destroy other instances
			Destroy (gameObject);
		} 
		else if( Instance == null )
		{
		}
		
		Instance = this;
		
		DontDestroyOnLoad(gameObject);
	}
	
	
	/*
	 -----------------------------------------------------
							Start
	 -----------------------------------------------------
	*/
	void Start () 
	{
		Debug.Log ("Start");

		GearFriction = 0.98f;
		GearShapeType = 5;
		GameTime = 7;
		Split1Name = "none";
		
		//get stored dynamic values
		if (PlayerPrefs.HasKey ("gearfriction")) {
			GearFriction = PlayerPrefs.GetFloat("gearfriction");
		}
		if (PlayerPrefs.HasKey ("geartype")) {
			GearShapeType = PlayerPrefs.GetInt("geartype");
		}
		if (PlayerPrefs.HasKey ("gametime")) {
			GameTime = PlayerPrefs.GetInt("gametime");
		}
		if (PlayerPrefs.HasKey ("splitgroup")) {
			Split1Name = PlayerPrefs.GetString("splitgroup");
		}
		
		
		if (PlayerPrefs.HasKey ("numLaunches")) 
		{
			int numLaunches = PlayerPrefs.GetInt ("numLaunches");
			numLaunches++;
			PlayerPrefs.SetInt("numLaunches", numLaunches);
		}
		
		setUserConditions ();
		
	}
	
	public void SetMatchScore(int scoreValue, int speedValue)
	{
		Debug.Log ("SetMatchScore = " + scoreValue);
		
		m_matchData.MatchScore = scoreValue;
		m_matchData.MatchMaxSpeed = speedValue;
		
		//hmmm, better clean up this check
		if (m_matchData.ValidMatchData == true) 
		{
			m_matchData.MatchComplete = true;
		}
	}

	/*
	 -----------------------------------------------------
							Update
	 -----------------------------------------------------
	*/
	void Update () 
	{
		
		
	}
	
	
	/*
	 -----------------------------------------------------
			Access to mainmenu this pointer
	 -----------------------------------------------------
	*/
	private InitMainMenu getMainMenuClass()
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		if (_mainmenu != null) {
			InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu> ();
			if(_mainmenuScript != null) {
				return _mainmenuScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}


	public const int MATCH_TYPE_SINGLE = 0;

	public void launchSinglePlayerGame()
	{
		m_matchData.MatchType = MATCH_TYPE_SINGLE;
		m_matchData.ValidMatchData = false;
		
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "LaunchGamePlay");
	}

	/*
	 ---------------------------------------------------------------------
								Fuel Dynamics
	 ---------------------------------------------------------------------
    */
	private bool isDeviceTablet ()
	{
		bool isTablet = false;
		
		#if UNITY_IPHONE
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			
			if(iPhone.generation == iPhoneGeneration.iPadUnknown ||
			   iPhone.generation == iPhoneGeneration.iPad1Gen ||
			   iPhone.generation == iPhoneGeneration.iPad2Gen ||
			   iPhone.generation == iPhoneGeneration.iPad3Gen ||
			   iPhone.generation == iPhoneGeneration.iPad4Gen ||
			   iPhone.generation == iPhoneGeneration.iPadMini1Gen)
			{
				isTablet = true;
			}
		}
		#endif
		
		return isTablet;
	}
	private int getNumLaunches ()
	{
		int numLaunches = 0;
		if (PlayerPrefs.HasKey ("numLaunches")) 
		{
			numLaunches = PlayerPrefs.GetInt ("numLaunches");
		}
		Debug.Log ("....numLaunches = " + numLaunches);
		return numLaunches;
	}
	
	private int getNumSessions ()
	{
		int numSessions = 0;
		if (PlayerPrefs.HasKey ("numSessions")) 
		{
			numSessions = PlayerPrefs.GetInt ("numSessions");
		}
		Debug.Log ("....numSessions = " + numSessions);
		return numSessions;
	}
	
	private int getUserAge ()
	{
		TimeSpan span = DateTime.Now.Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
		int _currentTimeStamp = (int)span.TotalSeconds;
		
		int _seconds = 0;
		if (PlayerPrefs.HasKey ("installTimeStamp")) 
		{
			int _installTimeStamp = PlayerPrefs.GetInt ("installTimeStamp");
			_seconds = _currentTimeStamp - _installTimeStamp;
		}
		
		int userAge = _seconds / 86400;
		
		Debug.Log ("....userAge = " + userAge + " <--- " + _seconds + "/ 86400");
		
		return userAge;
	}
	
	public void setUserConditions ()
	{
		Debug.Log ("setUserConditions");
		
		String isTablet = "FALSE";
		if( isDeviceTablet() == true) {
			isTablet = "TRUE";
		}
		
		int userAge = getUserAge ();
		int numLaunches = getNumLaunches ();
		int numSessions = getNumSessions ();
		
		float _latitude = 0.0f;
		float _longitude = 0.0f;
		if (Input.location.status == LocationServiceStatus.Running) 
		{
			_latitude = Input.location.lastData.latitude;
			_longitude = Input.location.lastData.longitude;
		}
		
		Dictionary<string, object> conditions = new Dictionary<string, object> ();
		
		//required
		conditions.Add ("userAge", userAge.ToString());
		conditions.Add ("numSessions", numSessions.ToString());
		conditions.Add ("numLaunches", numLaunches.ToString());
		conditions.Add ("isTablet", isTablet);
		
		//standardized
		conditions.Add ("orientation", "portrait");
		conditions.Add ("daysSinceFirstPayment", "-1");
		conditions.Add ("daysSinceLastPayment", "-1");
		conditions.Add ("language", "en");
		conditions.Add ("gender", "female");
		conditions.Add ("age", "16");
		conditions.Add ("gpsLong", _latitude.ToString());
		conditions.Add ("gpsLat", _longitude.ToString());
		
		//game conditions
		conditions.Add ("gameVersion", "tapgear v1.1");
		
		PropellerSDK.SetUserConditions (conditions);
		
		Debug.Log 
			(
				"*** conditions ***" + "\n" +
				"userAge = " + userAge.ToString() + "\n" +
				"numSessions = " + numSessions.ToString() + "\n" +
				"numLaunches = " + numLaunches.ToString() + "\n" +
				"isTablet = " + isTablet + "\n" +
				"orientation = " + "portrait" + "\n" +
				"daysSinceFirstPayment = " + "-1" + "\n" +
				"daysSinceLastPayment = " + "-1" + "\n" +
				"language = " + "en" + "\n" +
				"gender = " + "female" + "\n" +
				"age = " + "16" + "\n" +
				"gpsLong = " + _latitude.ToString() + "\n" +
				"gpsLat = " + _longitude.ToString() + "\n" +
				"gameVersion = " + "tapgear v1.1"
				);
		
	}
	
	public void syncUserValues()
	{
		PropellerSDK.SyncUserValues();
	}
	
	public void OnPropellerSDKUserValues (Dictionary<string, object> userValuesInfo)
	{
		//Game Values - defined in the CSV
		String _friction = "friction";
		String _geartype = "geartype";
		String _gametime = "gametime";
		String _split1name = "split1name";
		
		Dictionary<string, string> analyticResult = new Dictionary<string, string> ();
		
		foreach(KeyValuePair<string, object> entry in userValuesInfo)
		{
			if(_friction.Equals( entry.Key ))
			{
				string friction = (string) entry.Value;
				GearFriction = float.Parse(friction);
			}
			else if(_geartype.Equals( entry.Key ))
			{
				string geartypeStr = (string) entry.Value;
				GearShapeType = int.Parse(geartypeStr);
			}
			else if(_gametime.Equals( entry.Key ))
			{
				string gametimeStr = (string) entry.Value;
				GameTime = int.Parse(gametimeStr);
			}
			else if(_split1name.Equals( entry.Key ))
			{
				string split1nameStr = (string) entry.Value;
				Split1Name = split1nameStr;
			}
			
			analyticResult.Add (entry.Key, (string)entry.Value);
		}
		Debug.Log ("friction = " + GearFriction + ", geartype = " + GearShapeType + ", gametime = " + GameTime);
		
		//store values
		if (PlayerPrefs.HasKey ("gearfriction")) {
			PlayerPrefs.SetFloat("gearfriction", GearFriction);
		}
		if (PlayerPrefs.HasKey ("geartype")) {
			PlayerPrefs.SetInt("geartype", GearShapeType);
		}
		if (PlayerPrefs.HasKey ("gametime")) {
			PlayerPrefs.SetInt("gametime", GameTime);
		}
		if (PlayerPrefs.HasKey ("splitgroup")) {
			PlayerPrefs.SetString("splitgroup", Split1Name);
		}
		
		NotificationCenter.DefaultCenter.PostNotification(getMainMenuClass(), "RefreshDebugText");

	}
	
}




