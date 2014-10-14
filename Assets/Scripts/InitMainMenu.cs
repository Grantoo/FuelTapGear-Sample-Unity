using UnityEngine;
using System.Collections;

public class InitMainMenu : MonoBehaviour 
{

	public GameObject userCoins;
	
	void Start () 
	{
		Debug.Log ("InitMainMenu!");

		if (PlayerPrefs.HasKey ("userGold")) 
		{
			Debug.Log ("_getting userGold");
			var _gold = PlayerPrefs.GetInt("userGold");

			//set user coins (virtual goods)
			userCoins = GameObject.Find ("goldTextMesh");
			TextMesh t = (TextMesh)userCoins.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _gold.ToString();

		}

		//hide trophy
		GameObject gameObj = GameObject.Find ("Trophy");
		gameObj.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);


		//get Fuel Handler
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();


		//challenge count
		_fuelHandlerScript.SyncChallengeCounts();


		//Tournament Info
		_fuelHandlerScript.SyncTournamentInfo();


		//Virtual Goods
		_fuelHandlerScript.SyncVirtualGoods();


		//try launch fuelSDK
		_fuelHandlerScript.tryLaunchFuelSDK();

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
