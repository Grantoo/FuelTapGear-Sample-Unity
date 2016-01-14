//#define USE_ANALYTICS
//#define RUN_UNIT_TESTS
//#define LOCATION_SERVICES

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

public enum SocialPost
{
	NONE,
	INVITE,
	SHARE
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

#if PROPELLER_SDK
	private fuelSDKListener m_listener;
#endif
	private GameMatchData m_matchData;
	
	private bool useFaceBook; 
	private bool useFuelCompete; 

	private SocialPost socialPost;
	private Dictionary<string, string> socialPostData;

	public string fbname;
	public string fbfirstname;//use this as nickname for now
	public string fbemail;
	public string fbgender;

	public const int MATCH_TYPE_SINGLE = 0;
	public const int MATCH_TYPE_MULTI = 1;
	
	public float GearFriction { get; set; }
	public int GearShapeType { get; set; }
	public int GameTime { get; set; }
	public int ShowDebug { get; set; }
	public int FBIcon { get; set; }
	public string Split1Name { get; set; }

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
			#if LOCATION_SERVICES
			Input.location.Start();
			#endif

			#if USE_ANALYTICS
			flurryService = Flurry.Instance;
				//AssertNotNull(service, "Unable to create Flurry instance!", this);
				//Assert(!string.IsNullOrEmpty(_iosApiKey), "_iosApiKey is empty!", this);
				//Assert(!string.IsNullOrEmpty(_androidApiKey), "_androidApiKey is empty!", this);
			flurryService.StartSession(_iosApiKey, _androidApiKey);
			#endif

#if PROPELLER_SDK
			m_listener = new fuelSDKListener ();
			if(m_listener == null) 
			{
				throw new Exception();
			}
#endif
			
			m_matchData = new GameMatchData ();
			m_matchData.ValidMatchData = false;
			m_matchData.MatchComplete = false;

			socialPost = SocialPost.NONE;
			socialPostData = null;

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

		SetLanguageLocale ();

#if PROPELLER_SDK
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
#endif

