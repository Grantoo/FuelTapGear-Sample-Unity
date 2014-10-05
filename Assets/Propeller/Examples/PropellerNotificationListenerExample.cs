using UnityEngine;

public class PropellerNotificationListenerExample : PropellerSDKNotificationListener
{
	private PropellerExample m_propellerExample;
	
	public PropellerNotificationListenerExample (PropellerExample propellerExample)
	{
		m_propellerExample = propellerExample;
	}
	
	public override void SdkOnNotificationEnabled (PropellerSDK.NotificationType type) {
		Debug.Log("SdkOnNotificationEnabled - start");
		
		m_propellerExample.UpdateNotificationStatus(type);
		
		Debug.Log("SdkOnNotificationEnabled - end");
	}

	public override void SdkOnNotificationDisabled (PropellerSDK.NotificationType type) {
		Debug.Log("SdkOnNotificationDisabled - start");
		
		m_propellerExample.UpdateNotificationStatus(type);
		
		Debug.Log("SdkOnNotificationDisabled - end");
	}

}
