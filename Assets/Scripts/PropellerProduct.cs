using UnityEngine;
using System.Collections;
using System;

public class PropellerProduct : MonoBehaviour 
{
	public bool mDynamicsOnly = false;

	public static PropellerProduct Instance { get; private set; }


	/*
	 -----------------------------------------------------
							Awake
	 -----------------------------------------------------
	*/
	void Awake ()
	{
		Debug.Log ("PropellerProduct Awake");
		
		if (Instance != null && Instance != this) 
		{
			//destroy other instances
			Destroy (gameObject);
		} 
		else if( Instance == null )
		{
		}
		
		Instance = this;
		
		DontDestroyOnLoad(gameObject);
	}


	void Start () 
	{
		Debug.Log ("PropellerProduct Start!");
	}


	public bool isDynamicsOnly () 
	{
		return mDynamicsOnly;
	}


	public void syncUserValues () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			_dynamicsHandlerScript.syncUserValues();
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.syncUserValues();
		}
	}


	public void SyncChallengeCounts () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.SyncChallengeCounts();
		}
	}


	public void SyncTournamentInfo () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.SyncTournamentInfo();
		}
	}


	public void SyncVirtualGoods () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.SyncVirtualGoods();
		}
	}


	public void updateLoginText () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.updateLoginText();
		}
	}


	public void tryLaunchFuelSDK () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.tryLaunchFuelSDK();
		}
	}


	public GameMatchData getMatchData () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			return(_dynamicsHandlerScript.getMatchData());
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.getMatchData());
		}
	}


	public float getGameTime () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			return(_dynamicsHandlerScript.GameTime);
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.GameTime);
		}
	}

	public int getShowDebug () 
	{
		if (mDynamicsOnly == false) 
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.ShowDebug);
		}

		return 0;
	}

	public int getGearShapeType () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			return(_dynamicsHandlerScript.GearShapeType);
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.GearShapeType);
		}
	}


	public float getGearFriction () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			return(_dynamicsHandlerScript.GearFriction);
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.GearFriction);
		}
	}


	public string getSplit1Name () 
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			return(_dynamicsHandlerScript.Split1Name);
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			return(_fuelHandlerScript.Split1Name);
		}
	}

	
	public void SetMatchScore(int scoreValue, int speedValue)
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			_dynamicsHandlerScript.SetMatchScore(scoreValue, speedValue);
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.SetMatchScore(scoreValue, speedValue);
		}
	}


	public void launchSinglePlayerGame()
	{
		if (mDynamicsOnly == true) 
		{
			DynamicsHandler _dynamicsHandlerScript = getDynamicsHandlerClass();
			_dynamicsHandlerScript.launchSinglePlayerGame();
		} 
		else
		{
			FuelHandler _fuelHandlerScript = getFuelHandlerClass();
			_fuelHandlerScript.launchSinglePlayerGame();
		}
	}



	private FuelHandler getFuelHandlerClass()
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		if (_fuelHandler != null) {
			FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler> ();
			if(_fuelHandlerScript != null) {
				return _fuelHandlerScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}
	
	private DynamicsHandler getDynamicsHandlerClass()
	{
		GameObject _dynamicsHandler = GameObject.Find("DynamicsHandlerObject");
		if (_dynamicsHandler != null) {
			DynamicsHandler _dynamicsHandlerScript = _dynamicsHandler.GetComponent<DynamicsHandler> ();
			if(_dynamicsHandlerScript != null) {
				return _dynamicsHandlerScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}
	

}
