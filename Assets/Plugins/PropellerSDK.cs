using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using PropellerSDKSimpleJSON;
	
public class PropellerSDK : MonoBehaviour
{
	public enum ContentOrientation
	{
		landscape,
		portrait,
		auto
	}
	;

	public enum NotificationType
	{
		none = 0x0,
		all = 0x3,
		push = 1 << 0,
		local = 1 << 1
	}
	;

	private enum DataType
	{
		intType,
		longType,
		floatType,
		doubleType,
		boolType,
		stringType
	}
	;

	#region Unity Editor Fields
	public string GameKey;
	public string GameSecret;
	public bool UseTestServers = false;
	public ContentOrientation Orientation = ContentOrientation.landscape;
	public string HostGameObjectName;
	public bool iOSGameHandleLogin;
	public bool iOSGameHandleInvite;
	public bool iOSGameHandleShare;
	public string AndroidNotificationIcon = "notify_icon";
	public string AndroidGCMSenderID;
	public bool AndroidGameHandleLogin;
	public bool AndroidGameHandleInvite;
	public bool AndroidGameHandleShare;
	#endregion
	
	#region Fields
	private static bool m_bInitialized;
	private static PropellerSDKListener m_listener;
	private static PropellerSDKNotificationListener m_notificationListener;
	private static GameObject m_hostGameObject;
	#endregion

	#region Build-Target Specific Fields and Functions
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void iOSInitialize(string key, string secret, string screenOrientation, bool useTestServers, bool gameHasLogin, bool gameHasInvite, bool gameHasShare);
	[DllImport ("__Internal")]
	private static extern void iOSSetLanguageCode(string languageCode);
	[DllImport ("__Internal")]
	private static extern bool iOSLaunch();
	[DllImport ("__Internal")]
	private static extern bool iOSSubmitMatchResult(string delimitedMatchInfo);
	[DllImport ("__Internal")]
	private static extern void iOSSyncChallengeCounts();
	[DllImport ("__Internal")]
	private static extern void iOSSyncTournamentInfo();
	[DllImport ("__Internal")]
	private static extern void iOSSyncVirtualGoods();
	[DllImport ("__Internal")]
	private static extern void iOSAcknowledgeVirtualGoods(string transactionId, bool consumed);
	[DllImport ("__Internal")]
	private static extern void iOSEnableNotification(NotificationType notificationType);
	[DllImport ("__Internal")]
	private static extern void iOSDisableNotification(NotificationType notificationType);
	[DllImport ("__Internal")]
	private static extern bool iOSIsNotificationEnabled(NotificationType notificationType);
	[DllImport ("__Internal")]
	private static extern void iOSSdkSocialLoginCompleted(string loginInfo);
	[DllImport ("__Internal")]
	private static extern void iOSSdkSocialInviteCompleted();
	[DllImport ("__Internal")]
	private static extern void iOSSdkSocialShareCompleted();
	[DllImport ("__Internal")]
	private static extern void iOSRestoreAllLocalNotifications();
#elif UNITY_ANDROID
	private static AndroidJavaClass m_jniPropellerUnity = null;
#endif
	#endregion

	static PropellerSDK ()
	{
		m_bInitialized = false;
	}
	
	#region Public Functions


	/// <summary>
	/// Sets the language code for the Propeller SDK online content. Must be compliant to ISO 639-1.
	/// If the language code is not supported, then the content will default to English (en)
	/// </summary>
	/// <param name='languageCode'>
	/// Two character string language code to set the Propeller SDK online content to.
	/// </param>
	public static void SetLanguageCode (string languageCode)
	{
		Debug.Log ("SetLanguageCode - start");

		if (!Application.isEditor) {
			Debug.Log ("SetLanguageCode - " + languageCode);
#if UNITY_IPHONE
			iOSSetLanguageCode(languageCode);
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic ("SetLanguageCode", languageCode);
#endif
		}

		Debug.Log ("SetLanguageCode - end");
	}

	/// <summary>
	/// Launch the SDK with the provided listener to handle callbacks.
	/// </summary>
	/// <param name='listener'>
	/// A class that subclasses the PropellerSDKListener abstract class that will receive various callbacks.
	/// </param>
	public static bool Launch (PropellerSDKListener listener)
	{
		Debug.Log ("Launch - start");
		
		m_listener = listener;
		
		bool succeeded = false;
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			succeeded = iOSLaunch();
#elif UNITY_ANDROID
			succeeded = m_jniPropellerUnity.CallStatic<bool>("Launch");
#endif
		}
		
