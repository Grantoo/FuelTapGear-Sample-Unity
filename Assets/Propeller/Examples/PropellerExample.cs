using System.Collections.Generic;
using UnityEngine;

public class PropellerExample : MonoBehaviour
{
	private bool m_bInitialized;
	private PropellerSDKListener m_listener;
    private string m_virtualGoodsTransactionId;

	// language code switching
	private string languageCodeText;

	// notification toggling
    private string pushNotificationStatusText;
    private string localNotificationStatusText;

	private void OnGUI ()
	{
		const float kButtonWidth = 320f;
		const float kNumButtons = 5f;
		float kButtonX = (float)Screen.width * 0.5f - kButtonWidth * 0.5f;
		float kButtonHeight = (float)Screen.height * (1f / (kNumButtons + 1f));
		float kSpacing = kButtonHeight / (kNumButtons + 1f);
		
		if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 0f), kButtonWidth, kButtonHeight), "Launch")) {
			PropellerSDK.Launch (m_listener);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 1f), kButtonWidth, kButtonHeight), "Test Login")) {
			InvokeLoginDetails ();
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 2f), kButtonWidth, kButtonHeight), "SyncChallengeCounts")) {
			PropellerSDK.SyncChallengeCounts ();
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 3f), kButtonWidth, kButtonHeight), "SyncTournamentInfo")) {
			PropellerSDK.SyncTournamentInfo ();
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 4f), kButtonWidth, kButtonHeight), "SyncVirtualGoods")) {
			PropellerSDK.SyncVirtualGoods ();
		}

		// language code switching
		// - uncomment to test
		// - update the number of buttons (kNumButtons) to include the ones used for
		//   the language code switching
		// - make sure the button offsets are correct in the method or else the buttons
		//   and labels for language code switching will not render correctly
//		OnGUILanguageCodeSwitching (kButtonX, kSpacing, kButtonWidth, kButtonHeight);

		// notification toggling
		// - uncomment to test
		// - don't forget to uncomment the Awake() call
		// - update the number of buttons (kNumButtons) to include the ones used for
		//   the notification toggling UI
		// - make sure the button offsets are correct in the method or else the buttons
		//   and labels for notification toggling will not render correctly
