using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using PropellerSDKSimpleJSON;
	
public class FuelDynamics : MonoBehaviour
{

	#region Unity Editor Fields
	public string GameKey;
	public string GameSecret;
	public bool UseTestServers = false;
	public string HostGameObjectName;
	#endregion
	
	#region Fields
	private static bool m_bInitialized;
	private static GameObject m_hostGameObject;
	#endregion

	#region Build-Target Specific Fields and Functions
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void iOSInitializeDynamics(string key, string secret, bool useTestServers);
#elif UNITY_ANDROID
	private static AndroidJavaClass m_jniPropellerUnity = null;
#endif
	#endregion

	static FuelDynamics ()
	{
		m_bInitialized = false;
	}
	
	#region Public Functions

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
	public static bool SetUserConditions (Dictionary<string, string> conditions)
	{
		Debug.Log ("SetUserConditions - start");

		bool succeeded = false;

		if (!Application.isEditor) {
			Dictionary<string, object> conditionsToJSON = null;

			if (conditions != null) {
				// convert conditions into Dictionary<string, object> since
				// toJSONClass will not accept Dictionary<string, string>
				conditionsToJSON = new Dictionary<string, object> ();

				foreach (KeyValuePair<string, string> condition in conditions) {
					conditionsToJSON.Add (
						condition.Key,
						condition.Value);
				}
			}

			JSONClass conditionsJSON = PropellerCommon.toJSONClass (conditionsToJSON);

			if (conditionsJSON == null) {
				Debug.Log ("SetUserConditions - conditions parse error");
			} else {
#if UNITY_IPHONE
				succeeded = PropellerCommon.iOSSetUserConditions( conditionsJSON.ToString () );
#elif UNITY_ANDROID
				using (AndroidJavaObject conditionsJSONString = new AndroidJavaObject("java.lang.String", conditionsJSON.ToString ()))
				{
					succeeded = m_jniPropellerUnity.CallStatic<bool>("SetUserConditions", conditionsJSONString);
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
	public static bool SyncUserValues ()
	{
		Debug.Log ("SyncUserValues - start");

		bool succeeded = false;

		if (!Application.isEditor) {
#if UNITY_IPHONE
			succeeded = PropellerCommon.iOSSyncUserValues();
#elif UNITY_ANDROID
			succeeded = m_jniPropellerUnity.CallStatic<bool>("SyncUserValues");
#endif
		}

		Debug.Log ("SyncUserValues - end");

		return succeeded;
	}

	#endregion


	
	#region Unity Functions
	private void Awake ()
	{
		if (!m_bInitialized) {
			GameObject.DontDestroyOnLoad (gameObject);

			if (!Application.isEditor) {

#if UNITY_ANDROID
				m_jniPropellerUnity = new AndroidJavaClass( "com.fuelpowered.lib.propeller.unity.PropellerSDKUnitySingleton" );
#endif

				InitializeDynamics (GameKey, GameSecret, UseTestServers);


				if (!string.IsNullOrEmpty (HostGameObjectName)) {
					m_hostGameObject = GameObject.Find (HostGameObjectName);

					PropellerCommon.SetHostGameObject(m_hostGameObject);
					
					GameObject common = new GameObject();
					common.name = "PropellerCommon";
					common.AddComponent<PropellerCommon>();
				}

			}

			m_bInitialized = true;
		} else {
			GameObject.Destroy (gameObject);	
		}
	}
	
	#endregion

		
	#region Initialize
	private static void InitializeDynamics (string key, string secret, bool useTestServers)
	{
		Debug.Log ("InitializeDynamics - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSInitializeDynamics(key, secret, useTestServers);
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("InitializeDynamics", key, secret, useTestServers);
#endif
		}
		
		Debug.Log ("InitializeDynamics - end");
	}
	#endregion
	
}