		Debug.Log ("Launch - end");
		
		return succeeded;
	}
	
	/// <summary>
	/// Submits the results of the match. You must stuff the match result into a dictionary that will be parsed and passed to the SDK API servers..
	/// Current parameters:
	/// 	matchID
	/// 	tournamentID
	/// 	score
	/// </summary>
	/// <returns>
	/// True if the match results were submitted.
	/// </returns>
	/// <param name='matchResult'>
	/// A dictionary filled with values the SDK needs to use to properly pass the result to the API servers. Examples: score, tournamentID, matchID, etc.
	/// </param>
	public static bool SubmitMatchResult (Dictionary<string, object> matchResult)
	{
		Debug.Log ("SubmitMatchResult - start");
		
		bool succeeded = false;
		
		if (!Application.isEditor) {
			JSONClass matchResultJSON = toJSONClass (matchResult);

			if (matchResultJSON == null) {
				Debug.Log ("SubmitMatchResult - match result parse error");
			} else {
#if UNITY_IPHONE
				succeeded = iOSSubmitMatchResult( matchResultJSON.ToString () );
#elif UNITY_ANDROID
				using (AndroidJavaObject matchResultJavaJSONString = new AndroidJavaObject("java.lang.String", matchResultJSON.ToString ()))
				{
					succeeded = m_jniPropellerUnity.CallStatic<bool> ("SubmitMatchResult", matchResultJavaJSONString);
				}
#endif
			}
		}
		
		Debug.Log ("SubmitMatchResult - end");
		
		return succeeded;
	}
	
	/// <summary>
	/// Begins an asynchronous operation to request the player's challenge counts from Propeller.
	/// </summary>
	public static void SyncChallengeCounts ()
	{
		Debug.Log ("SyncChallengeCounts - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSSyncChallengeCounts();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic( "SyncChallengeCounts");
#endif
		}
		
		Debug.Log ("SyncChallengeCounts - end");
	}

	/// <summary>
	/// Begins an asynchronous operation to request the tournament information from Propeller.
	/// </summary>
	public static void SyncTournamentInfo ()
	{
		Debug.Log ("SyncTournamentInfo - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSSyncTournamentInfo();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic( "SyncTournamentInfo");
#endif
		}
		
		Debug.Log ("SyncTournamentInfo - end");
	}

	/// <summary>
	/// Begins an asynchronous operation to request the virtual goods from Propeller.
	/// </summary>
	public static void SyncVirtualGoods ()
	{
		Debug.Log ("SyncVirtualGoods - start");

		if (!Application.isEditor) {
#if UNITY_IPHONE
            iOSSyncVirtualGoods();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("SyncVirtualGoods");
#endif
		}

		Debug.Log ("SyncVirtualGoods - end");
	}

	/// <summary>
	/// Begins an asynchronous operation to acknowledge the received virtual goods from Propeller.
	/// </summary>
	/// <param name='transactionId'>
	/// The transaction ID being acknowledged
	/// </param>
	/// <param name='consumed'>
	/// Flags whether or not the virutal good were consumed
	/// </param>
	public static void AcknowledgeVirtualGoods (string transactionId, bool consumed)
	{
		Debug.Log ("AcknowledgeVirtualGoods - start");

		if (!Application.isEditor) {
#if UNITY_IPHONE
            iOSAcknowledgeVirtualGoods(transactionId, consumed);
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("AcknowledgeVirtualGoods", transactionId, consumed);
#endif
		}

		Debug.Log ("AcknowledgeVirtualGoods - end");
	}
	
	/// <summary>
	/// Sets the notification listener
	/// </summary>
	/// <param name='listener'>
	/// Notification listener to set
	/// </param>
	public static void SetNotificationListener (PropellerSDKNotificationListener listener)
	{
		m_notificationListener = listener;
	}

	/// <summary>
	/// Enables notifications of the given type
	/// </summary>
	/// <param name='notificationType'>
	/// The notification type to enabled
	/// </param>
	public static void EnableNotification (NotificationType notificationType)
	{
		Debug.Log ("EnableNotification - start");

		if (!Application.isEditor) {
			Debug.Log ("EnableNotification - " + notificationType);
#if UNITY_IPHONE
			iOSEnableNotification(notificationType);
#elif UNITY_ANDROID
			AndroidJavaObject propellerSDKNotificationType = GetPropellerSDKNotificationType(notificationType);

			if (propellerSDKNotificationType == null)
			{
				Debug.Log ("EnableNotification - invalid notification type");
				return;
			}

			m_jniPropellerUnity.CallStatic<bool>("EnableNotification", propellerSDKNotificationType);
#endif
		}

		Debug.Log ("EnableNotification - end");
	}

	/// <summary>
	/// Disables notifications of the given type
	/// </summary>
	/// <param name='notificationType'>
	/// The notification type to disable
	/// </param>
	public static void DisableNotification (NotificationType notificationType)
	{
		Debug.Log ("DisableNotification - start");

		if (!Application.isEditor) {
			Debug.Log ("DisableNotification - " + notificationType);
#if UNITY_IPHONE
			iOSDisableNotification(notificationType);
#elif UNITY_ANDROID
			AndroidJavaObject propellerSDKNotificationType = GetPropellerSDKNotificationType(notificationType);
			
			if (propellerSDKNotificationType == null)
			{
				Debug.Log ("DisableNotification - invalid notification type");
				return;
			}
			
			m_jniPropellerUnity.CallStatic<bool>("DisableNotification", propellerSDKNotificationType);
#endif
		}

		Debug.Log ("DisableNotification - end");
	}

	/// <summary>
	/// Validates the enabled state of the given notification type
	/// </summary>
	/// <returns>
	/// True if the given notification type is enabled, false otherwise
	/// </returns>
	/// <param name='notificationType'>
	/// The notification type validate
	/// </param>
	public static bool IsNotificationEnabled (NotificationType notificationType)
	{
		Debug.Log ("IsNotificationEnabled - start");

		bool succeed = false;

		if (!Application.isEditor) {
#if UNITY_IPHONE
			succeed = iOSIsNotificationEnabled(notificationType);
#elif UNITY_ANDROID
			AndroidJavaObject propellerSDKNotificationType = GetPropellerSDKNotificationType(notificationType);
			
			if (propellerSDKNotificationType == null)
			{
				Debug.Log ("IsNotificationEnabled - invalid notification type");
				return false;
			}
			
			succeed = m_jniPropellerUnity.CallStatic<bool>("IsNotificationEnabled", propellerSDKNotificationType);
#endif
			Debug.Log ("IsNotificationEnabled - " + notificationType + ":" + succeed);
		}

		Debug.Log ("IsNotificationEnabled - end");

		return succeed;
	}