		GearFriction = 0.98f;
		GearShapeType = 5;
		GameTime = 7;
		ShowDebug = 0;
		FBIcon = 0;
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
		if (PlayerPrefs.HasKey ("showdebug")) {
			ShowDebug = PlayerPrefs.GetInt("showdebug");
		}
		if (PlayerPrefs.HasKey ("fbicon")) {
			FBIcon = PlayerPrefs.GetInt("fbicon");
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
	

	void OnApplicationPause(bool paused)
	{
		// application entering background
		if (paused) 
		{
			#if UNITY_IOS
			UnityEngine.iOS.NotificationServices.ClearLocalNotifications ();
			UnityEngine.iOS.NotificationServices.ClearRemoteNotifications ();
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
	private void LaunchDashBoard()
	{
		Debug.Log ("LaunchDashBoard");

#if PROPELLER_SDK
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		PropellerSDK.Launch (m_listener);
#endif
	}

	
	public bool tryLaunchFuelSDK()
	{
		Debug.Log ("tryLaunchFuelSDK");
		if (m_matchData.MatchComplete == true && m_matchData.MatchType == MATCH_TYPE_MULTI) 
		{
			m_matchData.MatchComplete = false;

			LaunchDashBoard();

			return true;
		}

		return false;
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

#if PROPELLER_SDK
		PropellerSDK.SubmitMatchResult (matchResult);
#endif
	}


	//not currently being used
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

#if PROPELLER_SDK
		PropellerSDK.SubmitMatchResult (matchResult);
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		PropellerSDK.Launch (m_listener);
#endif
	}

	public void launchPropeller ()
	{
		if (useFuelCompete == false) 
		{
			return;
		}

		Debug.Log ("launchPropeller");

#if PROPELLER_SDK		
		if (m_listener == null) 
		{
			throw new Exception();
		}
		
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "AddTransOverlay");
		PropellerSDK.Launch (m_listener);
#endif
	}


	public void updateLoginText () 
	{
		GameObject gameObj = GameObject.Find ("LoginStatusText");
		if (gameObj != null) 
		{

			if (FB.IsLoggedIn) {
					if (gameObj) {
							TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
							tmesh.text = "LogOut";
					}
			} else {
					if (gameObj) {
							TextMesh tmesh = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
							tmesh.text = "Log In";
					}
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

		//timer has run out. this is the earliest the score can be set
		//early score reporting
		sendMatchResult (m_matchData.MatchScore);

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

#if PROPELLER_SDK
		PropellerSDK.SyncChallengeCounts ();
#endif
	}
	
	public void OnPropellerSDKChallengeCountUpdated (string count)
	{
		int countValue;
		if (!int.TryParse(count, out countValue)) {
			return;
		}

		Debug.Log ("OnPropellerSDKChallengeCountUpdated : countValue = " + countValue);

		Hashtable ccTable = new Hashtable();                 
		ccTable.Add("cc", countValue);

		getMainMenuClass().RefreshChallengeCount(ccTable);

		//NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshChallengeCount", ccTable );
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

#if PROPELLER_SDK
		PropellerSDK.SyncTournamentInfo ();
#endif
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
			Debug.Log ("name");
			string tournyname = tournamentInfo["name"];
			Debug.Log ("campaignName");
			string campaignName = tournamentInfo["campaignName"];
			Debug.Log ("startDate");
			string startDate = tournamentInfo["startDate"];
			Debug.Log ("endDate");
			string endDate = tournamentInfo["endDate"];
			Debug.Log ("logo");
			string logo = tournamentInfo["logo"];


			Debug.Log 
			(
			    "*** TournamentInfo ***" + "\n" +
			    "tournyname = " + tournyname + "\n" +
			    "campaignName = " + campaignName + "\n" +
			    "startDate = " + startDate + "\n" +
			    "endDate = " + endDate + "\n" +
			    "logo = " + logo + "\n"
			);

			Hashtable tournyTable = new Hashtable();  

			tournyTable.Add("running", true);
			tournyTable.Add("tournyname", tournyname);
			tournyTable.Add("startDate", startDate);
			tournyTable.Add("endDate", endDate);

			getMainMenuClass().RefreshTournamentInfo(tournyTable);

			//notification not passing hashtable properly (runtime error)
			//NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshTournamentInfo", tournyTable );
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

#if RUN_UNIT_TESTS
		VirtualGoodUnitTest ();
#endif
#if PROPELLER_SDK
		PropellerSDK.SyncVirtualGoods ();
#endif
	}

	public void OnPropellerSDKVirtualGoodList (Dictionary<string, object> virtualGoodInfo)
	{
		string transactionId = (string) virtualGoodInfo["transactionID"];
		List<string> virtualGoods = (List<string>) virtualGoodInfo["virtualGoods"];

		Debug.Log ("OnPropellerSDKVirtualGoodList: transactionId = " + transactionId);

		Hashtable goodsTable = new Hashtable();  
		goodsTable["addGold"] = 0;                          
		goodsTable["addOil"] = 0;                          
		goodsTable["showTrophy"] = 0;                          

		bool virtualGoodsTaken = false;
		foreach (string vg in virtualGoods)
		{
			Debug.Log (":virtual good = " + vg);

			if(vg == "golddrop")
			{
				goodsTable["addGold"] = 2;                          
				virtualGoodsTaken = true;
			}
			else if(vg == "oildrop")
			{
				goodsTable["addOil"] = 2;                          
				virtualGoodsTaken = true;
			}
			else if(vg == "trophydrop")
			{
				goodsTable["showTrophy"] = 1;                          
				virtualGoodsTaken = true;
			}
		}

		if(virtualGoodsTaken == true) {
			getMainMenuClass().RefreshVirtualGoods(goodsTable);
			//NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "RefreshVirtualGoods", goodsTable);
		}

#if PROPELLER_SDK
		// Acknowledge the receipt of the virtual goods list
		PropellerSDK.AcknowledgeVirtualGoods(transactionId, true);
#endif
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

		trySocialLogin (false);
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

		if (!FB.IsLoggedIn) {
			if (result.Error != null) {
				Debug.Log ("LoginCallback - login request failed: " + result.Error);
			} else {
				Debug.Log ("LoginCallback - login request cancelled");
			}

#if PROPELLER_SDK
			PropellerSDK.SdkSocialLoginCompleted(null);
#endif
			
			if (socialPost != SocialPost.NONE) {
#if PROPELLER_SDK
				switch (socialPost) {
				case SocialPost.INVITE:
					PropellerSDK.SdkSocialInviteCompleted ();
					break;
				case SocialPost.SHARE:
					PropellerSDK.SdkSocialShareCompleted ();
					break;
				}
#endif
				
				socialPost = SocialPost.NONE;
				socialPostData = null;
			}
			
			return;
		}

		OnLoggedIn ();
	}                                                                                          

	void OnLoggedIn()                                                                          
	{        
		Debug.Log("OnLoggedIn");                                                          
		FB.API("me?fields=name,email,gender,first_name", Facebook.HttpMethod.GET, UserCallBack);
	}  
	
	
	
	void UserCallBack(FBResult result) 
	{
		Debug.Log("UserCallBack");
		
		if (result.Error != null) {
			Debug.Log("UserCallBack - user graph request failed: " + result.Error + " - " + result.Text);

#if PROPELLER_SDK
			PropellerSDK.SdkSocialLoginCompleted (null);
#endif

			if (socialPost != SocialPost.NONE) {
#if PROPELLER_SDK
				switch (socialPost) {
				case SocialPost.INVITE:
					PropellerSDK.SdkSocialInviteCompleted ();
					break;
				case SocialPost.SHARE:
					PropellerSDK.SdkSocialShareCompleted ();
					break;
				}
#endif
				
				socialPost = SocialPost.NONE;
				socialPostData = null;
			}

			return;
		}

		string get_data = result.Text;
		
		var dict = Json.Deserialize(get_data) as IDictionary;
		fbname = dict ["name"].ToString();
		fbemail = dict ["email"].ToString();
		fbgender = dict ["gender"].ToString();
		fbfirstname = dict ["first_name"].ToString();
		
		PushFBDataToFuel ();

		if (socialPost != SocialPost.NONE) {
			switch (socialPost) {
			case SocialPost.INVITE:
				onSocialInviteClicked (socialPostData);
				break;
			case SocialPost.SHARE:
				onSocialShareClicked (socialPostData);
				break;
			}
		}
	}
	
	public void trySocialLogin(bool allowCache)                                                                       
	{
		Debug.Log("trySocialLogin");

		if (FB.IsLoggedIn) {
			if (allowCache == true) {
				PushFBDataToFuel ();
				return;
			}

			FB.Logout ();
		}

		FB.Login("public_profile,email,publish_actions", LoginCallback);
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

#if PROPELLER_SDK
		PropellerSDK.SdkSocialLoginCompleted (loginInfo);
#endif
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
			FB.AppRequest (
				inviteInfo ["long"], 
				null, 
				null, 
				null, 
				null, 
				null, 
				inviteInfo ["subject"], 
				appRequestCallback);
		} 
		else 
		{
			if (socialPost != SocialPost.NONE) {
				socialPost = SocialPost.NONE;
				socialPostData = null;

#if PROPELLER_SDK
				PropellerSDK.SdkSocialInviteCompleted ();
#endif
				return;
			}
			
			socialPost = SocialPost.INVITE;
			socialPostData = inviteInfo;

			trySocialLogin (false);
		}
		                                                                                                            
		
	}                                                                                                                              

	private void appRequestCallback (FBResult result)                                                                              
	{     
		Debug.Log("appRequestCallback");  
		
		if (result.Error != null) {
			Debug.Log ("appRequestCallback - invite request failed: " + result.Error);
		} else  {
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			
			object obj = null;
			
			if (responseObject.TryGetValue ("cancelled", out obj)) {
				Debug.Log("appRequestCallback - invite request cancelled");
			} else if (responseObject.TryGetValue ("request", out obj)) {
				Debug.Log("appRequestCallback - invite request sent");
			}
		}

#if PROPELLER_SDK
		PropellerSDK.SdkSocialInviteCompleted();
#endif
	}  



	/*
	 -----------------------------------------------------
	 				FaceBook Share
	 -----------------------------------------------------
	*/
	
	public void onSocialShareClicked(Dictionary<string, string> shareInfo)                                                                                              
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
			FB.Feed (
				FB.UserId, 
				shareInfo ["link"], 
				shareInfo ["subject"], 
				shareInfo ["short"],
				shareInfo ["long"], 
				shareInfo ["picture"], 
				null, 
				null, 
				null, 
				null, 
				null,
				appFeedCallback);
		} 
		else 
		{
			if (socialPost != SocialPost.NONE) {
				socialPost = SocialPost.NONE;
				socialPostData = null;

#if PROPELLER_SDK
				PropellerSDK.SdkSocialShareCompleted ();
#endif
				return;
			}

			socialPost = SocialPost.SHARE;
			socialPostData = shareInfo;

			trySocialLogin (false);
		}
		
		
	}                                                                                                                              

