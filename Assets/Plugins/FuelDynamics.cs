using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using PropellerSDKSimpleJSON;
	
public class FuelDynamics : MonoBehaviour
{
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
	/// False if required conditions not set
	/// </returns>
	/// <param name='conditions'>
	/// A dictionary filled with values the SDK needs to use to properly pass the result to the Propeller servers. 
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
				succeeded = PropellerCommon.iOSSetUserConditions( conditionsJSON.ToString () );
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
			PropellerCommon.iOSSyncUserValues();
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

#if UNITY_ANDROID
				m_jniPropellerUnity = new AndroidJavaClass( "com.fuelpowered.lib.propeller.unity.PropellerSDKUnitySingleton" );
#endif

				InitializeDynamics (GameKey, GameSecret, UseTestServers);

#if UNITY_ANDROID
				m_jniPropellerUnity.CallStatic<bool>("SetNotificationIcon", AndroidNotificationIcon);
				m_jniPropellerUnity.CallStatic("InitializeGCM", AndroidGCMSenderID);
#endif
				if (!string.IsNullOrEmpty (HostGameObjectName)) {
					m_hostGameObject = GameObject.Find (HostGameObjectName);

					PropellerCommon.SetHostGameObject(m_hostGameObject);
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
			m_jniPropellerUnity.CallStatic("InitializeDynamics", key, secret);
#endif
		}
		
		Debug.Log ("InitializeDynamics - end");
	}
	#endregion


	#region Callback Functions

	private void PropellerOnUserValues (string message)
    {
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("FuelDynamicsUserValues - null or empty message");
			return;
        }

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
