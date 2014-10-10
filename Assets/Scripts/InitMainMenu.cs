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
		_mainmenuScript.SyncChallengeCounts();


		//try launch fuelSDK
		_mainmenuScript.tryLaunchFuelSDK();


	}

	public void RefreshChallengeCount(int ccount)
	{
		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = ccount.ToString();
	}
	
	void Update () 
	{
		
		
	}
}