//		OnGUINotificationToggling (kButtonX, kSpacing, kButtonWidth, kButtonHeight);
	}
	
	public void OnPropellerSDKChallengeCountUpdated (string count)
	{
		Debug.Log ("PropellerExample - OnPropellerSDKChallengeCountUpdated - " + count);

		// Update the UI with the count
	}

	public void OnPropellerSDKTournamentInfo (Dictionary<string, string> tournamentInfo)
	{
		DisplayTournamentInfo("OnPropellerSDKTournamentInfo", tournamentInfo);
		
		// Update the UI with the tournament information
	}

    public void OnPropellerSDKVirtualGoodList (Dictionary<string, object> virtualGoodInfo)
    {
        string transactionId = (string) virtualGoodInfo["transactionID"];
        List<string> virtualGoods = (List<string>) virtualGoodInfo["virtualGoods"];

        string virtualGoodList = string.Join(" - ", virtualGoods.ToArray());

        Debug.Log ("PropellerExample - OnPropellerSDKVirtualGoodList - " + transactionId + " - " + virtualGoodList);

        // Store the virtual goods for consumption

        m_virtualGoodsTransactionId = transactionId;

        InvokeAcknowledgeVirtualGoods ();
    }

    public void OnPropellerSDKVirtualGoodRollback (string transactionId)
    {
		Debug.Log ("PropellerExample - OnPropellerSDKVirtualGoodRollback - " + transactionId);

        // Rollback the virtual good transaction for the given transaction ID
    }

	private void Awake ()
	{
		if (!m_bInitialized) {
			GameObject.DontDestroyOnLoad (gameObject);

			if (!Application.isEditor) {
				m_listener = new PropellerListenerExample (this);

				// notification toggling
				// - uncomment to test
				// - don't forget to uncomment the OnGUI() call
//				initNotificationToggling ();
			}

			m_bInitialized = true;
		} else {
			GameObject.Destroy (gameObject);
		}
	}

	public void InvokeMatchDetails ()
	{
		Debug.Log ("PropellerExample - InvokeMatchDetails");
		Invoke ("SendMatchDetails", 0.5f);
	}

	private void SendMatchDetails ()
	{
		Debug.Log ("PropellerExample - SendMatchDetails");
		
		PropellerListenerExample listener = (PropellerListenerExample)m_listener;

		Dictionary<string, object> matchResult = new Dictionary<string, object> ();
		matchResult.Add ("matchID", listener.GetMatchID ());
		matchResult.Add ("tournamentID", listener.GetTournamentID ());
		matchResult.Add ("score", "55");

		PropellerSDK.LaunchWithMatchResult (matchResult, m_listener);
	}
	
	public void InvokeLoginDetails ()
	{
		Debug.Log ("PropellerExample - InvokeLoginDetails");
		Invoke ("SendLoginDetails", 0.5f);
	}
	
	private void SendLoginDetails ()
	{
		Debug.Log ("PropellerExample - SendingLoginDetails");

		Dictionary<string, string> loginInfo = new Dictionary<string, string> ();
		loginInfo.Add ("provider", "facebook");
		loginInfo.Add ("email", "testguy@grantoo.org");
		loginInfo.Add ("id", "testguyid");
		loginInfo.Add ("nickname", "testguy445");
		loginInfo.Add ("token", "testguy445");

		PropellerSDK.SdkSocialLoginCompleted (loginInfo);

		// Return null in the case of a failure
		//PropellerSDK.SdkSocialLoginCompleted(null);
	}

	public void InvokeInviteCompleted ()
	{
		Debug.Log ("PropellerExample - InvokeInviteCompleted");
		Invoke ("SendInviteCompleted", 0.5f);
	}

	private void SendInviteCompleted ()
	{
		Debug.Log ("PropellerExample - SendInviteCompleted");
		PropellerSDK.SdkSocialInviteCompleted ();
	}

	public void InvokeShareCompleted ()
	{
		Debug.Log ("PropellerExample - InvokeShareCompleted");
		Invoke ("SendShareCompleted", 0.5f);
	}

	private void SendShareCompleted ()
	{
		Debug.Log ("PropellerExample - SendShareCompleted");
		PropellerSDK.SdkSocialShareCompleted ();
	}

    public void InvokeAcknowledgeVirtualGoods ()
    {
        Debug.Log ("PropellerExample - InvokeAcknowledgeVirtualGoods");
        Invoke ("SendAcknowledgeVirtualGoods", 0.5f);
    }

    private void SendAcknowledgeVirtualGoods ()
    {
        Debug.Log ("PropellerExample - SendAcknowledgeVirtualGoods");
        PropellerSDK.AcknowledgeVirtualGoods (m_virtualGoodsTransactionId, true);
    }
	
	private void DisplayTournamentInfo(string tag, Dictionary<string, string> tournamentInfo) {
		if ((tournamentInfo == null) || (tournamentInfo.Count == 0)) {
			Debug.Log ("PropellerExample - " + tag + " - no tournament currently running");
		} else {
		    string name = tournamentInfo["name"];
		    string campaignName = tournamentInfo["campaignName"];
		    string sponsorName = tournamentInfo["sponsorName"];
		    string startDate = tournamentInfo["startDate"];
		    string endDate = tournamentInfo["endDate"];
		    string logo = tournamentInfo["logo"];

		    Debug.Log ("PropellerExample - " + tag + " - " + name + " - " + campaignName + " - " + sponsorName + " - " + startDate + " - " + endDate + " - " + logo);
		}
	}

	private void initNotificationToggling () {
		UpdateNotificationStatus(PropellerSDK.NotificationType.push);
		UpdateNotificationStatus(PropellerSDK.NotificationType.local);
		PropellerSDK.SetNotificationListener(new PropellerNotificationListenerExample(this));
	}

	private void OnGUILanguageCodeSwitching (float kButtonX, float kSpacing, float kButtonWidth, float kButtonHeight) {
		if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 5f), kButtonWidth, kButtonHeight), "SetLanguageCode: en")) {
			languageCodeText = "en";
			PropellerSDK.SetLanguageCode(languageCodeText);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 6f), kButtonWidth, kButtonHeight), "SetLanguageCode: fr")) {
			languageCodeText = "fr";
			PropellerSDK.SetLanguageCode(languageCodeText);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 7f), kButtonWidth, kButtonHeight), "SetLanguageCode: it")) {
			languageCodeText = "it";
			PropellerSDK.SetLanguageCode(languageCodeText);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 8f), kButtonWidth, kButtonHeight), "SetLanguageCode: de")) {
			languageCodeText = "de";
			PropellerSDK.SetLanguageCode(languageCodeText);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 9f), kButtonWidth, kButtonHeight), "SetLanguageCode: es")) {
			languageCodeText = "es";
			PropellerSDK.SetLanguageCode(languageCodeText);
		}
		
		GUIStyle textStyle = new GUIStyle ();
		textStyle.fontSize = 20;
		textStyle.fontStyle = FontStyle.Bold;
		textStyle.font = new Font ();
		textStyle.normal.textColor = Color.white;
		textStyle.alignment = TextAnchor.MiddleCenter;
		
		GUI.Label (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 10f), kButtonWidth, kButtonHeight), "Language Code: " + languageCodeText, textStyle);
	}
	
	private void OnGUINotificationToggling (float kButtonX, float kSpacing, float kButtonWidth, float kButtonHeight) {
		if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 5f), kButtonWidth, kButtonHeight), "EnableNotifications: push")) {
			PropellerSDK.EnableNotification(PropellerSDK.NotificationType.push);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 6f), kButtonWidth, kButtonHeight), "EnableNotifications: local")) {
			PropellerSDK.EnableNotification(PropellerSDK.NotificationType.local);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 7f), kButtonWidth, kButtonHeight), "EnableNotifications: all")) {
			PropellerSDK.EnableNotification(PropellerSDK.NotificationType.all);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 8f), kButtonWidth, kButtonHeight), "DisableNotifications: push")) {
			PropellerSDK.DisableNotification(PropellerSDK.NotificationType.push);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 9f), kButtonWidth, kButtonHeight), "DisableNotifications: local")) {
			PropellerSDK.DisableNotification(PropellerSDK.NotificationType.local);
		} else if (GUI.Button (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 10f), kButtonWidth, kButtonHeight), "DisableNotifications: all")) {
			PropellerSDK.DisableNotification(PropellerSDK.NotificationType.all);
		}

		GUIStyle textStyle = new GUIStyle ();
		textStyle.fontSize = 20;
		textStyle.fontStyle = FontStyle.Bold;
		textStyle.font = new Font ();
		textStyle.normal.textColor = Color.white;
		textStyle.alignment = TextAnchor.MiddleCenter;

		GUI.Label (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 11f), kButtonWidth, kButtonHeight), pushNotificationStatusText, textStyle);
		GUI.Label (new Rect (kButtonX, kSpacing + ((kButtonHeight + kSpacing) * 12f), kButtonWidth, kButtonHeight), localNotificationStatusText, textStyle);
	}

	public void UpdateNotificationStatus(PropellerSDK.NotificationType type) {
		bool enabled = PropellerSDK.IsNotificationEnabled(type);

		Debug.Log ("PropellerExample - UpdateNotificationStatus - " + type + ":" + enabled);

		switch (type) {
		case PropellerSDK.NotificationType.none:
			break;
		case PropellerSDK.NotificationType.all:
			pushNotificationStatusText = "Push Notifications: " + enabled;
			localNotificationStatusText = "Local Notifications: " + enabled;
			break;
		case PropellerSDK.NotificationType.push:
			pushNotificationStatusText = "Push Notifications: " + enabled;
			break;
		case PropellerSDK.NotificationType.local:
			localNotificationStatusText = "Local Notifications: " + enabled;
			break;
		}
	}

}
