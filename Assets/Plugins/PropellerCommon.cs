using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PropellerSDKSimpleJSON;


public class PropellerCommon : MonoBehaviour
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

	private static GameObject m_hostGameObject;
	private static bool m_bInitialized;

	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern bool iOSSetUserConditions(string conditions);
	[DllImport ("__Internal")]
	public static extern bool iOSSyncUserValues();
	#endif
	
	static PropellerCommon ()
	{
		m_bInitialized = false;
	}


	public static void SetHostGameObject (GameObject hostGameObject)
	{
		m_hostGameObject = hostGameObject;
	}


	private void PropellerOnUserValues (string message)
	{
		if (string.IsNullOrEmpty (message)) {
			Debug.Log ("PropellerOnUserValues - null or empty message");
			return;
		}
		
		const char kDelimeter = '&';
		string[] resultsArray = message.Split (kDelimeter);
		
		if (resultsArray.Length == 0 || resultsArray.Length%2 == 1) {
			Debug.LogError ("PropellerOnUserValues - Invalid response from PropellerUnitySDK");
			return;
		}
		
		Dictionary<string, string> userValuesInfo = new Dictionary<string, string> ();
		for(int i = 0; i < resultsArray.Length; i+=2) {
			userValuesInfo.Add (resultsArray[i], resultsArray[i+1]);	
		}
		
		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnUserValues - undefined host game object");
			return;
		}


		Debug.Log ("PropellerOnUserValues #-# coming from Propeller Common!");

		m_hostGameObject.SendMessage("OnPropellerSDKUserValues", userValuesInfo);
	}
	
	
	private void Awake ()
	{
		if (!m_bInitialized) {
			GameObject.DontDestroyOnLoad (gameObject);
			m_bInitialized = true;
		} else {
			GameObject.Destroy (gameObject);	
		}
	}



	#region private utility methods
	public static JSONClass toJSONClass (Dictionary<string, object> dictionary)
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
	
	public static JSONArray toJSONArray (List<object> list)
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
	
	public static JSONClass toJSONValue (object data)
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


