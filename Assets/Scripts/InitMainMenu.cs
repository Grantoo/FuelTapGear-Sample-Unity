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
			t.text = "x" + _coins.ToString();

		}

		//get Fuel Handler
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();


		//challenge count
		_mainmenuScript.SyncChallengeCounts();


		//Tournament Info
		_mainmenuScript.SyncTournamentInfo();


		//Virtual Goods
		_mainmenuScript.SyncVirtualGoods();


		//try launch fuelSDK
		_mainmenuScript.tryLaunchFuelSDK();

	}

	public void RefreshChallengeCount(int ccount)
	{
		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = ccount.ToString();
	}

	public void RefreshTournamentInfo(bool enabled, string tournamentName, int timeRemaining)
	{
		GameObject gameObj = GameObject.Find ("Trophy");

		if(enabled == false)
		{
			gameObj.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);
		}
		else
		{
			gameObj.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
		}


	}

	void Update () 
	{
			
	}
}
