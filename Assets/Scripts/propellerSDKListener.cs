using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;


/*
	-----------------------------------------------------
					fuelSDKListener
	-----------------------------------------------------
*/

public class fuelSDKListener : PropellerSDKListener
{

	public override void SdkSocialLogin (bool allowCache)
	{
		Debug.Log ("SdkSocialLogin: handle social login");

		// handle social login
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

		_fuelHandlerScript.trySocialLogin(allowCache);
	}
	
	
	public override void SdkSocialInvite (Dictionary<string, string> inviteDetail)
	{
		Debug.Log ("SdkSocialInvite: handle social invite");
		
		string subject = inviteDetail["subject"];
		string longMessage = inviteDetail["long"];
		string shortMessage = inviteDetail["short"];
		string linkUrl = inviteDetail["link"];
		string pictureUrl = inviteDetail["picture"];
		
		Debug.Log ("subject = " + subject + "\n" +
		           "longMessage = " + longMessage + "\n" +
		           "shortMessage = " + shortMessage + "\n" +
		           "linkUrl = " + linkUrl + "\n" +
		           "pictureUrl = " + pictureUrl + "\n");
		
		// handle social invite
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
		_fuelHandlerScript.onSocialInviteClicked (inviteDetail); 
	}
	
	public override void SdkSocialShare (Dictionary<string, string> shareDetail)
	{
		// handle social share
		Debug.Log ("shareDetail" + shareDetail);

		string subject = shareDetail["subject"];
		string longMessage = shareDetail["long"];
		string shortMessage = shareDetail["short"];
		string linkUrl = shareDetail["link"];
		string pictureUrl = shareDetail["picture"];

		Debug.Log ("subject = " + subject + "\n" +
		           "longMessage = " + longMessage + "\n" +
		           "shortMessage = " + shortMessage + "\n" +
		           "linkUrl = " + linkUrl + "\n" +
		           "pictureUrl = " + pictureUrl + "\n");

		// handle social invite
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();
		_fuelHandlerScript.onSocialShareClicked (shareDetail); 
	}
	
	public override void SdkCompletedWithExit ()
	{
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "SubTransOverlay");

		if (InitMainMenu.sComingFromGame == true) {

			NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "UpdatePropellerSDK");
			InitMainMenu.sComingFromGame = false;
		}

		Debug.Log ("SdkCompletedWithExit");
	}
	
	public override void SdkCompletedWithMatch (Dictionary<string, string> matchResult)
	{
		GameObject _fuelHandler = GameObject.Find("FuelHandlerObject");
		FuelHandler _fuelHandlerScript = _fuelHandler.GetComponent<FuelHandler>();

		_fuelHandlerScript.LaunchMultiplayerGame(matchResult); 
	}
	
	public override void SdkFailed (string reason)
	{
		// sdk has failed with an unrecoverable error
		NotificationCenter.DefaultCenter.PostNotification (getMainMenuClass(), "SubTransOverlay");
		Debug.Log ("SdkFailed" + reason);
	}


	/*
	 -----------------------------------------------------
			Access to mainmenu this pointer
	 -----------------------------------------------------
	*/
	private InitMainMenu getMainMenuClass()
	{
		GameObject _mainmenu = GameObject.Find("InitMainMenu");
		if (_mainmenu != null) {
			InitMainMenu _mainmenuScript = _mainmenu.GetComponent<InitMainMenu> ();
			if(_mainmenuScript != null) {
				return _mainmenuScript;
			}
			throw new Exception();
		}
		throw new Exception();
	}

}



