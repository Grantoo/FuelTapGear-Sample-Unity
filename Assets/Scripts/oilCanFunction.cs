using UnityEngine;
using System.Collections;

public class oilCanFunction : MonoBehaviour 
{
	private int buttonDebounce = 0;
	
	void Start () 
	{
	}
	
	void Update () 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			buttonDebounce = 0;
		}
		
		Animator _Animator = gameObject.GetComponent<Animator>();
		if (_Animator.GetCurrentAnimatorStateInfo (0).IsName ("oilCanAnim2")) 
		{
			AnimatorStateInfo _stateInfo2 = _Animator.GetCurrentAnimatorStateInfo (0);
			Debug.Log ("Gear Tap - oilCanAnim2 is playing : normalized time = " + _stateInfo2.normalizedTime + "Length" + _stateInfo2.length);

			if(_stateInfo2.normalizedTime > _stateInfo2.length)
			{
				_Animator.Play ("oilCanAnim");
			}
		} 
	}

	void OnMouseOver () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			if(buttonDebounce == 0)
			{
				//GameObject _mainLoop = GameObject.Find("MainLoop");
				//MainLoop _mainloopScript = _mainLoop.GetComponent<MainLoop>();
				//var _gameState = _mainloopScript.mGameState;

				Animator _Animator = gameObject.GetComponent<Animator>();
				_Animator.Play ("oilCanAnim2");

				Debug.Log ("Gear Tap - oilCanAnim2");

			}
		}
	}
}




