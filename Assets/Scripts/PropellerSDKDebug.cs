using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class PropellerSDKDebug : MonoBehaviour {

#if PROPELLER_SDK_DEBUG

#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern bool iOSUseDebugServers(string sdkHost, string apiHost, string tournamentHost, string challengeHost, string cdnHost, string transactionHost, string dynamicsHost);
#elif UNITY_ANDROID
	private AndroidJavaClass _jniPropellerSDK;
#endif

	private string _gameKey;
	private string _gameSecret;
	private string _sdkHost;
	private string _apiHost;
	private string _tournamentHost;
	private string _challengeHost;
	private string _cdnHost;
	private string _transactionHost;
	private string _dynamicsHost;
	private string _androidGCMSenderID;

	void Awake ()
	{
#if UNITY_ANDROID
		_jniPropellerSDK = new AndroidJavaClass( "com.fuelpowered.lib.propeller.PropellerSDK" );
#endif

#if PROPELLER_SDK_DEBUG_INTERNAL
		_gameKey = "5522ed09663330000b090000";
		_gameSecret = "62363bb2-cecc-a2bc-694c-5e5e532a214f";
		_sdkHost = "https://api-internal.fuelpowered.com/sdk/";
		_apiHost = "https://api-internal.fuelpowered.com/api/v1";
		_tournamentHost = "https://api-internal.fuelpowered.com/api/v1";
		_challengeHost = "https://challenge-internal.fuelpowered.com/v1";
		_cdnHost = "http://cdn-internal.fuelpowered.com/api/v1";
		_transactionHost = "https://transaction-internal.fuelpowered.com/api";
		_dynamicsHost = "http://apiv2-internal.fuelpowered.com/api/v2";
		_androidGCMSenderID = "630730529138";
#elif PROPELLER_SDK_DEBUG_SANDBOX
		_gameKey = "5477a6c17061701423190000";
		_gameSecret = "bc8939af-ac67-de48-7791-a4dc76ac3cfe";
		_sdkHost = "https://api-sandbox.fuelpowered.com/sdk/";
		_apiHost = "https://api-sandbox.fuelpowered.com/api/v1";
		_tournamentHost = "https://api-sandbox.fuelpowered.com/api/v1";
		_challengeHost = "https://challenge-sandbox.fuelpowered.com/v1";
		_cdnHost = "http://cdn-sandbox.fuelpowered.com/api/v1";
		_transactionHost = "https://transaction-sandbox.fuelpowered.com/api";
		_dynamicsHost = "https://api-sandbox.fuelpowered.com/api/v2";
		_androidGCMSenderID = "630730529138";
#elif PROPELLER_SDK_DEBUG_PRODUCTION
		_gameKey = "542b3bec636f62427de7ac00";
		_gameSecret = "7944a1a9-fc41-8789-4c67-dc98bb4c1743";
		_sdkHost = "https://api.fuelpowered.com/sdk/";
		_apiHost = "https://api.fuelpowered.com/api/v1";
		_tournamentHost = "https://api.fuelpowered.com/api/v1";
		_challengeHost = "https://challenge.fuelpowered.com/v1";
		_cdnHost = "http://cdn.fuelpowered.com/api/v1";
		_transactionHost = "https://transaction.fuelpowered.com/api";
		_dynamicsHost = "https://api.fuelpowered.com/api/v2";
		_androidGCMSenderID = "630730529138";
#else
		return;
#endif

		UpdatePropellerSDKPrefab ();
		UseDebugServers ();
	}

	PropellerSDK GetPropellerSDK ()
	{
		GameObject gameObject = GameObject.Find("PropellerSDK");

		if (gameObject == null) {
			throw new Exception("Unable to find the Propeller SDK game object");
		}

		PropellerSDK propellerSDK = gameObject.GetComponent<PropellerSDK> ();

		if (propellerSDK == null) {
			throw new Exception("Unable to obtain the Propeller SDK script");
		}

		return propellerSDK;
	}

	void UpdatePropellerSDKPrefab ()
	{
		PropellerSDK propellerSDK = GetPropellerSDK ();
		propellerSDK.GameKey = _gameKey;
		propellerSDK.GameSecret = _gameSecret;
		propellerSDK.AndroidGCMSenderID = _androidGCMSenderID;
	}

	bool UseDebugServers ()
	{
#if UNITY_IPHONE
		return iOSUseDebugServers (_sdkHost, _apiHost, _tournamentHost, _challengeHost, _cdnHost, _transactionHost, _dynamicsHost);
#elif UNITY_ANDROID
		return _jniPropellerSDK.CallStatic<bool>("useDebugServers", _sdkHost, _apiHost, _tournamentHost, _challengeHost, _cdnHost, _transactionHost, _dynamicsHost);
#endif
	}

#endif
}
