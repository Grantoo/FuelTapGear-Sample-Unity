using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class PropellerListenerExample : PropellerSDKListener
{
	private string m_tournamentID;
	private string m_matchID;
	private PropellerExample m_propellerExample;
	
	public PropellerListenerExample (PropellerExample propellerExample)
	{
		m_tournamentID = "";
		m_matchID = "";
		m_propellerExample = propellerExample;
	}
	
	public string GetTournamentID ()
	{
		return m_tournamentID;
	}
	
	public string GetMatchID ()
	{
		return m_matchID;
	}
	
	public override void SdkCompletedWithMatch (Dictionary<string, string> matchResult)
	{
		string tournamentID = matchResult ["tournamentID"];
		string matchID = matchResult ["matchID"];
		
		Debug.Log ("PropellerExample - SdkCompletedWithMatch - " + tournamentID + " - " + matchID);
		Debug.Log ("Params - " + matchResult ["paramsJSON"]);
		
		// Caching match information for later
		m_tournamentID = tournamentID;
		m_matchID = matchID;
		
		// Calling InvokeMatchDetails() to fake a match result
		m_propellerExample.InvokeMatchDetails ();
	}
	
	public override void SdkCompletedWithExit ()
	{
		Debug.Log ("PropellerExample - SdkCompletedWithExit");
		
		// Clean exit
	}
	
	public override void SdkFailed (string reason)
	{
		Debug.Log ("PropellerExample - SdkFailed - " + reason);
		
		// Handle the failure as necessary
	}	
	
	// Only need to create this override if Android Game Handle Login
	// is enabled in the Propeller SDK prefab
	public override void SdkSocialLogin (bool allowCache)
	{
		Debug.Log ("PropellerExample - SdkSocialLogin - " + allowCache);
		
		// Calling InvokeLoginDetails() to fake a social login
		m_propellerExample.InvokeLoginDetails ();
	}	
	
	// Only need to create this override if Android Game Handle Invite
	// is enabled in the Propeller SDK prefab
	public override void SdkSocialInvite (Dictionary<string, string> inviteDetail)
	{
		StringBuilder stringBuilder = new StringBuilder ();
		
		bool first = true;
		
		foreach (KeyValuePair<string,string> entry in inviteDetail) {
			if (first) {
				first = false;
			} else {
				stringBuilder.Append (", ");
			}
			
			stringBuilder.Append (entry.Key);
			stringBuilder.Append ("=");
			stringBuilder.Append (entry.Value);
		}
		
		Debug.Log ("PropellerExample - SdkSocialInvite - " + stringBuilder.ToString ());
		
		// Calling InvokeInviteCompleted() to fake a social invite
		m_propellerExample.InvokeInviteCompleted ();
	}	
	
	// Only need to create this override if Android Game Handle Share
	// is enabled in the Propeller SDK prefab
	public override void SdkSocialShare (Dictionary<string, string> shareDetail)
	{
		StringBuilder stringBuilder = new StringBuilder ();
		
		bool first = true;
		
		foreach (KeyValuePair<string,string> entry in shareDetail) {
			if (first) {
				first = false;
			} else {
				stringBuilder.Append (", ");
			}
			
			stringBuilder.Append (entry.Key);
			stringBuilder.Append ("=");
			stringBuilder.Append (entry.Value);
		}
		
		Debug.Log ("PropellerExample - SdkSocialShare - " + stringBuilder.ToString ());
		
		// Calling InvokeShareCompleted() to fake a social share
		m_propellerExample.InvokeShareCompleted ();
	}
	
}
