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
	private bool matchDataReady;
	private bool matchComplete;
	private string tournamentID;
	private string matchID;
	private int matchType;
	private int matchRound;
	private int matchScore;
	private int matchMaxSpeed;
	private string yourNickname;
	private string yourAvatarURL;
	private string theirNickname;
	private string theirAvatarURL;
	
	public bool MatchDataReady 
	{	
		get { return matchDataReady; }	
		set { matchDataReady = value; }
	}
	public bool MatchComplete 
	{	
		get { return matchComplete; }	
		set { matchComplete = value; }
	}
	public string TournamentID 
	{	
		get { return tournamentID; }	
		set { tournamentID = value; }
	}
	public string MatchID 
	{	
		get { return matchID; }	
		set { matchID = value; }
	}
	public int MatchType 
	{	
		get { return matchType; }	
		set { matchType = value; }
	}
	public int MatchRound 
	{	
		get { return matchRound; }	
		set { matchRound = value; }
	}
	public int MatchScore 
	{	
		get { return matchScore; }	
		set { matchScore = value; }
	}
	public int MatchMaxSpeed 
	{	
		get { return matchMaxSpeed; }	
		set { matchMaxSpeed = value; }
	}
	public string YourNickname 
	{	
		get { return yourNickname; }	
		set { yourNickname = value; }
	}
	public string YourAvatarURL 
	{	
		get { return yourAvatarURL; }	
		set { yourAvatarURL = value; }
	}
	public string TheirNickname 
	{	
		get { return theirNickname; }	
		set { theirNickname = value; }
	}
	public string TheirAvatarURL 
	{	
		get { return theirAvatarURL; }	
		set { theirAvatarURL = value; }
	}	
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

	public enum eFBState 
	{
		WaitForInit, 
		DataRetrived,
		LaunchWithResults,
		SystemSettled
	};
	public eFBState mFBState = eFBState.WaitForInit;

	public bool useFaceBook; 
	public bool useFuelCompete; 
	public bool useFuelDynamics; 

	public string get_data; 
	public string fbname;
	public string fbfirstname;//use this as nickname for now
	public string fbemail;
	public string fbgender;
	private bool fbdata_ready; 


	public const int MATCH_TYPE_SINGLE = 0;
	public const int MATCH_TYPE_MULTI = 1;


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


	public GameMatchData getMatchData()
	{
		return m_matchData;
	}


	public void tryLaunchFuelSDK()
	{
		if (m_matchData.MatchComplete == true && m_matchData.MatchType == MATCH_TYPE_MULTI) 
		{
			LaunchDashBoardWithResults();
		}
	}


	public void launchSinglePlayerGame()
	{
		m_matchData.MatchType = MATCH_TYPE_SINGLE;
		m_matchData.MatchDataReady = false;

		Application.LoadLevel("GamePlay");
	}

	public void LaunchMultiplayerGame(Dictionary<string, string> matchResult)
	{
		m_matchData.MatchType = MATCH_TYPE_MULTI;

		m_matchData.MatchDataReady = true;

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
		           "MatchDataReady = " + m_matchData.MatchDataReady + "\n" +
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

		Application.LoadLevel("GamePlay");
	}
	
	private void sendMatchResult (long score)
	{
		Debug.Log ("sendMatchResult");

		long visualScore = score;
		
		Dictionary<string, object> matchResult = new Dictionary<string, object> ();
		matchResult.Add ("tournamentID", m_matchData.TournamentID);
		matchResult.Add ("matchID", m_matchData.MatchID);
		matchResult.Add ("score", m_matchData.MatchScore);
		string visualScoreStr = visualScore.ToString() + " taps : " + m_matchData.MatchMaxSpeed.ToString() + " mps";
		matchResult.Add ("visualScore", visualScoreStr);

		PropellerSDK.SubmitMatchResult (matchResult);
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

		PropellerSDK.Launch (m_listener);	
	}

	/*
	 * Functions called from other scripts
	*/
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
		
		PropellerSDK.Launch (m_listener);
	}


	public void updateLoginText (string str) 
	{
		GameObject gameObj = GameObject.Find ("LoginStatusText");
		if (gameObj) 
		{
			TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			tmesh.text = str;
		}
	}
	
	
	public void StuffScore(int scoreValue, int speedValue)
	{
		Debug.Log ("StuffScore = " + scoreValue);

		m_matchData.MatchScore = scoreValue;
		m_matchData.MatchMaxSpeed = speedValue;

		//hmmm, better clean up this check
		if (m_matchData.MatchDataReady == true) 
		{
			m_matchData.MatchComplete = true;
		}
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

			m_matchData = new GameMatchData ();
			m_matchData.MatchDataReady = false;
			m_matchData.MatchComplete = false;

			fbdata_ready = false;

			useFaceBook = false;
			useFuelCompete = false; 
			useFuelDynamics = false; 


			if(useFaceBook)
			{
				// Initialize FB SDK 
				FB.Init(SetInit, OnHideUnity);  
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

		gearFriction = 0.98f;
		gearShapeType = 0;

		if (PlayerPrefs.HasKey ("numLaunches")) 
		{
			int numLaunches = PlayerPrefs.GetInt ("numLaunches");
			numLaunches++;
			PlayerPrefs.SetInt("numLaunches", numLaunches);
		}

		if (useFuelDynamics == true) 
		{
			setUserConditions ();
		}

		Debug.Log ("<----- Start Done ----->");

	}
	
	public void LaunchDashBoardWithResults()
	{
		sendMatchResult (m_matchData.MatchScore);
		//sendCustomMatchResult (m_matchData.MatchScore, 44);
	}

	
	/*
	 * Update
	*/
	void Update () 
	{

		//change this to a message
		switch (mFBState) 
		{
			case eFBState.WaitForInit:
					if (fbdata_ready) //deprecated? probably
					{
						//PushFBDataToFuel();	
						mFBState = eFBState.DataRetrived;
					}
			break;
			
			case eFBState.DataRetrived:
				break;


			case eFBState.LaunchWithResults:
				break;

			case eFBState.SystemSettled:

				break;

			
		}
		//-------------------------

	}


	
	/*
	 -----------------------------------------------------
						Challenge Counts
	 -----------------------------------------------------
	*/
	public void SyncChallengeCounts ()
	{
		if (useFuelCompete == false) 
		{
			return;
		}

		PropellerSDK.SyncChallengeCounts ();
	}
	
	public void OnPropellerSDKChallengeCountUpdated (string count)
	{
		int countInt;

		Debug.Log ("OnPropellerSDKChallengeCountUpdated : count = " + count);

		if (!int.TryParse(count, out countInt))
		{
			return;
		}

		Debug.Log ("OnPropellerSDKChallengeCountUpdated : countInt = " + countInt);

		// Update the UI with the count
		tryRefreshChallengeCount (countInt);
	}
	
	public void tryRefreshChallengeCount(int ccount)
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();

		if(_mainmenuScript)
		{
			_mainmenuScript.RefreshChallengeCount(ccount);
		}
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

		bool tournyRunning = false;

		if ((tournamentInfo == null) || (tournamentInfo.Count == 0)) 
		{
			// there is no tournament currently running
			Debug.Log ("....no tournaments currently running");
		} 
		else 
		{
			string name = tournamentInfo["name"];
			string campaignName = tournamentInfo["campaignName"];
			string sponsorName = tournamentInfo["sponsorName"];
			string startDate = tournamentInfo["startDate"];
			string endDate = tournamentInfo["endDate"];
			string logo = tournamentInfo["logo"];

			Debug.Log ("______________________" + "\n" +
			           "*** TournamentInfo ***" + "\n" +
			           "name = " + name + "\n" +
			           "campaignName = " + campaignName + "\n" +
			           "sponsorName = " + sponsorName + "\n" +
			           "startDate = " + startDate + "\n" +
			           "endDate = " + endDate + "\n" +
			           "logo = " + logo + "\n"
					  );


			tournyRunning = true;
		}
		
		// Update the UI with the tournament information

		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();

		_mainmenuScript.RefreshTournamentInfo(tournyRunning, "noname", 0);
	}

	public void tryRefreshTournamentInfo(Dictionary<string, string> tournamentInfo)
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();
		
		if(_mainmenuScript)
		{
		}
	}

	public void tryRefreshHiScore()
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();
		
		if(_mainmenuScript)
		{
			_mainmenuScript.RefreshHiScore(m_matchData.MatchScore);
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


		// Store the virtual goods for consumption

		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu>();
		if (_mainmenuScript == null) 
		{
			throw new Exception();
		}

		bool virtualGoodsTaken = false;
		foreach (string vg in virtualGoods)
		{
			Debug.Log (">>>>>> vg = " + vg);

			if(vg == "gameToken")//Game Token
			{
				_mainmenuScript.RefreshGameTokenCount(8);
				virtualGoodsTaken = true;
			}
			else if(vg == "goldPack")//Bunch of gold
			{
				_mainmenuScript.RefreshGoldCount(4);
				virtualGoodsTaken = true;
			}
			else if(vg == "diamondGrade1")//Single Diamond
			{
				_mainmenuScript.RefreshDiamondCount(1);
				virtualGoodsTaken = true;
			}
		}

		//tell main menu to setup some fan fair
		if(virtualGoodsTaken == true)
		{
			_mainmenuScript.VirtualGoodsFanFare();
		}

		// Acknowledge the receipt of the virtual goods list
		PropellerSDK.AcknowledgeVirtualGoods(transactionId, true);
	}

	public void OnPropellerSDKVirtualGoodRollback (string transactionId)
	{
		Debug.Log ("OnPropellerSDKVirtualGoodRollback");

		// Rollback the virtual good transaction for the given transaction ID
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
	 ---------------------------------------------------------------------
	 ---------------------------------------------------------------------
							Face Book Unity Plugin
	 ---------------------------------------------------------------------
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
			FB.Login("email, publish_actions", LoginCallback); 
			//FB.Login ("public_profile, user_friends, email, publish_actions", LoginCallback); 
		}
		else
		{
			updateLoginText("Already Logged In");
		}
		
	}
	public void LogoutButtonPressed()
	{
		if (useFaceBook == false) 
		{
			return;
		}

		if (FB.IsLoggedIn) 
		{      
			Debug.Log("LogoutButtonPressed: LOGGING OUT!");                                                          

			FB.Logout();
		}
	}

	void LoginCallback(FBResult result)                                                        
	{                                                                                          
		Debug.Log("___Login_Callback___");                                                          

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
		//get additional data from facebook

		FB.API("me?fields=name,email,gender,first_name", Facebook.HttpMethod.GET, UserCallBack);

	}  
	
	
	
	void UserCallBack(FBResult result) 
	{

		if (result.Error != null)
		{
			get_data = result.Text;
		}
		else
		{
			get_data = result.Text;
		}


		var dict = Json.Deserialize(get_data) as IDictionary;
		fbname = dict ["name"].ToString();
		fbemail = dict ["email"].ToString();
		fbgender = dict ["gender"].ToString();
		fbfirstname = dict ["first_name"].ToString();


		//fbdata_ready = true;


		PushFBDataToFuel ();

	}
	
	public void trySocialLogin(bool allowCache)                                                                       
	{
		if (FB.IsLoggedIn && allowCache == false) 
		{    
			Debug.Log("trySocialLogin::::Logout - Login");                                                          

			FB.Logout();
			FB.Login("email, publish_actions", LoginCallback); 

			//return to sdk
			//PropellerSDK.SdkSocialLoginCompleted (null);

		}
		else if (FB.IsLoggedIn) 
		{    
			Debug.Log("trySocialLogin::::PushFBDataToFuel");                                                          
			PushFBDataToFuel();//is this needed
		}
		else 
		{
			Debug.Log("trySocialLogin::::Login");                                                          
			FB.Login("email, publish_actions", LoginCallback); 
		}

	}

	
	public void PushFBDataToFuel()                                                                       
	{
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

		//string localTime = 


		Debug.Log ("__PushFBDataToFuel__" + "\n" +
				"*** loginInfo ***" + "\n" +
				"provider = " + loginInfo ["provider"].ToString () + "\n" +
				"email = " + loginInfo ["email"].ToString () + "\n" +
				"id = " + loginInfo ["id"].ToString () + "\n" +
				"token = " + loginInfo ["token"].ToString () + "\n" +
				"nickname = " + loginInfo ["nickname"].ToString () + "\n" +
				"name = " + loginInfo ["name"].ToString () + "\n" +
				"gender = " + loginInfo ["gender"].ToString () + "\n" +
				"expireDate = " + expireDate.ToLongDateString ());

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
			Debug.Log("....Already logged in");
			
			//set onscreen button text
			updateLoginText ("Logged In");
			
			OnLoggedIn();
			
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
		if (useFuelDynamics == false) 
		{
			return;
		}
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
		
		PropellerSDK.SetUserConditions (conditions);	
	}
	
	public void getUserValues()
	{
		if (useFuelDynamics == false) 
		{
			return;
		}

		PropellerSDK.GetUserValues();
	}
	
	public void OnFuelDynamicsUserValues (Dictionary<string, object> userValuesInfo)
	{
		if (useFuelDynamics == false) 
		{
			return;	
		}

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




