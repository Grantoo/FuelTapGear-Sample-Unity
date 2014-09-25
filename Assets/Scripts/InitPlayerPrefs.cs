using UnityEngine;
using System.Collections;

public class InitPlayerPrefs : MonoBehaviour 
{
	
	void Start () 
	{
		Debug.Log ("InitPlayerPrefs!");

		if (PlayerPrefs.HasKey ("userCoins") == false) 
		{
			Debug.Log ("_setting userCoins");
			PlayerPrefs.SetInt("userCoins", 0);
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