	private void appFeedCallback (FBResult result)                                                                              
	{     
		Debug.Log("appFeedCallback");  

		if (result.Error != null) {
			Debug.Log ("appFeedCallback - share request failed: " + result.Error);
		} else  {
			var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;

			object obj = null;

			if (responseObject.TryGetValue ("cancelled", out obj)) {
				Debug.Log("appFeedCallback - share request cancelled");
			} else if (responseObject.TryGetValue ("request", out obj)) {
				Debug.Log("appFeedCallback - share request sent");
			}
		}

#if PROPELLER_SDK
		PropellerSDK.SdkSocialShareCompleted();
#endif
	}  


	
	/*
	 ---------------------------------------------------------------------
								Fuel Dynamics
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
		if( isDeviceTablet() == true) {
			isTablet = "TRUE";
		}

		int userAge = getUserAge ();
		int numLaunches = getNumLaunches ();
		int numSessions = getNumSessions ();

		float _latitude = 0.0f;
		float _longitude = 0.0f;
		#if LOCATION_SERVICES
		if (Input.location.status == LocationServiceStatus.Running) 
		{
			_latitude = Input.location.lastData.latitude;
			_longitude = Input.location.lastData.longitude;
		}
		#endif

		Dictionary<string, string> conditions = new Dictionary<string, string> ();
		
		//required
		conditions.Add ("userAge", userAge.ToString());
		conditions.Add ("numSessions", numSessions.ToString());
		conditions.Add ("numLaunches", numLaunches.ToString());
		conditions.Add ("isTablet", isTablet);
		conditions.Add ("gameVersion", "1.0.2");

		//standardized
		conditions.Add ("orientation", "portrait");
		conditions.Add ("daysSinceFirstPayment", "-1");
		conditions.Add ("daysSinceLastPayment", "-1");
		conditions.Add ("language", "en");
		conditions.Add ("gender", "female");
		conditions.Add ("age", "16");
		conditions.Add ("gpsLong", _latitude.ToString());
		conditions.Add ("gpsLat", _longitude.ToString());
		
		//non standardized

#if PROPELLER_SDK
		PropellerSDK.SetUserConditions (conditions);
#endif

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
				"gameVersion = " + "1.0.2"
		);

	}
	
	public void syncUserValues()
	{
#if PROPELLER_SDK
		PropellerSDK.SyncUserValues();
#endif
	}
	
	public void OnPropellerSDKUserValues (Dictionary<string, string> userValuesInfo)
	{
		Debug.Log("OnPropellerSDKUserValues");

		//Game Values - defined in the CSV
		String _friction = "friction";
		String _geartype = "geartype";
		String _gametime = "gametime";
		String _showdebug = "showdebug";
		String _fbicon = "fbicon";
		String _split1name = "split1name";



		string value;
		if (userValuesInfo.TryGetValue (_friction, out value)) {
			GearFriction = float.Parse (value.ToString ());
		} else {
			Debug.Log("friction not found in userValueInfo");
		}
		
		if (userValuesInfo.TryGetValue (_geartype, out value)) {
			GearShapeType = int.Parse(value.ToString());
		} else {
			Debug.Log("friction not found in userValueInfo");
		}

		if (userValuesInfo.TryGetValue (_gametime, out value)) {
			GameTime = int.Parse(value.ToString());
		} else {
			Debug.Log("friction not found in userValueInfo");
		}

		if (userValuesInfo.TryGetValue (_showdebug, out value)) {
			ShowDebug = int.Parse(value.ToString());
		} else {
			Debug.Log("showdebug not found in userValueInfo");
		}

		if (userValuesInfo.TryGetValue (_fbicon, out value)) {
			FBIcon = int.Parse(value.ToString());
		} else {
			Debug.Log("fbicon not found in userValueInfo");
		}

		if (userValuesInfo.TryGetValue (_split1name, out value)) {
			Split1Name = value.ToString();
		} else {
			Debug.Log("split1name not found in userValueInfo");
		}

		Debug.Log ("TryGetValue:: friction = " + GearFriction + ", geartype = " + GearShapeType + ", gametime = " + GameTime + ", showdebug = " + ShowDebug + ", fbicon = " + FBIcon + ", split1name = " + Split1Name);


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
		if (PlayerPrefs.HasKey ("showdebug")) {
			PlayerPrefs.SetInt("showdebug", ShowDebug);
		}
		if (PlayerPrefs.HasKey ("fbicon")) {
			PlayerPrefs.SetInt("fbicon", FBIcon);
		}
		if (PlayerPrefs.HasKey ("splitgroup")) {
			PlayerPrefs.SetString("splitgroup", Split1Name);
		}

		NotificationCenter.DefaultCenter.PostNotification(getMainMenuClass(), "RefreshDebugText");

#if USE_ANALYTICS
		flurryService.LogEvent("OnFuelDynamicsUserValues", analyticResult);
#endif

	}
	
	private bool isDeviceTablet ()
	{
		bool isTablet = false;
		
		#if UNITY_IOS

			if (Application.platform == RuntimePlatform.IPhonePlayer) {
				
				if(UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadUnknown ||
				   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad1Gen ||
				   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad2Gen ||
				   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad3Gen ||
				   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPad4Gen ||
				   UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadMini1Gen)
				{
					isTablet = true;
				}
			}

		#else

			float screenWidth = Screen.width / Screen.dpi;
			float screenHeight = Screen.height / Screen.dpi;
			float size = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));
			size =  (float)Mathf.Round(size * 10f) / 10f;
			Debug.Log("IsTablet inches = " + size);
			if(size >= 7.0)
				isTablet = true;
			else
				isTablet = false;

		#endif
		
		return isTablet;
	}


	
	
	//Unit Tests
	private void VirtualGoodUnitTest()
	{
		Hashtable goodsTable = new Hashtable();  
		goodsTable["addGold"] = 2;                          
		goodsTable["addOil"] = 2;                          
		goodsTable["showTrophy"] = 1;                          
		
		getMainMenuClass().RefreshVirtualGoods(goodsTable);
	}
	
	

	/*
	 ---------------------------------------------------------------------
						Notifications & Deep Linking
	 ---------------------------------------------------------------------
    */
	public void OnPropellerSDKNotification(string applicationState)
	{
		Debug.Log("OnPropellerSDKNotification");

		AutoLauncher.Instance ().ValidateAutoLauncher (applicationState);
	}
	
	
	private void SetLanguageLocale()
	{
		Dictionary<string, string> langLookup = new Dictionary<string, string> ();
		
		langLookup.Add ("English", "en");
		langLookup.Add ("French", "fr");
		langLookup.Add ("German", "de");
		langLookup.Add ("Spanish", "es");
		langLookup.Add ("Italian", "it");
		langLookup.Add ("Portuguese", "pt");
		
		langLookup.Add ("Chinese", "zh");
		langLookup.Add ("ChineseSimplified", "zh");
		langLookup.Add ("Korean", "ko");
		langLookup.Add ("Japanese", "ja");
		langLookup.Add ("Russian", "ru");
		langLookup.Add ("Arabic", "ar");
		
		var unityLang = Application.systemLanguage;
		
		string langCode;
		if (langLookup.TryGetValue (unityLang.ToString (), out langCode)) {
#if PROPELLER_SDK
			PropellerSDK.SetLanguageCode (langCode);
#endif
		} else {
			Debug.Log("SetLanguageLocale Error: " + unityLang.ToString() + " not supported.");
#if PROPELLER_SDK
			PropellerSDK.SetLanguageCode ("en");
#endif
		}
	}

}






