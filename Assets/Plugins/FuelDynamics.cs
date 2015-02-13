using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using PropellerSDKSimpleJSON;
	
public class FuelDynamics : MonoBehaviour
{
	/*
	public enum ContentOrientation
	{
		landscape,
		portrait,
		auto
	};

	public enum NotificationType
	{
		none = 0x0,
		all = 0x3,
		push = 1 << 0,
		local = 1 << 1
	};
	*/
	private enum DataType
	{
		intType,
		longType,
		floatType,
		doubleType,
		boolType,
		stringType
	};

	#region Unity Editor Fields
	public string GameKey;
	public string GameSecret;
	public string HostGameObjectName;
	#endregion
	
	#region Fields
	private static bool m_bInitialized;
	private static GameObject m_hostGameObject;
	#endregion

	#region Build-Target Specific Fields and Functions
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void iOSInitializeDynamicsOnly(string key, string secret);
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
	public static bool SetUserConditions (Dictionary<string, object> conditions)
	{
		Debug.Log ("SetUserConditions - start");
		
		bool succeeded = false;
		
		if (!Application.isEditor)
		{
			JSONClass conditionsJSON = toJSONClass (conditions);

			if (conditionsJSON == null)
			{
				Debug.Log ("SetUserConditions - conditions parse error");
			}
			else
			{
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
	public static void GetUserValues ()
	{
		Debug.Log ("GetUserValues - start");

		if (!Application.isEditor) {
#if UNITY_IPHONE
			PropellerImports.iOSGetUserValues();
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("GetUserValues");
#endif
		}

		Debug.Log ("GetUserValues - end");
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
				Initialize (GameKey, GameSecret);

				if (!string.IsNullOrEmpty(HostGameObjectName)) {
					m_hostGameObject = GameObject.Find(HostGameObjectName);
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
	private static void Initialize (string key, string secret)
	{
		Debug.Log ("Initialize - start");
		
		if (!Application.isEditor) {
#if UNITY_IPHONE
			iOSInitializeDynamicsOnly(key, secret);
#elif UNITY_ANDROID
			m_jniPropellerUnity.CallStatic("InitializeDynamicsOnly", key, secret);
#endif
		}
		
		Debug.Log ("Initialize - end");
	}
	#endregion
	
	#region Callback Functions


	private void FuelDynamicsUserValues (string message)
    {
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("FuelDynamicsUserValues - null or empty message");
			return;
        }

		Debug.Log ("FuelDynamicsUserValues = " + message);

        const char kDelimeter = '&';
		string[] resultsArray = message.Split (kDelimeter);

		if (resultsArray.Length == 0) {
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

		m_hostGameObject.SendMessage("OnFuelDynamicsUserValues", userValuesInfo);
    }

	#endregion
	
	#region private utility methods

	private static JSONClass toJSONClass(Dictionary<string, object> dictionary)
	{
		if (dictionary == null)
		{
			return null;
		}

		JSONClass jsonClass = new JSONClass ();

		foreach (KeyValuePair<string, object> item in dictionary)
		{
			JSONNode jsonNode;

			if (item.Value is List<object>)
			{
				jsonNode = toJSONArray ((List<object>) item.Value);
			}
			else if (item.Value is Dictionary<string, object>)
			{
				jsonNode = toJSONClass ((Dictionary<string, object>) item.Value);
			}
			else
			{
				jsonNode = toJSONValue (item.Value);
			}

			if (jsonNode == null)
			{
				continue;
			}

			jsonClass.Add (item.Key, jsonNode);
		}

		return jsonClass;
	}

	private static JSONArray toJSONArray(List<object> list)
	{
		if (list == null)
		{
			return null;
		}

		JSONArray jsonArray = new JSONArray();

		foreach (object item in list)
		{
			if (item == null)
			{
				continue;
			}

			JSONNode jsonNode;

			if (item is List<object>)
			{
				jsonNode = toJSONArray ((List<object>) item);
			}
			else if (item is Dictionary<string, object>)
			{
				jsonNode = toJSONClass ((Dictionary<string, object>) item);
			}
			else
			{
				jsonNode = toJSONValue (item);
			}

			if (jsonNode == null)
			{
				continue;
			}

			jsonArray.Add (jsonNode);
		}

		return jsonArray;
	}

	private static JSONClass toJSONValue(object data)
	{
		if (data == null)
		{
			return null;
		}

		DataType type;

		if (data is int)
		{
			type = DataType.intType;
		}
		else if (data is long)
		{
			type = DataType.longType;
		}
		else if (data is float)
		{
			type = DataType.floatType;
		}
		else if (data is double)
		{
			type = DataType.doubleType;
		}
		else if (data is bool)
		{
			type = DataType.boolType;
		}
		else if (data is string)
		{
			type = DataType.stringType;
		}
		else
		{
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
