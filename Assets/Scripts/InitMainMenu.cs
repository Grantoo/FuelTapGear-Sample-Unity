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

			//set user coins (virtual goods)
			userCoins = GameObject.Find ("goldTextMesh");
			TextMesh t = (TextMesh)userCoins.GetComponent (typeof(TextMesh)); 
			t.text = "Gold:" + _coins.ToString();

		}

		//get controller object
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();


		//challenge count
		GameObject ccount = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccount.GetComponent (typeof(TextMesh)); 

		int _challengeCount = _mainmenuScript.getChallengeCount();
		tmesh.text = "Challenge Count:" + _challengeCount.ToString();


		//try launch fuelSDK
		_mainmenuScript.tryLaunchFuelSDK();

		

	}
	
	void Update () 
	{
		
		
	}
}
