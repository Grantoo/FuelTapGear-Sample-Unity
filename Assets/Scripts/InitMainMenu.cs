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
			userCoins = GameObject.Find ("goldTextMesh");
			TextMesh t = (TextMesh)userCoins.GetComponent (typeof(TextMesh)); 
			t.text = "Gold:" + _coins.ToString();

		}


		//try launch fuelSDK
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();
		
		_mainmenuScript.tryLaunchFuelSDK();

		

	}
	
	void Update () 
	{
		
		
	}
}
