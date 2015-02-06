using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Facebook.MiniJSON;
using SimpleJSON;

public class InitPlayerPrefs : MonoBehaviour 
{
	
	void Start () 
	{
		Debug.Log ("InitPlayerPrefs!");

		if (PlayerPrefs.HasKey ("installTimeStamp") == false) 
		{
			TimeSpan span = DateTime.Now.Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			int _seconds = (int)span.TotalSeconds;
			Debug.Log ("_setting installTimeStamp");
			PlayerPrefs.SetInt("installTimeStamp", _seconds);
		}

		if (PlayerPrefs.HasKey ("numLaunches") == false) 
		{
			Debug.Log ("_setting numLaunches");
			PlayerPrefs.SetInt("numLaunches", 1);
		}

		if (PlayerPrefs.HasKey ("numSessions") == false) 
		{
			Debug.Log ("_setting numSessions");
			PlayerPrefs.SetInt("numSessions", 0);
		}

		if (PlayerPrefs.HasKey ("gameTokens") == false) 
		{
			Debug.Log ("_setting gameTokens");
			PlayerPrefs.SetInt("gameTokens", 0);
		}

		if (PlayerPrefs.HasKey ("userGold") == false) 
		{
			Debug.Log ("_setting userGold");
			PlayerPrefs.SetInt("userGold", 0);
		}

		if (PlayerPrefs.HasKey ("diamonds") == false) 
		{
			Debug.Log ("_setting diamonds");
			PlayerPrefs.SetInt("diamonds", 0);
		}

		if (PlayerPrefs.HasKey ("hiScore") == false) 
		{
			Debug.Log ("_setting hiScore");
			PlayerPrefs.SetInt("hiScore", 0);
		}

	}
	
	void Update () 
	{
			
	}
}
