using UnityEngine;
using System.Collections;

public class gearAction : MonoBehaviour 
{
	public Sprite img0, img1, img2, img3, img4, img5;

	public AudioSource bonusSound1, bonusSound2, bonusSound3, bonusSound4, bonusSound5;


	public GameObject spinTextObj;

	public int buttonDebounce = 0;
	public float minTapDelta = 0.5f;
	public float spinvelocity = 0.0f;
	public float maxspinvelocity = 0.0f;
	public string velocityStr = "0";
	private float angle = 0.0f;
	private float friction = 0.995f;

	private int addBonusTap;
	private bool add5sec;

	public enum eBonusState 
	{
		BaseLevel,
		Level1,
		Level2, 
		Level3, 
		Level4,
		Level5,
		MaxedOut
	};

	public eBonusState mBonusState = eBonusState.BaseLevel;

	public void SetFriction (float f) 
	{
		friction = f;
	}

	public void updateSpinText (string str) 
	{
		spinTextObj = GameObject.Find ("speedTextMesh");
		TextMesh tmesh = (TextMesh)spinTextObj.GetComponent (typeof(TextMesh)); 
		tmesh.text = str;
	}

	public void Reset (int _gearType, float _friction) 
	{
		spinvelocity = 0.0f;
		maxspinvelocity = 0.0f;
		angle = 0.0f;
		friction = _friction;

		GameObject shadow = GameObject.Find ("GearShadow");

		Debug.Log ("________________________________________________gearAction:Reset - friction = " + friction + ", geartype = " + _gearType);


		switch (_gearType) 
		{
			case 0:
				gameObject.GetComponent<SpriteRenderer>().sprite = img0;
				shadow.GetComponent<SpriteRenderer>().sprite = img0;
			break;
			case 1:
				gameObject.GetComponent<SpriteRenderer>().sprite = img1;
				shadow.GetComponent<SpriteRenderer>().sprite = img1;
			break;
			case 2:
				gameObject.GetComponent<SpriteRenderer>().sprite = img2;
				shadow.GetComponent<SpriteRenderer>().sprite = img2;
			break;
			case 3:
				gameObject.GetComponent<SpriteRenderer>().sprite = img3;
				shadow.GetComponent<SpriteRenderer>().sprite = img3;
			break;
			case 4:
				gameObject.GetComponent<SpriteRenderer>().sprite = img4;
				shadow.GetComponent<SpriteRenderer>().sprite = img4;
			break;
			case 5:
				gameObject.GetComponent<SpriteRenderer>().sprite = img5;
				shadow.GetComponent<SpriteRenderer>().sprite = img5;
			break;
		}
	}

	void Start () 
	{

		//GameObject MultTimesTwo = GameObject.Find ("MultTimesTwo");
		//GameObject bonus = (GameObject) Instantiate(MultTimesTwo, transform.position, transform.rotation);


		ClearActiveBonuses ();
	}
	
