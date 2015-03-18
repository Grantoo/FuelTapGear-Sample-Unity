using UnityEngine;
using System.Collections;

public class InitMainMenu : MonoBehaviour 
{


	void Start () 
	{
		Debug.Log ("InitMainMenu!");

		AddNotificationObservers ();

		//init particles to off
		Debug.Log ("Start Particles!");
		GameObject particleObj = GameObject.Find ("VirtualGoodsParticles");
		ParticleSystem psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		//GAME TOKENS
		Debug.Log ("Start RefreshGameTokenCount!");
		//RefreshGameTokenCount(0);

		//GOLD
		Debug.Log ("Start RefreshGoldCount!");
		RefreshGoldCount(0);

		//DIAMONDS
		Debug.Log ("Start RefreshDiamondCount!");
		RefreshDiamondCount(0);


		//hide challenge count pieces
		GameObject gameObj = GameObject.Find("ccbacking");
		gameObj.renderer.enabled = false;

		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.renderer.enabled = false;

		//hide trophy
		gameObj = GameObject.Find ("Trophy");
		gameObj.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 0f);

		//get Fuel Handler
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

		//get Fuel Dynamics Handler
		//GameObject _fuelDynamicsHandler = GameObject.Find("FuelDynamicsHandlerObject");
		//FuelDynamicsHandler _fuelDynamicsHandlerScript = _fuelDynamicsHandler.GetComponent<FuelDynamicsHandler>();

		//_fuelHandlerScript.setUserConditions();

		_fuelHandlerScript.syncUserValues();

		_fuelHandlerScript.tryRefreshHiScore();

		_fuelHandlerScript.SyncChallengeCounts();

		_fuelHandlerScript.SyncTournamentInfo();

		_fuelHandlerScript.SyncVirtualGoods();

		_fuelHandlerScript.updateLoginText();

		_fuelHandlerScript.tryLaunchFuelSDK();
	}




	public void AddNotificationObservers()
	{
		NotificationCenter.DefaultCenter.AddObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.AddObserver(this, "RefreshChallengeCount");
	}
	public void RemoveNotificationObservers()
	{
		NotificationCenter.DefaultCenter.RemoveObserver(this, "LaunchGamePlay");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshDebugText");
		NotificationCenter.DefaultCenter.RemoveObserver(this, "RefreshChallengeCount");
	}


	
	public void LaunchGamePlay()
	{
		RemoveNotificationObservers ();
		
		Application.LoadLevel("GamePlay");
	}
	


	public void RefreshChallengeCount(Hashtable ccTable)
	{
		// retrieve a value for the given key
		int ccount = (int)ccTable["cc"];    

		//show challenge count pieces and set count values
		GameObject gameObj = GameObject.Find("ccbacking");
		gameObj.renderer.enabled = true;

		GameObject ccountObj = GameObject.Find ("ChallengeCount");
		TextMesh tmesh = (TextMesh)ccountObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = ccount.ToString();
		tmesh.renderer.enabled = true;
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


	public void RefreshGameTokenCount(int addAmount)
	{
		//GAME TOKEN
		if (PlayerPrefs.HasKey ("gameTokens")) 
		{
			Debug.Log ("_getting gameTokens");
			var _tokens = PlayerPrefs.GetInt("gameTokens");
			
			if(addAmount > 0)
			{
				_tokens += addAmount;
				PlayerPrefs.SetInt("gameTokens", _tokens);
			}
			
			GameObject gameObj = GameObject.Find ("gameTokenTextMesh");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _tokens.ToString();
		}
	}


	public void RefreshGoldCount(int addAmount)
	{
		//GOLD
		if (PlayerPrefs.HasKey ("userGold")) 
		{
			Debug.Log ("_getting userGold");
			var _gold = PlayerPrefs.GetInt("userGold");

			if(addAmount > 0)
			{
				_gold += addAmount;
				PlayerPrefs.SetInt("userGold", _gold);
			}
			
			GameObject gameObj = GameObject.Find ("goldTextMesh");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _gold.ToString();
		}
	}

	public void RefreshDiamondCount(int addAmount)
	{
		//GOLD
		if (PlayerPrefs.HasKey ("diamonds")) 
		{
			Debug.Log ("_getting userGold");
			var _diamonds = PlayerPrefs.GetInt("diamonds");
			
			if(addAmount > 0)
			{
				_diamonds += addAmount;
				PlayerPrefs.SetInt("diamonds", _diamonds);
			}
			
			GameObject gameObj = GameObject.Find ("diamondTextMesh");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = "x" + _diamonds.ToString();
		}
	}

	public void RefreshHiScore(int score)
	{
		//SCORE
		if (PlayerPrefs.HasKey ("hiScore")) 
		{
			Debug.Log ("_getting hiScore");
			var _score = PlayerPrefs.GetInt ("hiScore");
			
			if (score > _score) 
			{
				_score = score;
				PlayerPrefs.SetInt ("hiScore", _score);
			}
			
			GameObject gameObj = GameObject.Find ("HiScore");
			TextMesh t = (TextMesh)gameObj.GetComponent (typeof(TextMesh)); 
			t.text = _score.ToString ();
				
		}
	}

	public void VirtualGoodsFanFare()
	{
		GameObject gameObj = GameObject.Find ("VirtualGoodsParticles");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 

		psystem.Play();
	}


	public void RefreshDebugText()
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
		
		int _gearShapeType = _fuelHandlerScript.GearShapeType;
		float _gearFriction = _fuelHandlerScript.GearFriction;

		GameObject textMesh = GameObject.Find ("DebugText1");
		TextMesh tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 

		tmesh.text = "friction = " + _gearFriction.ToString();
		tmesh.renderer.enabled = true;

		textMesh = GameObject.Find ("DebugText2");
		tmesh = (TextMesh)textMesh.GetComponent (typeof(TextMesh)); 
		
		tmesh.text = "geartype = " + _gearShapeType.ToString();
		tmesh.renderer.enabled = true;
	}


	void Update () 
	{

	}
}
