using UnityEngine;

public class PropellerNotificationListenerExample : PropellerSDKNotificationListener
{
	private PropellerExample m_propellerExample;
	
	public PropellerNotificationListenerExample (PropellerExample propellerExample)
	{
		m_propellerExample = propellerExample;
	}
	
	public override void SdkOnNotificationEnabled (PropellerSDK.NotificationType type)
	{
		m_propellerExample.UpdateDialog ("SdkOnNotificationEnabled() callback called...");
		m_propellerExample.UpdateDialog ("Notification type: " + type);
		m_propellerExample.UpdateDialog ("Done!");
		m_propellerExample.ShowDialog ();
		
		m_propellerExample.UpdateNotificationStatus (type);
	}
	
	public override void SdkOnNotificationDisabled (PropellerSDK.NotificationType type)
	{
		m_propellerExample.UpdateDialog ("SdkOnNotificationDisabled() callback called...");
		m_propellerExample.UpdateDialog ("Notification type: " + type);
		m_propellerExample.UpdateDialog ("Done!");
		m_propellerExample.ShowDialog ();
		
		m_propellerExample.UpdateNotificationStatus (type);
	}
	
}
