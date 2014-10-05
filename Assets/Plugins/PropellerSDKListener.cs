using UnityEngine;
using System.Collections.Generic;

public abstract class PropellerSDKListener
{
	public abstract void SdkCompletedWithExit ();
	
	public abstract void SdkCompletedWithMatch (Dictionary<string, string> matchInfo);
	
	public abstract void SdkFailed (string reason);
	
	public virtual void SdkSocialLogin (bool allowCache)
	{
		// do nothing
		Debug.Log ("SdkSocialLogin default");
	}

	public virtual void SdkSocialInvite (Dictionary<string, string> inviteDetail)
	{
		// do nothing
		Debug.Log ("SdkSocialInvite default");
	}

	public virtual void SdkSocialShare (Dictionary<string, string> shareDetail)
	{
		// do nothing
		Debug.Log ("SdkSocialShare default");
	}
}
