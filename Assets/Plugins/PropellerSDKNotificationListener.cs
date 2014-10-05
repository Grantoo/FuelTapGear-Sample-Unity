using UnityEngine;

public abstract class PropellerSDKNotificationListener
{

	public abstract void SdkOnNotificationEnabled (PropellerSDK.NotificationType type);

	public abstract void SdkOnNotificationDisabled (PropellerSDK.NotificationType type);

}