#if UNITY_ANDROID
	/// <summary>
	/// Retrieves the AndroidJavaObject PropellerSDKNotificationType equivalent to the given NotificationType
	/// </summary>
	/// <returns>
	/// The AndroidJavaObject PropellerSDKNotificationType equivalent to the given notification type, null otherwise
	/// </returns>
	/// <param name='notificationType'>
	/// The notification type whose equivalent will be retrieved
	/// </param>
	private static AndroidJavaObject GetPropellerSDKNotificationType(NotificationType notificationType)
	{
		int notificationTypeValue = (int)notificationType;

		AndroidJavaClass propellerSDKNotificationTypeClass = new AndroidJavaClass("com.fuelpowered.lib.propeller.PropellerSDKNotificationType");

		return propellerSDKNotificationTypeClass.CallStatic<AndroidJavaObject>("findByValue", notificationTypeValue);
	}
#endif

	/// <summary>
	/// Begins an asynchronous operation to indicate login info is complete.
	/// </summary>
	public static void SdkSocialLoginCompleted (Dictionary<string, string> loginInfo)
	{
		Debug.Log ("SdkSocialLoginCompleted - start");

		if (!Application.isEditor) {
#if UNITY_IPHONE
			string urlEncodedString = null;

			if (loginInfo != null)
			{
				StringBuilder stringBuilder = new StringBuilder();

				bool first = true;

				foreach ( KeyValuePair<string,string> entry in loginInfo )
				{
					if (first)
					{
						first = false;
					}
					else
					{
						stringBuilder.Append( "&" );
					}

					stringBuilder.Append( WWW.EscapeURL (entry.Key) );
					stringBuilder.Append( "=" );
					stringBuilder.Append( WWW.EscapeURL (entry.Value) );
				}

				urlEncodedString = stringBuilder.ToString();
			}

			iOSSdkSocialLoginCompleted( urlEncodedString );
#elif UNITY_ANDROID
			using (AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap"))
			{
				if (loginInfo != null)
				{
					System.IntPtr hashMapPutMethodId = AndroidJNI.GetMethodID (
						hashMap.GetRawClass (),
						"put",
						"(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

					foreach (string key in loginInfo.Keys)
					{
						using (AndroidJavaObject hashMapPutMethodArgKey = new AndroidJavaObject("java.lang.String", key))
						{
							using (AndroidJavaObject hashMapPutMethodArgValue = new AndroidJavaObject ("java.lang.String", loginInfo [key]))
							{
								object[] args = new object[2];
								args [0] = hashMapPutMethodArgKey;
								args [1] = hashMapPutMethodArgValue;

								jvalue[] hashMapPutMethodArgs = AndroidJNIHelper.CreateJNIArgArray (args);

								AndroidJNI.CallObjectMethod (
									hashMap.GetRawObject (),
									hashMapPutMethodId,
									hashMapPutMethodArgs);
							}
						}
					}
				}

				m_jniPropellerUnity.CallStatic("SdkSocialLoginCompleted", hashMap);
			}
#endif
		}

		Debug.Log ("SdkSocialLoginCompleted - end");
	}

	/// <summary>
	/// Begins an asynchronous operation to indicate invite info is complete.
	/// </summary>
	public static void SdkSocialInviteCompleted ()
	{
		Debug.Log ("SdkSocialInviteCompleted - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSSdkSocialInviteCompleted();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic( "SdkSocialInviteCompleted");
#endif
		}
		
		Debug.Log ("SdkSocialInviteCompleted - end");
	}
	
	/// <summary>
	/// Begins an asynchronous operation to indicate sharing info is complete.
	/// </summary>
	public static void SdkSocialShareCompleted ()
	{
		Debug.Log ("SdkSocialShareCompleted - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSSdkSocialShareCompleted();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic( "SdkSocialShareCompleted");
#endif
		}
		
		Debug.Log ("SdkSocialShareCompleted - end");
	}
	
	/// <summary>
	/// Restores cancelled Propeller SDK local notifications
	/// </summary>
	public static void RestoreAllLocalNotifications ()
	{
		Debug.Log ("RestoreAllLocalNotifications - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSRestoreAllLocalNotifications();
#elif UNITY_ANDROID
			Debug.Log ("RestoreAllLocalNotifications - unused by Android");
#endif
		}
		
		Debug.Log ("RestoreAllLocalNotifications - end");
	}

	/// <summary>
	/// FUEL DYNAMICS - SetUserConditions
	/// 
	/// </summary>
	/// <returns>
	/// True if the call to the SDK succeeded.
	/// </returns>
	/// <param name='conditions'>
	/// A dictionary filled with values the SDK needs to use to properly pass the result to the Propeller servers. Examples: score, tournamentID, matchID, etc.
	/// </param>
	public static bool SetUserConditions (Dictionary<string, object> conditions)
	{
		Debug.Log ("SetUserConditions - start");

		bool succeeded = false;

		if (!Application.isEditor) {
			JSONClass conditionsJSON = toJSONClass (conditions);

			if (conditionsJSON == null) {
				Debug.Log ("SetUserConditions - conditions parse error");
			} else {
#if UNITY_IPHONE
				succeeded = PropellerImports.iOSSetUserConditions( conditionsJSON.ToString () );
#elif UNITY_ANDROID
				using (AndroidJavaObject conditionsJSONString = new AndroidJavaObject("java.lang.String", conditionsJSON.ToString ()))
				{
					m_jniPropellerUnity.CallStatic("SetUserConditions", conditionsJSONString);
				}
#endif
			}
		}

		Debug.Log ("SetUserConditions - end");

		return succeeded;
	}

	/// <summary>
	/// Begins an asynchronous operation to request the user values from Propeller.
	/// </summary>
	public static void SyncUserValues ()
	{
		Debug.Log ("SyncUserValues - start");

		if (!Application.isEditor) {
#if UNITY_IPHONE
			PropellerImports.iOSSyncUserValues();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic<bool>("SyncUserValues");
#endif
		}

		Debug.Log ("SyncUserValues - end");
	}

	#endregion

	#region Unity Functions
	private void Awake ()
	{
		if (!m_bInitialized) {
			GameObject.DontDestroyOnLoad (gameObject);

			if (!Application.isEditor) {
				bool gameHandleLogin = false;
				bool gameHandleInvite = false;
				bool gameHandleShare = false;
#if UNITY_IPHONE
				gameHandleLogin = iOSGameHandleLogin;
				gameHandleInvite = iOSGameHandleInvite;
				gameHandleShare = iOSGameHandleShare;
#elif UNITY_ANDROID
				m_jniPropellerUnity = new AndroidJavaClass( "com.fuelpowered.lib.propeller.unity.PropellerSDKUnitySingleton" );

				gameHandleLogin = AndroidGameHandleLogin;
				gameHandleInvite = AndroidGameHandleInvite;
				gameHandleShare = AndroidGameHandleShare;				
#endif
				Initialize (GameKey, GameSecret, Orientation.ToString (), UseTestServers, gameHandleLogin, gameHandleInvite, gameHandleShare);

#if UNITY_ANDROID
				m_jniPropellerUnity.CallStatic<bool>("SetNotificationIcon", AndroidNotificationIcon);
				m_jniPropellerUnity.CallStatic("InitializeGCM", AndroidGCMSenderID);
#endif
				if (!string.IsNullOrEmpty (HostGameObjectName)) {
					m_hostGameObject = GameObject.Find (HostGameObjectName);
				}
			}
			
			m_bInitialized = true;
		} else {
			GameObject.Destroy (gameObject);	
		}
	}
	
	private void OnApplicationPause (bool paused)
	{
		if (!Application.isEditor) {
			if (paused) {
#if UNITY_ANDROID
				 m_jniPropellerUnity.CallStatic("OnPause");
#endif
			} else {
#if UNITY_ANDROID
				 m_jniPropellerUnity.CallStatic("OnResume");
#endif
			}
		}
		
	}
	
	private void OnApplicationQuit ()
	{
		if (!Application.isEditor) {
#if UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("OnQuit");
#endif
		}
	}
	#endregion
		
	#region Initialize
	private static void Initialize (string key, string secret, string screenOrientation, bool useTestServers, bool gameHasLogin, bool gameHasInvite, bool gameHasShare)
	{
		Debug.Log ("Initialize - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSInitialize(key, secret, screenOrientation, useTestServers, gameHasLogin, gameHasInvite, gameHasShare);
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("Initialize", key, secret, screenOrientation, useTestServers, gameHasLogin, gameHasInvite, gameHasShare);
#endif
		}
		
		Debug.Log ("Initialize - end");
	}
	#endregion
	
	#region Callback Functions
	private void PropellerOnSdkCompletedWithExit (string message)
	{
		Debug.Log ("PropellerOnSdkCompletedWithExit");

		if (!string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkCompletedWithExit - " + message);
		}
		
		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkCompletedWithExit - undefined listener");
			return;
		}

		m_listener.SdkCompletedWithExit ();
	}
	
	private void PropellerOnSdkCompletedWithMatch (string message)
	{
		Debug.Log ("PropellerOnSdkCompletedWithMatch");

		if (string.IsNullOrEmpty (message)) {
			Debug.LogError ("PropellerOnSdkCompletedWithMatch - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkCompletedWithMatch - " + message);
		
		const char kDelimeter = '&';
		string[] resultsArray = message.Split (kDelimeter);
		
		if (resultsArray.Length != 3) {
			Debug.LogError ("PropellerOnSdkCompletedWithMatch - Invalid response from PropellerUnitySDK");
			return;
		}
		
		string tournamentID = resultsArray [0];
		string matchID = resultsArray [1];
		string paramsJSON = WWW.UnEscapeURL (resultsArray [2]);

		Dictionary<string, string> matchInfo = new Dictionary<string, string> ();
		matchInfo.Add ("tournamentID", tournamentID);
		matchInfo.Add ("matchID", matchID);
		matchInfo.Add ("paramsJSON", paramsJSON);
		
		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkCompletedWithExit - undefined listener");
			return;
		}

		m_listener.SdkCompletedWithMatch (matchInfo);
	}
	
	private void PropellerOnSdkFailed (string message)
	{
		Debug.Log ("PropellerOnSdkFailed");

		if (!string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkFailed - " + message);
		}
		
		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkFailed - undefined listener");
			return;
		}

		m_listener.SdkFailed (message);
	}
	
	private void PropellerOnChallengeCountChanged (string message)
	{
		Debug.Log ("PropellerOnChallengeCountChanged");

		// message must contain the challenge count or else its an error
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnChallengeCountChanged - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnChallengeCountChanged - " + message);
		
		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnChallengeCountChanged - undefined host game object");
			return;
		}

		m_hostGameObject.SendMessage ("OnPropellerSDKChallengeCountUpdated", message);
	}
	
	private void PropellerOnTournamentInfo (string message)
	{
		Debug.Log ("PropellerOnTournamentInfo");

		// message must be defined or else its an error
		if (message == null) {
			Debug.Log ("PropellerOnTournamentInfo - null message");
			return;
		}

		// can have no tournament info in the case where
		// no current or future tournament has been
		// scheduled

		Debug.Log ("PropellerOnTournamentInfo - " + message);

		string[] resultsArray = null;

		if (message.Length > 0) {
			const char kDelimeter = '&';

			resultsArray = message.Split (kDelimeter);

			if (resultsArray.Length != 6) {
				Debug.LogError ("PropellerOnTournamentInfo - Invalid response from PropellerUnitySDK");
				return;
			}
		}

		Dictionary<string, string> tournamentInfo = new Dictionary<string, string> ();

		if (resultsArray != null) {
			if (!string.IsNullOrEmpty (resultsArray [0])) {
				tournamentInfo.Add ("name", WWW.UnEscapeURL (resultsArray [0]));
			}

			if (!string.IsNullOrEmpty (resultsArray [1])) {
				tournamentInfo.Add ("campaignName", WWW.UnEscapeURL (resultsArray [1]));
			}

			if (!string.IsNullOrEmpty (resultsArray [2])) {
				tournamentInfo.Add ("sponsorName", WWW.UnEscapeURL (resultsArray [2]));
			}

			if (!string.IsNullOrEmpty (resultsArray [3])) {
				tournamentInfo.Add ("startDate", WWW.UnEscapeURL (resultsArray [3]));
			}

			if (!string.IsNullOrEmpty (resultsArray [4])) {
				tournamentInfo.Add ("endDate", WWW.UnEscapeURL (resultsArray [4]));
			}

			if (!string.IsNullOrEmpty (resultsArray [5])) {
				tournamentInfo.Add ("logo", WWW.UnEscapeURL (resultsArray [5]));
			}
		}

		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnTournamentInfo - undefined host game object");
			return;
		}

		m_hostGameObject.SendMessage ("OnPropellerSDKTournamentInfo", tournamentInfo);
	}

	private void PropellerOnVirtualGoodList (string message)
	{
		Debug.Log ("PropellerOnVirtualGoodList");

		// message must at least contain the transaction ID or else its an error
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnVirtualGoodList - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnVirtualGoodList - " + message);

		const char kDelimeter = '&';
		string[] resultsArray = message.Split (kDelimeter);

		if (resultsArray.Length == 0) {
			Debug.LogError ("PropellerOnVirtualGoodList - Invalid response from PropellerUnitySDK");
			return;
		}

		Dictionary<string, object> virtualGoodInfo = new Dictionary<string, object> ();
		virtualGoodInfo.Add ("transactionID", resultsArray [0]);

		List<string> virtualGoods = new List<string> ();

		for (int i = 1; i < resultsArray.Length; i++) {
			virtualGoods.Add (resultsArray [i]);
		}

		virtualGoodInfo.Add ("virtualGoods", virtualGoods);

		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnVirtualGoodList - undefined host game object");
			return;
		}

		m_hostGameObject.SendMessage ("OnPropellerSDKVirtualGoodList", virtualGoodInfo);
	}

	private void PropellerOnVirtualGoodRollback (string message)
	{
		Debug.Log ("PropellerOnVirtualGoodRollback");

		// message must contain the transaction ID or else its an error
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnVirtualGoodRollback - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnVirtualGoodRollback - " + message);

		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnVirtualGoodRollback - undefined host game object");
			return;
		}

		m_hostGameObject.SendMessage ("OnPropellerSDKVirtualGoodRollback", message);
	}

	private void PropellerOnSdkSocialLogin (string message)
	{
		Debug.Log ("PropellerOnSdkSocialLogin");

		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkSocialLogin - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkSocialLogin - " + message);
		
		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkSocialLogin - undefined listener");
			return;
		}

		bool allowCache = System.Convert.ToBoolean (message);
		m_listener.SdkSocialLogin (allowCache);
	}

	private void PropellerOnSdkSocialInvite (string message)
	{
		Debug.Log ("PropellerOnSdkSocialInvite");

		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkSocialInvite - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkSocialInvite - " + message);

		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkSocialInvite - undefined listener");
			return;
		}

		Dictionary<string, string> inviteDetail = new Dictionary<string, string> ();

		string[] resultsArray = message.Split ('&');

		foreach (string resultItem in resultsArray) {
			string[] keyValuePair = resultItem.Split ('=');

			string key = WWW.UnEscapeURL (keyValuePair [0]);
			string value = WWW.UnEscapeURL (keyValuePair [1]);

			inviteDetail.Add (key, value);
		}

		m_listener.SdkSocialInvite (inviteDetail);
	}

	private void PropellerOnSdkSocialShare (string message)
	{
		Debug.Log ("PropellerOnSdkSocialShare");

		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkSocialShare - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkSocialShare - " + message);

		if (m_listener == null) {
			Debug.Log ("PropellerOnSdkSocialShare - undefined listener");
			return;
		}

		Dictionary<string, string> shareDetail = new Dictionary<string, string> ();
		
		string[] resultsArray = message.Split ('&');
		
		foreach (string resultItem in resultsArray) {
			string[] keyValuePair = resultItem.Split ('=');

			string key = WWW.UnEscapeURL (keyValuePair [0]);
			string value = WWW.UnEscapeURL (keyValuePair [1]);
			
			shareDetail.Add (key, value);
		}
		
		m_listener.SdkSocialShare (shareDetail);
	}

	private void PropellerOnSdkOnNotificationEnabled (string message)
	{
		Debug.Log ("PropellerOnSdkOnNotificationEnabled");

		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkOnNotificationEnabled - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkOnNotificationEnabled - " + message);
		
		if (m_notificationListener == null) {
			// undefined notification listener
			return;
		}

		int notificationTypeValue = -1;
		
		if (!int.TryParse (message, out notificationTypeValue)) {
			Debug.Log ("PropellerOnSdkOnNotificationEnabled - unparsable notification type value");
			return;
		}
		
		System.Array notificationTypes = System.Enum.GetValues (typeof(NotificationType));
		NotificationType notificationType = NotificationType.none;
		bool foundNotificationType = false;
		
		foreach (NotificationType notificationTypeItem in notificationTypes) {
			notificationType = notificationTypeItem;

			if (((int)notificationType) == notificationTypeValue) {
				foundNotificationType = true;
				break;
			}
		}
		
		if (!foundNotificationType) {
			Debug.Log ("PropellerOnSdkOnNotificationEnabled - unsupported notification type");
			return;
		}
		
		m_notificationListener.SdkOnNotificationEnabled (notificationType);
	}

	private void PropellerOnSdkOnNotificationDisabled (string message)
	{
		Debug.Log ("PropellerOnSdkOnNotificationDisabled");

		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnSdkOnNotificationDisabled - null or empty message");
			return;
		}

		Debug.Log ("PropellerOnSdkOnNotificationDisabled - " + message);
		
		if (m_notificationListener == null) {
			// undefined notification listener
			return;
		}

		int notificationTypeValue = -1;
		
		if (!int.TryParse (message, out notificationTypeValue)) {
			Debug.Log ("PropellerOnSdkOnNotificationDisabled - unparsable notification type value");
			return;
		}
		
		System.Array notificationTypes = System.Enum.GetValues (typeof(NotificationType));
		NotificationType notificationType = NotificationType.none;
		bool foundNotificationType = false;
		
		foreach (NotificationType notificationTypeItem in notificationTypes) {
			notificationType = notificationTypeItem;

			if (((int)notificationType) == notificationTypeValue) {
				foundNotificationType = true;
				break;
			}
		}
		
		if (!foundNotificationType) {
			Debug.Log ("PropellerOnSdkOnNotificationDisabled - unsupported notification type");
			return;
		}
		
		m_notificationListener.SdkOnNotificationDisabled (notificationType);
	}

	private void PropellerOnUserValues (string message)
    {
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("FuelDynamicsUserValues - null or empty message");
			return;
        }

		Debug.Log ("PropellerOnUserValues::::::::::::::FuelDynamicsUserValues = " + message);

        const char kDelimeter = '&';
		string[] resultsArray = message.Split (kDelimeter);

		if (resultsArray.Length == 0 || resultsArray.Length%2 == 1) {
			Debug.LogError ("FuelDynamicsUserValues - Invalid response from PropellerUnitySDK");
			return;
		}

		Dictionary<string, object> userValuesInfo = new Dictionary<string, object> ();
		for(int i = 0; i < resultsArray.Length; i+=2) {
			userValuesInfo.Add (resultsArray[i], resultsArray[i+1]);	
		}

		if (m_hostGameObject == null) {
			Debug.Log ("FuelDynamicsUserValues - undefined host game object");
            return;
		}

		m_hostGameObject.SendMessage("OnPropellerSDKUserValues", userValuesInfo);
    }

	#endregion
	
	#region private utility methods
	private static JSONClass toJSONClass (Dictionary<string, object> dictionary)
	{
		if (dictionary == null) {
			return null;
		}

		JSONClass jsonClass = new JSONClass ();

		foreach (KeyValuePair<string, object> item in dictionary) {
			JSONNode jsonNode;

			if (item.Value is List<object>) {
				jsonNode = toJSONArray ((List<object>)item.Value);
			} else if (item.Value is Dictionary<string, object>) {
				jsonNode = toJSONClass ((Dictionary<string, object>)item.Value);
			} else {
				jsonNode = toJSONValue (item.Value);
			}

			if (jsonNode == null) {
				continue;
			}

			jsonClass.Add (item.Key, jsonNode);
		}

		return jsonClass;
	}

	private static JSONArray toJSONArray (List<object> list)
	{
		if (list == null) {
			return null;
		}

		JSONArray jsonArray = new JSONArray ();

		foreach (object item in list) {
			if (item == null) {
				continue;
			}

			JSONNode jsonNode;

			if (item is List<object>) {
				jsonNode = toJSONArray ((List<object>)item);
			} else if (item is Dictionary<string, object>) {
				jsonNode = toJSONClass ((Dictionary<string, object>)item);
			} else {
				jsonNode = toJSONValue (item);
			}

			if (jsonNode == null) {
				continue;
			}

			jsonArray.Add (jsonNode);
		}

		return jsonArray;
	}

	private static JSONClass toJSONValue (object data)
	{
		if (data == null) {
			return null;
		}

		DataType type;

		if (data is int) {
			type = DataType.intType;
		} else if (data is long) {
			type = DataType.longType;
		} else if (data is float) {
			type = DataType.floatType;
		} else if (data is double) {
			type = DataType.doubleType;
		} else if (data is bool) {
			type = DataType.boolType;
		} else if (data is string) {
			type = DataType.stringType;
		} else {
			return null;
		}

		JSONData jsonDataChecksum = new JSONData ("faddface");

		// Data type is stored as a string instead of remaining an
		// integer because of a bug in SimpleJSON where by the
		// JSONData.ToString() method surrounds the value with
		// double quotes, making it appear like a JSON string
		JSONData jsonDataType = new JSONData (((int)type).ToString ());

		JSONData jsonDataValue = new JSONData (data.ToString ());

		JSONClass jsonClass = new JSONClass ();
		jsonClass.Add ("checksum", jsonDataChecksum);
		jsonClass.Add ("type", jsonDataType);
		jsonClass.Add ("value", jsonDataValue);

		return jsonClass;
	}
	#endregion

}
