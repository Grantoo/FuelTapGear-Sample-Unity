using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class PropellerCommon : MonoBehaviour
{

	private static GameObject m_hostGameObject;
	private static bool m_bInitialized;

	#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern bool iOSSetUserConditions(string conditions);
	[DllImport ("__Internal")]
	public static extern void iOSSyncUserValues();
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
		
		Dictionary<string, object> userValuesInfo = new Dictionary<string, object> ();
		for(int i = 0; i < resultsArray.Length; i+=2) {
			userValuesInfo.Add (resultsArray[i], resultsArray[i+1]);	
		}
		
		if (m_hostGameObject == null) {
			Debug.Log ("PropellerOnUserValues - undefined host game object");
			return;
		}
		
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

	
}


