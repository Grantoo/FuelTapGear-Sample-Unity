//#define USE_ANALYTICS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using SimpleJSON;

#if USE_ANALYTICS
using Analytics;
#endif


public class FuelDynamicsHandler : MonoBehaviour 
{
#if USE_ANALYTICS
	[Header("Flurry Settings")]
	[SerializeField] private string _iosApiKey = "RMD3XX7P4FY999T2JR9X";
	[SerializeField] private string _androidApiKey = "JC6T8PMDXXD7524RRN8K";
	IAnalytics flurryService;
#endif

	public static FuelDynamicsHandler Instance { get; private set; }

	private fuelSDKListener m_listener;
	
	public string dynamicConditions;

	private float gearFriction;
	public float getGearFriction()
	{	
		return gearFriction;	
	}

	private int gearShapeType;
	public int getGearShapeType() 
	{	
		return gearShapeType;
	}
	
	/*
	 * Awake
	*/
	void Awake ()
	{
		if (Instance != null && Instance != this) 
		{
			//destroy other instances
			Destroy (gameObject);
		} 
		else if( Instance == null )
		{
#if USE_ANALYTICS
			flurryService = Flurry.Instance;
			
			//AssertNotNull(service, "Unable to create Flurry instance!", this);
			//Assert(!string.IsNullOrEmpty(_iosApiKey), "_iosApiKey is empty!", this);
			//Assert(!string.IsNullOrEmpty(_androidApiKey), "_androidApiKey is empty!", this);
			flurryService.StartSession(_iosApiKey, _androidApiKey);
#endif
			m_listener = new fuelSDKListener ();
			if(m_listener == null) 
			{
				throw new Exception();
			}


		}
		
		Instance = this;
		
		DontDestroyOnLoad(gameObject);

	}
	
	
	
	/*
	 * Start
	*/
	void Start () 
	{
		Debug.Log ("<----- Start ----->");

		gearFriction = 0.98f;
		gearShapeType = 0;

		if (PlayerPrefs.HasKey ("numLaunches")) 
		{
			int numLaunches = PlayerPrefs.GetInt ("numLaunches");
			numLaunches++;
			PlayerPrefs.SetInt("numLaunches", numLaunches);
		}

		setUserConditions ();

		Debug.Log ("<----- Start Done ----->");

	}
	
	
	/*
	 * Update
	*/
	void Update () 
	{

	}

	public void tryRefreshHiScore()
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();
		
		if(_mainmenuScript)
		{
			//_mainmenuScript.RefreshHiScore(m_matchData.MatchScore);
		}
	}
	
	void OnApplicationPause(bool paused)
	{
		// application entering background

		if (paused) 
		{
			#if UNITY_IPHONE

			//NotificationServices.ClearLocalNotifications ();
			//NotificationServices.ClearRemoteNotifications ();

			#endif
		}

	}

	private void OnHideUnity(bool isGameShown)                                                   
	{                                                                                            
		Debug.Log("Facebook OnHideUnity");    
		                                                        
		if (!isGameShown)                                                                        
		{                                                                                        
			// pause the game - we will need to hide                                             
			Time.timeScale = 0;                                                                  
		}                                                                                        
		else                                                                                     
		{                                                                                        
			// start the game back up - we're getting focus again                                
			Time.timeScale = 1;                                                                  
		}
		                                                                                       
	}    
	
	
	
	





	/*
	 ---------------------------------------------------------------------
	 ---------------------------------------------------------------------
								Fuel Dynamics
	 ---------------------------------------------------------------------
	 ---------------------------------------------------------------------
    */
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
		
		int userAge = getUserAge ();
		int numLaunches = getNumLaunches ();
		int numSessions = getNumSessions ();
		
		
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
		conditions.Add ("gpsLong", "0.0000");
		conditions.Add ("gpsLat", "0.00000");
		
		//game conditions
		conditions.Add ("gameVersion", "tapgear v1.1");
		
		FuelDynamics.SetUserConditions (conditions);	
	}
	
	public void getUserValues()
	{
		FuelDynamics.GetUserValues();
	}
	
	public void OnFuelDynamicsUserValues (Dictionary<string, object> userValuesInfo)
	{
		Debug.Log ("-----------OnFuelDynamicsUserValues----------------");
		
		//Game Values - defined int the CSV
		String _friction = "friction";
		String _greartype = "geartype";
		String _status = "status";
		String statusresult = "notset";

		Dictionary<string, string> analyticResult = new Dictionary<string, string> ();

		foreach(KeyValuePair<string, object> entry in userValuesInfo)
		{
			Debug.Log ("....Key = " + entry.Key + " : Value = " + entry.Value);
			
			if(_friction.Equals( entry.Key ))
			{
				string friction = (string) entry.Value;
				gearFriction = float.Parse(friction);
				
			}
			else if(_greartype.Equals( entry.Key ))
			{
				string geartype = (string) entry.Value;
				gearShapeType = int.Parse(geartype);
			}
			else if(_status.Equals( entry.Key ))
			{
				statusresult = (string) entry.Value;
			}

			analyticResult.Add (entry.Key, (string)entry.Value);
		}
		
		Debug.Log ("__Final: friction = " + gearFriction + ", geartype = " + gearShapeType + ", statusresult = " + statusresult);

#if USE_ANALYTICS
		flurryService.LogEvent("OnFuelDynamicsUserValues", analyticResult);
#endif

		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();
		if (_mainmenuScript == null) 
		{
			throw new Exception();
		}
	}

	
}




