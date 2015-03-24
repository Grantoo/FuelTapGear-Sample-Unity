//#define USE_ANALYTICS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Facebook.MiniJSON;
using SimpleJSON;

#if USE_ANALYTICS
using Analytics;
#endif


public struct GameMatchData 
{
	public bool ValidMatchData { get; set; }
	public bool MatchComplete { get; set; }
	public int MatchType { get; set; }
	public int MatchRound { get; set; }
	public int MatchScore { get; set; }
	public int MatchMaxSpeed { get; set; }
	public string TournamentID { get; set; }
	public string MatchID { get; set; }
	public string YourNickname { get; set; }
	public string YourAvatarURL { get; set; }
	public string TheirNickname { get; set; }
	public string TheirAvatarURL { get; set; }
}



public class FuelHandler : MonoBehaviour 
{
#if USE_ANALYTICS
	[Header("Flurry Settings")]
	[SerializeField] private string _iosApiKey = "RMD3XX7P4FY999T2JR9X";
	[SerializeField] private string _androidApiKey = "JC6T8PMDXXD7524RRN8K";
	IAnalytics flurryService;
#endif

	public static FuelHandler Instance { get; private set; }

	private fuelSDKListener m_listener;
	private GameMatchData m_matchData;
	
	private bool useFaceBook; 
	private bool useFuelCompete; 

	public string fbname;
	public string fbfirstname;//use this as nickname for now
	public string fbemail;
	public string fbgender;

	public const int MATCH_TYPE_SINGLE = 0;
	public const int MATCH_TYPE_MULTI = 1;
	
	public float GearFriction { get; set; }
	public int GearShapeType { get; set; }
	public int GameTime { get; set; }

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
			Input.location.Start();

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
			
			m_matchData = new GameMatchData ();
			m_matchData.ValidMatchData = false;
			m_matchData.MatchComplete = false;
			
			useFaceBook = true;
			useFuelCompete = true; 

			if(useFaceBook)
			{
				Debug.Log ("FB.Init");
				FB.Init(SetInit, OnHideUnity);  
			}
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
		
		// enable push notifications
		PropellerSDK.EnableNotification ( PropellerSDK.NotificationType.push );
		
		// disable local notifications
		//PropellerSDK.DisableNotification ( PropellerSDK.NotificationType.local );
		
		// validate enabled notifications - result is false since local notifications are disabled
		bool fuelEnabled = PropellerSDK.IsNotificationEnabled( PropellerSDK.NotificationType.all );	
		if (fuelEnabled) 
		{
			Debug.Log ("fuelEnabled NotificationEnabled!");
		}

		GearFriction = 0.98f;
		GearShapeType = 5;
		GameTime = 7;

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


		if (PlayerPrefs.HasKey ("numLaunches")) 
		{
			int numLaunches = PlayerPrefs.GetInt ("numLaunches");
			numLaunches++;
			PlayerPrefs.SetInt("numLaunches", numLaunches);
		}
		
