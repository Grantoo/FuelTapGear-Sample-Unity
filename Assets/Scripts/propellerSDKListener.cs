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
	
	//public int m_matchStatus;
	//public string m_tournamentID;
	//public string m_matchID;
	//public int mMatchRound;





	
	public override void SdkSocialLogin (bool allowCache)
	{
		Debug.Log ("SdkSocialLogin: handle social login");

		// handle social login
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();
		
		_mainmenuScript.trySocialLogin();
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
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();
		_mainmenuScript.onSocialInviteClicked (null); 
		
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
		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();
		_mainmenuScript.onSocialShareClicked (null); 

	}
	
	public override void SdkCompletedWithExit ()
	{
		// sdk completed gracefully with no further action
		Debug.Log ("SdkCompletedWithExit");
	}
	
	public override void SdkCompletedWithMatch (Dictionary<string, string> matchResult)
	{
		// sdk completed with a match

		

		// extract the params data
		string paramsJSON = matchResult ["paramsJSON"];
		JSONNode json = JSONNode.Parse (paramsJSON);
		
		// extract the match seed value
		long seed = 0;
		
		// must parse long values manually since SimpleJSON
		// doesn't yet provide this function automatically
		if (!long.TryParse(json ["seed"], out seed))
		{
			// invalid string encoded long value, defaults to 0
		}
		
		// extract the match round value
		int round = json ["round"].AsInt;

		// extract the ads allowed flag
		bool adsAllowed = json ["adsAllowed"].AsBool;
		
		// extract the fair play flag
		bool fairPlay = json ["fairPlay"].AsBool;
		
		// extract the options data
		JSONClass options = json ["options"].AsObject;
		
		// extract the player's public profile data
		JSONClass you = json ["you"].AsObject;
		string yourNickname = you ["name"];
		string yourAvatarURL = you ["avatar"];
		
		// extract the opponent's public profile data
		JSONClass them = json ["them"].AsObject;
		string theirNickname = them ["name"];
		string theirAvatarURL = them ["avatar"];
		


		/*
		Debug.Log (	"__SdkCompletedWithMatch__" + "\n" +
					"m_matchStatus = " + m_matchStatus + "\n" +
		           	"m_tournamentID = " + m_tournamentID + "\n" +
		           	"m_matchID = " + m_matchID + "\n" +
		            "round = " + round + "\n" +
		            "adsAllowed = " + adsAllowed + "\n" +
		            "fairPlay = " + fairPlay + "\n" +
		            "yourNickname = " + yourNickname + "\n" +
		            "yourAvatarURL = " + yourAvatarURL + "\n" +
		            "theirNickname = " + theirNickname + "\n" +
		            "theirAvatarURL = " + theirAvatarURL + "\n" 
		           );
		
		// play the game for the given match data
		//startGame();

		*/

		GameObject _mainmenu = GameObject.Find("MainMenuFuelFB");
		mainmenuFuelFB _mainmenuScript = _mainmenu.GetComponent<mainmenuFuelFB>();

		_mainmenuScript.LaunchMultiplayerGame(matchResult); 



		
	}
	
	public override void SdkFailed (string reason)
	{
		// sdk has failed with an unrecoverable error
		
		Debug.Log ("SdkFailed" + reason);
		
	}
}



