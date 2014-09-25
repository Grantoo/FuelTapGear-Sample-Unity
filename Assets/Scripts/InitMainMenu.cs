using UnityEngine;
using System.Collections;

public class InitMainMenu : MonoBehaviour 
{

	public GameObject userCoins;
	
	void Start () 
	{
		Debug.Log ("InitMainMenu!");

		if (PlayerPrefs.HasKey ("userCoins")) 
		{
			Debug.Log ("_getting userCoins");
			var _coins = PlayerPrefs.GetInt("userCoins");

			//set user coins
			userCoins = GameObject.Find ("coinsTextMesh");
			TextMesh t = (TextMesh)userCoins.GetComponent (typeof(TextMesh)); 
			t.text = "Coins:" + _coins.ToString();

		}
		

	}
	
	void Update () 
	{
		
		
	}
}