		setUserConditions ();

	}
	

	void OnApplicationPause(bool paused)
	{
		// application entering background
		if (paused) 
		{
			#if UNITY_IPHONE
			NotificationServices.ClearLocalNotifications ();
			NotificationServices.ClearRemoteNotifications ();
			#endif
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


	/*
	 -----------------------------------------------------
						Launch Routines
	 -----------------------------------------------------
	*/
	private void LaunchDashBoardWithResults()
	{
		Debug.Log ("LaunchDashBoardWithResults");
		sendMatchResult (m_matchData.MatchScore);
	}

	
	public void tryLaunchFuelSDK()
	{
		Debug.Log ("tryLaunchFuelSDK");
		if (m_matchData.MatchComplete == true && m_matchData.MatchType == MATCH_TYPE_MULTI) 
		{
			LaunchDashBoardWithResults();
		}
	}


	public void launchSinglePlayerGame()
	{
		m_matchData.MatchType = MATCH_TYPE_SINGLE;
		m_matchData.ValidMatchData = false;

		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "LaunchGamePlay");
	}

	
	public void LaunchMultiplayerGame(Dictionary<string, string> matchResult)
	{
		m_matchData.MatchType = MATCH_TYPE_MULTI;
		m_matchData.ValidMatchData = true;

		m_matchData.TournamentID = matchResult ["tournamentID"];
		m_matchData.MatchID = matchResult ["matchID"];

		// extract the params data
		string paramsJSON = matchResult ["paramsJSON"];
		JSONNode json = JSONNode.Parse (paramsJSON);

		m_matchData.MatchRound = json ["round"].AsInt;

		JSONClass you = json ["you"].AsObject;
		m_matchData.YourNickname = you ["name"];
		m_matchData.YourAvatarURL = you ["avatar"];

		JSONClass them = json ["them"].AsObject;
		m_matchData.TheirNickname = them ["name"];
		m_matchData.TheirAvatarURL = them ["avatar"];

		Debug.Log (	"__LaunchMultiplayerGame__" + "\n" +
		           "ValidMatchData = " + m_matchData.ValidMatchData + "\n" +
		           "TournamentID = " + m_matchData.TournamentID + "\n" +
		           "MatchID = " + m_matchData.MatchID + "\n" +
		           "MatchRound = " + m_matchData.MatchRound + "\n" +
		           "adsAllowed = " + "\n" +
		           "fairPlay = " + "\n" +
		           "YourNickname = " + m_matchData.YourNickname + "\n" +
		           "YourAvatarURL = " + m_matchData.YourAvatarURL + "\n" +
		           "TheirNickname = " + m_matchData.TheirNickname + "\n" +
		           "TheirAvatarURL = " + m_matchData.TheirAvatarURL + "\n" 
		           );


		m_matchData.MatchComplete = false;

		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "LaunchGamePlay");
	}


	private void sendMatchResult (long score)
	{
		Debug.Log ("sendMatchResult");

		long visualScore = score;
		
		Dictionary<string, object> matchResult = new Dictionary<string, object> ();
		matchResult.Add ("tournamentID", m_matchData.TournamentID);
		matchResult.Add ("matchID", m_matchData.MatchID);
		matchResult.Add ("score", m_matchData.MatchScore);
		string visualScoreStr = visualScore.ToString() + " : " + m_matchData.MatchMaxSpeed.ToString() + " mps";
		matchResult.Add ("visualScore", visualScoreStr);

		PropellerSDK.SubmitMatchResult (matchResult);
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		PropellerSDK.Launch (m_listener);	
	}

	
	private void sendCustomMatchResult (long score, float visualScore)
	{
		Debug.Log ("sendCustomMatchResult");

		// construct custom leader board score currency dictionary for auxScore1
		Dictionary<string, object> auxScore1Currency = new Dictionary<string, object> ();
		auxScore1Currency.Add ("id", "visualScore");
		auxScore1Currency.Add ("score", visualScore);
		auxScore1Currency.Add ("visualScore", visualScore.ToString ());

		// construct array of custom leader board score currencies
		List<object> currencies = new List<object> ();
		currencies.Add (auxScore1Currency);

		// construct match data dictionary
		Dictionary<string, object> matchData = new Dictionary<string, object> ();
		matchData.Add ("currencies", currencies);

		Dictionary<string, object> matchResult = new Dictionary<string, object> ();
		matchResult.Add ("tournamentID", m_matchData.TournamentID);
		matchResult.Add ("matchID", m_matchData.MatchID);
		matchResult.Add ("score", m_matchData.MatchScore);
		matchResult.Add ("matchData", matchData);

		PropellerSDK.SubmitMatchResult (matchResult);
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		PropellerSDK.Launch (m_listener);	
	}

	public void launchPropeller ()
	{
		if (useFuelCompete == false) 
		{
			return;
		}

		Debug.Log ("launchPropeller");
		if (m_listener == null) 
		{
			throw new Exception();
		}
		
		
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		
		PropellerSDK.Launch (m_listener);
	}


	public void updateLoginText () 
	{
		GameObject gameObj = GameObject.Find ("LoginStatusText");

		if (FB.IsLoggedIn) 
		{
			if (gameObj) 
			{
				TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
				tmesh.text = "LogOut";
			}
		} 
		else 
		{
			if (gameObj) 
			{
				TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
				tmesh.text = "Log In";
			}
		}
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
						Challenge Counts
	 -----------------------------------------------------
	*/
	public void SyncChallengeCounts ()
	{
		if (useFuelCompete == false) {
			return;
		}

		PropellerSDK.SyncChallengeCounts ();
	}
	
	public void OnPropellerSDKChallengeCountUpdated (string count)
	{
		int countValue;
		if (!int.TryParse(count, out countValue)) {
			return;
		}

		Debug.Log ("OnPropellerSDKChallengeCountUpdated : countValue = " + countValue);

		Hashtable ccTable = new Hashtable();                 
		ccTable["cc"] = countValue;                          
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshChallengeCount", ccTable );
	}


	/*
	 -----------------------------------------------------
						Tournament Info
	 -----------------------------------------------------
	*/
	public void SyncTournamentInfo ()
	{
		if (useFuelCompete == false) 
		{
			return;
		}

		PropellerSDK.SyncTournamentInfo ();
	}
	public void OnPropellerSDKTournamentInfo (Dictionary<string, string> tournamentInfo)
	{
		Debug.Log ("OnPropellerSDKTournamentInfo");

		if ((tournamentInfo == null) || (tournamentInfo.Count == 0)) 
		{
			Debug.Log ("....no tournaments currently running");
		} 
		else 
		{
			string tournyname = tournamentInfo["name"];
			string campaignName = tournamentInfo["campaignName"];
			string sponsorName = tournamentInfo["sponsorName"];
			string startDate = tournamentInfo["startDate"];
			string endDate = tournamentInfo["endDate"];
			string logo = tournamentInfo["logo"];

			Debug.Log 
			(
			    "*** TournamentInfo ***" + "\n" +
			    "tournyname = " + tournyname + "\n" +
			    "campaignName = " + campaignName + "\n" +
			    "sponsorName = " + sponsorName + "\n" +
			    "startDate = " + startDate + "\n" +
			    "endDate = " + endDate + "\n" +
			    "logo = " + logo + "\n"
			);

			Hashtable tournyTable = new Hashtable();                 
			tournyTable["running"] = false;                          
			tournyTable["tournyname"] = tournyname;                          
			tournyTable["startDate"] = startDate;                          
			tournyTable["endDate"] = endDate;                          
			NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshTournamentInfo", tournyTable );
		}
	}


	/*
	 -----------------------------------------------------
						Virtual Goods
	 -----------------------------------------------------
	*/
	public void SyncVirtualGoods ()
	{
		if (useFuelCompete == false) 
		{
			return;
		}

		PropellerSDK.SyncVirtualGoods ();
	}

	public void OnPropellerSDKVirtualGoodList (Dictionary<string, object> virtualGoodInfo)
	{
		string transactionId = (string) virtualGoodInfo["transactionID"];
		List<string> virtualGoods = (List<string>) virtualGoodInfo["virtualGoods"];

		Debug.Log ("OnPropellerSDKVirtualGoodList: transactionId = " + transactionId);


		Hashtable goodsTable = new Hashtable();                 
		bool virtualGoodsTaken = false;
		foreach (string vg in virtualGoods)
		{
			Debug.Log (">> vg = " + vg);

			if(vg == "goldPack")
			{
				goodsTable["addGold"] = 4;                          
				virtualGoodsTaken = true;
			}
			else if(vg == "diamondGrade1")
			{
				goodsTable["addOil"] = 2;                          
				virtualGoodsTaken = true;
			}
		}

		if(virtualGoodsTaken == true) {
			NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshVirtualGoods", goodsTable);
		}

		// Acknowledge the receipt of the virtual goods list
		PropellerSDK.AcknowledgeVirtualGoods(transactionId, true);
	}

	public void OnPropellerSDKVirtualGoodRollback (string transactionId)
	{
		Debug.Log ("OnPropellerSDKVirtualGoodRollback");

		// Rollback the virtual good transaction for the given transaction ID
	}








	

	
	
	
	
	
	
	/*
	 ---------------------------------------------------------------------
							Face Book Unity Plugin
	 ---------------------------------------------------------------------
	*/
	public void LoginButtonPressed()
	{
		if (useFaceBook == false) 
		{
			return;
		}

		if (!FB.IsLoggedIn) 
		{   
			Debug.Log("FB.Login(email, publish_actions, LoginCallback);");                                                          

			FB.Login("email, publish_actions", LoginCallback); 
			//FB.Login ("public_profile, user_friends, email, publish_actions", LoginCallback); 
		}
		else
		{
			//Logout?
			//FB.Logout();
		}
	}
	public void LogoutButtonPressed()
	{
		/*
		if (useFaceBook == false) 
		{
			return;
		}

		if (FB.IsLoggedIn) 
		{      
			Debug.Log("LogoutButtonPressed: LOGGING OUT!");                                                          

			FB.Logout();
		}
		*/
	}

	void LoginCallback(FBResult result)                                                        
	{                                                                                          
		Debug.Log("LoginCallback");                                                          

		if (FB.IsLoggedIn) 
		{                                                                                      
			OnLoggedIn ();                                                                      
		} 
		else 
		{
			Debug.Log("....WARNING NOT LOGGING IN");                                                          
		}
	}                                                                                          
	
	void OnLoggedIn()                                                                          
	{        
		Debug.Log("OnLoggedIn");                                                          
		FB.API("me?fields=name,email,gender,first_name", Facebook.HttpMethod.GET, UserCallBack);
	}  
	
	
	
	void UserCallBack(FBResult result) 
	{
		string get_data;
		if (result.Error != null) {
			get_data = result.Text;
		} else {
			get_data = result.Text;
		}

		var dict = Json.Deserialize(get_data) as IDictionary;
		fbname = dict ["name"].ToString();
		fbemail = dict ["email"].ToString();
		fbgender = dict ["gender"].ToString();
		fbfirstname = dict ["first_name"].ToString();

		PushFBDataToFuel ();
	}
	
	public void trySocialLogin(bool allowCache)                                                                       
	{
		Debug.Log("trySocialLogin");                                                          
		if (FB.IsLoggedIn && allowCache == false) 
		{    
			//FB.Logout();
			//FB.Login("email, publish_actions", LoginCallback); 

			//return to sdk
			//PropellerSDK.SdkSocialLoginCompleted (null);
		}
		else if (FB.IsLoggedIn) 
		{    
			PushFBDataToFuel();//is this needed?
		}
		else 
		{
			FB.Login("email, publish_actions", LoginCallback); 
		}

	}

	
	public void PushFBDataToFuel()                                                                       
	{
		Debug.Log("PushFBDataToFuel"); 

		string provider = "facebook";
		string email = fbemail;
		string id = FB.UserId;
		string token = FB.AccessToken;
		DateTime expireDate = FB.AccessTokenExpiresAt;
		string nickname = fbfirstname;//not available from FB using first name
		string name = fbname;
		string gender = fbgender;

		Dictionary<string, string> loginInfo = null;
		loginInfo = new Dictionary<string, string> ();
		loginInfo.Add ("provider", provider);
		loginInfo.Add ("email", email);
		loginInfo.Add ("id", id);
		loginInfo.Add ("token", token);
		loginInfo.Add ("nickname", nickname);
		loginInfo.Add ("name", name);
		loginInfo.Add ("gender", gender);

		Debug.Log 
		(
			"*** loginInfo ***" + "\n" +
			"provider = " + loginInfo ["provider"].ToString () + "\n" +
			"email = " + loginInfo ["email"].ToString () + "\n" +
			"id = " + loginInfo ["id"].ToString () + "\n" +
			"token = " + loginInfo ["token"].ToString () + "\n" +
			"nickname = " + loginInfo ["nickname"].ToString () + "\n" +
			"name = " + loginInfo ["name"].ToString () + "\n" +
			"gender = " + loginInfo ["gender"].ToString () + "\n" +
			"expireDate = " + expireDate.ToLongDateString ()
		);

		PropellerSDK.SdkSocialLoginCompleted (loginInfo);
	}
	
	
	
	/*
	 -----------------------------------------------------
	 				FaceBook Init - private
	 -----------------------------------------------------
	*/
	private void SetInit()                                                                       
	{                                                                                            
		Debug.Log("Facebook SetInit"); 
		    
		if (FB.IsLoggedIn) 
		{                                                                                        
			Debug.Log ("....Already logged in");

			OnLoggedIn ();

		} else {

			Debug.Log ("....Already logged in");

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
	 -----------------------------------------------------
	 				FaceBook Invite
	 -----------------------------------------------------
	*/

	public void onSocialInviteClicked(Dictionary<string, string> inviteInfo)                                                                                              
	{ 
		Debug.Log("onSocialInviteClicked");  
		/*
			string message,
			string[] to = null,
			List<object> filters = null,
			string[] excludeIds = null,
			int? maxRecipients = null,
			string data = "",
			string title = "",
			FacebookDelegate callback = null)
		*/


		if (FB.IsLoggedIn) 
		{
			FB.AppRequest ("Come On!", 
			null, 
			null, 
			null, 
			null, 
			"Some Data", 
			"Some Title", 
			appRequestCallback);
		} 
		else 
		{
			FB.Login("email, publish_actions", LoginCallback); 
		}
		                                                                                                            
		
	}                                                                                                                              
	private void appRequestCallback (FBResult result)                                                                              
	{     

		Debug.Log("appRequestCallback");  
		                                                                                       
		if (result != null)                                                                                                        
		{    
			
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;                                      
			object obj = 0;                                                                                                        
			if (responseObject.TryGetValue ("cancelled", out obj))                                                                 
			{                                                                                                                      
				Debug.Log("Request cancelled");                                                                                  
			}                                                                                                                      
			else if (responseObject.TryGetValue ("request", out obj))                                                              
			{                
				//AddPopupMessage("Request Sent", ChallengeDisplayTime);
				Debug.Log("Request sent");                                                                                       
			} 

			PropellerSDK.SdkSocialInviteCompleted();

			
		}  

	}  



	/*
	 -----------------------------------------------------
	 				FaceBook Share
	 -----------------------------------------------------
	*/
	
	public void onSocialShareClicked(Dictionary<string, string> inviteInfo)                                                                                              
	{ 
		Debug.Log("onSocialShareClicked");  
		/*
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null,
            FacebookDelegate callback = null)
		*/

		if (FB.IsLoggedIn) 
		{
			FB.Feed (FB.UserId, 
	         "", 
	         "", 
	         "", 
	         "", 
	         "", 
	         "", 
	         "", 
	         "", 
	         "", 
	         null,
	         appFeedCallback);
		} 
		else 
		{
			FB.Login("email, publish_actions", LoginCallback); 
		}
		
		
	}                                                                                                                              
	private void appFeedCallback (FBResult result)                                                                              
	{     
		Debug.Log("appFeedCallback");  
		
		if (result != null)                                                                                                        
		{    
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;                                      
			object obj = 0;                                                                                                        
			if (responseObject.TryGetValue ("cancelled", out obj))                                                                 
			{                                                                                                                      
				Debug.Log("Request cancelled");                                                                                  
			}                                                                                                                      
			else if (responseObject.TryGetValue ("request", out obj))                                                              
			{                
				//AddPopupMessage("Request Sent", ChallengeDisplayTime);
				Debug.Log("Request sent");                                                                                       
			} 
			
			PropellerSDK.SdkSocialShareCompleted();
		}  
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

		NotificationCenter.DefaultCenter.PostNotification(getMainMenuClass(), "RefreshDebugText");

#if USE_ANALYTICS
		flurryService.LogEvent("OnFuelDynamicsUserValues", analyticResult);
#endif

	}
		
}




