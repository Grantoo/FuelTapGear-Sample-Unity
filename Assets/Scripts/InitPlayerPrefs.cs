using UnityEngine;
using System.Collections;

public class InitPlayerPrefs : MonoBehaviour 
{
	
	void Start () 
	{
		Debug.Log ("InitPlayerPrefs!");

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