	void Update () 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			buttonDebounce = 0;
		}

		angle -= spinvelocity;
		transform.rotation = Quaternion.Euler(0, 0, angle);

		GameObject shadow = GameObject.Find ("GearShadow");
		shadow.transform.rotation = Quaternion.Euler(0, 0, angle);

		velocityStr = maxspinvelocity.ToString ("0.00");


		spinvelocity *= friction;


		UpdateBackGroundGears ();

		//bonues
		switch (mBonusState) 
		{

			case eBonusState.BaseLevel:
			{
				if(spinvelocity >= 6.0f)
				{
					bonusSound1.Play();

					SpeedFanFareLevel1();

					mBonusState = eBonusState.Level1;
				}
			}
			break;

			case eBonusState.Level1:
			{
				if(spinvelocity >= 10.0f)
				{
					bonusSound2.Play();

					SpeedFanFareLevelTimes2();
					addBonusTap = 1;

					mBonusState = eBonusState.Level2;
				}
			}
			break;

			case eBonusState.Level2:
			{
				if(spinvelocity >= 14.0f)
				{
					bonusSound3.Play();

					friction += 0.005f;
					SpeedFanFareLevelFminus1();

					mBonusState = eBonusState.Level3;
				}
			}
			break;

			case eBonusState.Level3:
			{
				if(spinvelocity >= 20.0f)
				{
					bonusSound4.Play();

					SpeedFanFareLevelTimes3();
					addBonusTap = 2;

					mBonusState = eBonusState.Level4;
				}
			}
			break;
			
			case eBonusState.Level4:
			{
				if(spinvelocity >= 26.0f)
				{
					bonusSound5.Play();

					SpeedFanFareLevelPlus5();
					add5sec = true;

					mBonusState = eBonusState.Level5;
				}
			}
			break;

			case eBonusState.Level5:
			{
				if(spinvelocity >= 32.0f)
				{
					bonusSound1.Play();

					mBonusState = eBonusState.MaxedOut;
				}
			}
			break;

			case eBonusState.MaxedOut:
			break;
		}



	}

	void UpdateBackGroundGears () 
	{
		GameObject gear = GameObject.Find ("OrangeGear");
		gearMount _gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmBlueGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmBlueGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmOrangeGear");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);
		
		gear = GameObject.Find ("SmOrangeGear2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("SmOrangeGearTrans");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGearTrans");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

		gear = GameObject.Find ("BlueGearTrans2");
		_gearMountScript = gear.GetComponent<gearMount>();
		_gearMountScript.SetGameVelocity (spinvelocity);

	}


	void OnMouseOver () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			if(buttonDebounce == 0)
			{
				GameObject _mainLoop = GameObject.Find("MainLoop");
				MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();
			
				var _gameState = _mainloopScript.mGameState;


				if(_gameState == MainLoop.eGameState.Ready)
				{
					//first tap, tell mainloop to start
					_mainloopScript.mGameState = MainLoop.eGameState.FirstTap;
					_gameState = _mainloopScript.mGameState;

					_mainloopScript.setStartButtonText("Tap! Fast, fast, faster!!");
				}

				if(_gameState == MainLoop.eGameState.Running)
				{
					_mainloopScript.scoreValue += (1 + addBonusTap);
			
					buttonDebounce = 1;

					//increase spin speed
					var currentTime = Time.deltaTime;
					var increase = minTapDelta - currentTime;

					if(currentTime < minTapDelta)
					{
						increase = minTapDelta - currentTime;
					}
					else
					{
						increase = 0.0f;
					}

					spinvelocity += increase;

					if(spinvelocity > maxspinvelocity)
					{
						maxspinvelocity = spinvelocity;
					}

					//Debug.Log ("Gear Tap - spinvelocity = " + spinvelocity);
				}
			}
		}
	}





	public void SpeedFanFareLevel1()
	{
		GameObject gameObj = GameObject.Find ("GearBonusParticlesLevel1");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		
		psystem.Play();
	}

	public void SpeedFanFareLevelTimes2()
	{
		GameObject gameObj = GameObject.Find ("GearBonusParticlesLevel2");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		
		psystem.Play();
	}

	public void SpeedFanFareLevelTimes3()
	{
		GameObject gameObj = GameObject.Find ("GearBonusParticlesLevel3");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		
		psystem.Play();

		gameObj = GameObject.Find ("GearBonusParticlesLevel2");
		psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

	}

	public void SpeedFanFareLevelFminus1()
	{
		GameObject gameObj = GameObject.Find ("GearBonusParticlesLevelF1");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		
		psystem.Play();
	}

	public void SpeedFanFareLevelPlus5()
	{
		GameObject gameObj = GameObject.Find ("GearBonusParticlesLevelPlus5");
		ParticleSystem psystem = (ParticleSystem)gameObj.GetComponent (typeof(ParticleSystem)); 
		
		psystem.Play();
	}

	public void ClearActiveBonuses()
	{
		GameObject particleObj = GameObject.Find ("GearBonusParticlesLevel1");
		ParticleSystem psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		particleObj = GameObject.Find ("GearBonusParticlesLevel2");
		psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		particleObj = GameObject.Find ("GearBonusParticlesLevel3");
		psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		particleObj = GameObject.Find ("GearBonusParticlesLevelF1");
		psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();

		particleObj = GameObject.Find ("GearBonusParticlesLevelPlus5");
		psystem = (ParticleSystem)particleObj.GetComponent (typeof(ParticleSystem)); 
		psystem.Stop();


		addBonusTap = 0;
		add5sec = false;

	}

	public bool Check5secBonus()
	{
		bool result = false;

		if (add5sec == true) 
		{
			add5sec = false;
			result = true;
		}


		return result;
	}



}




