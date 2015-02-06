using System;
using System.Collections.Generic;
using UnityEngine;

public class PropellerExample : MonoBehaviour
{
	public enum LanguageCode
	{
		en,
		fr,
		it,
		de,
		es,
		pt
	}
	;
	
	public enum NotificationType
	{
		all,
		push,
		local
	}
	;
	
	public enum NotificationState
	{
		enabled,
		disabled
	}
	;
	
	private const int MARGIN = 50;
	private const int PADDING = 30;
	private const int FONT_SIZE = 30;
	private bool m_bInitialized;
	private PropellerSDKListener m_listener;
	private string m_virtualGoodsTransactionId;
	
	// language code switching
	private string languageCodeText;
	
	// notification toggling
	private string pushNotificationStatusText;
	private string localNotificationStatusText;
	private Rect dialogRect;
	private List<string> dialogContent;
	private bool showDialog;
	private int lastLanguageCodeSelected;
	private ComboBox languageCodeComboBox;
	private int lastNotificationToggleSelected;
	private ComboBox notificationToggleComboBox;
	private Texture2D whiteTexture;
	
	private void OnGUI ()
	{
		GUI.skin.button.padding = new RectOffset (PADDING, PADDING, PADDING, PADDING);
		GUI.skin.button.fontSize = FONT_SIZE;
		GUI.skin.label.fontSize = FONT_SIZE;
		GUI.skin.label.wordWrap = true;
		GUI.skin.box.fontSize = FONT_SIZE;
		GUI.skin.box.normal.background = whiteTexture;
		
		if (showDialog) {
			int dialogSize = (Math.Min (Screen.width, Screen.height)) - (2 * MARGIN);
			int dialogX = (Screen.width - dialogSize) / 2;
			int dialogY = (Screen.height - dialogSize) / 2;
			
			dialogRect.width = dialogSize;
			dialogRect.height = 0;
			dialogRect.x = dialogX;
			dialogRect.y = dialogY;
			
			GUILayout.Window (0, dialogRect, OnDialogGUI, new GUIContent ("Output"));
		} else {
			GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginVertical ();
			
			GUILayout.FlexibleSpace ();
			
			int languageCodeSelected = languageCodeComboBox.Show ();
			
			if (languageCodeSelected != lastLanguageCodeSelected) {
				OnLanguageCodeSwitchingGUI (languageCodeSelected);
				lastLanguageCodeSelected = languageCodeSelected;
			}
			
			GUILayout.FlexibleSpace ();
			
			int notificationToggleSelected = notificationToggleComboBox.Show ();
			
			if (notificationToggleSelected != lastNotificationToggleSelected) {
				OnNotificationTogglingGUI (notificationToggleSelected);
				lastNotificationToggleSelected = notificationToggleSelected;
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Launch")) {
				PropellerSDK.Launch (m_listener);
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Test Login")) {
				InvokeLoginDetails ();
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("SyncChallengeCounts")) {
				UpdateDialog ("Calling PropellerSDK.SyncChallengeCounts()...");
				ShowDialog ();
				
				PropellerSDK.SyncChallengeCounts ();
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("SyncTournamentInfo")) {
				UpdateDialog ("Calling PropellerSDK.SyncTournamentInfo()...");
				ShowDialog ();
				
				PropellerSDK.SyncTournamentInfo ();
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("SyncVirtualGoods")) {
				UpdateDialog ("Calling PropellerSDK.SyncVirtualGoods()...");
				ShowDialog ();
				
				PropellerSDK.SyncVirtualGoods ();
			}
			
			GUILayout.FlexibleSpace ();
			
			GUILayout.EndVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
		}
	}
	
	private void OnLanguageCodeSwitchingGUI (int selectedIndex)
	{
		LanguageCode languageCode = (LanguageCode)selectedIndex;
		
		languageCodeText = languageCode.ToString ();
		UpdateDialog ("Calling PropellerSDK.SetLanguageCode(\"" + languageCodeText + "\")...");
		UpdateDialog ("Done!");
		ShowDialog ();
		
		PropellerSDK.SetLanguageCode (languageCodeText);
	}
	
	private void OnNotificationTogglingGUI (int selectedIndex)
	{
		int numTypes = Enum.GetValues (typeof(NotificationType)).Length;
		
		int typeIndex = selectedIndex % numTypes;
		NotificationType notificationType = (NotificationType)typeIndex;
		
		int stateIndex = selectedIndex / numTypes;
		NotificationState notificationState = (NotificationState)stateIndex;
		
		switch (notificationState) {
		case NotificationState.enabled:
			UpdateDialog ("Calling PropellerSDK.EnableNotification(" + notificationType.ToString () + ")...");
			ShowDialog ();
			
			switch (notificationType) {
			case NotificationType.all:
				PropellerSDK.EnableNotification (PropellerSDK.NotificationType.all);
				break;
			case NotificationType.push:
				PropellerSDK.EnableNotification (PropellerSDK.NotificationType.push);
				break;
			case NotificationType.local:
				// enabling local notifications will internally force a sync of
				// tournament info because the tournament schedule may have
				// changed and we want to force a sync to schedule local
				// notifications with the latest tournament info
				PropellerSDK.EnableNotification (PropellerSDK.NotificationType.local);
				break;
			}
			break;
		case NotificationState.disabled:
			UpdateDialog ("Calling PropellerSDK.DisableNotification(" + notificationType.ToString () + ")...");
			ShowDialog ();
			
			switch (notificationType) {
			case NotificationType.all:
				PropellerSDK.DisableNotification (PropellerSDK.NotificationType.all);
				break;
			case NotificationType.push:
				PropellerSDK.DisableNotification (PropellerSDK.NotificationType.push);
				break;
			case NotificationType.local:
				PropellerSDK.DisableNotification (PropellerSDK.NotificationType.local);
				break;
			}
			break;
		}
	}
	
	private void OnDialogGUI (int windowID)
	{
		GUI.FocusWindow (windowID);
		
		GUILayout.BeginVertical ();
		
		GUILayout.Label (new GUIContent ("Language Code: " + languageCodeText));
		GUILayout.Label (new GUIContent (pushNotificationStatusText));
		GUILayout.Label (new GUIContent (localNotificationStatusText));
		
		if (dialogContent.Count > 0) {
			foreach (string item in dialogContent) {
				GUILayout.Label (new GUIContent (item));
			}
		}
		
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		
		if (GUILayout.Button ("Close")) {
			HideDialog ();
			ClearDialog ();
		}
		
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		
		GUILayout.EndVertical ();
	}
	
	public void OnPropellerSDKChallengeCountUpdated (string count)
	{
		UpdateDialog ("OnPropellerSDKChallengeCountUpdated() callback called...");
		UpdateDialog ("Challenge count: " + count);
		UpdateDialog ("Done!");
		ShowDialog ();
		
		// Update the UI with the count
	}
	
	public void OnPropellerSDKTournamentInfo (Dictionary<string, string> tournamentInfo)
	{
		UpdateDialog ("OnPropellerSDKTournamentInfo() callback called...");
		
		if ((tournamentInfo == null) || (tournamentInfo.Count == 0)) {
			UpdateDialog ("No tournament currently running or scheduled");
		} else {
			UpdateDialog ("Tournament Name: " + tournamentInfo ["name"]);
			UpdateDialog ("Campaign Name: " + tournamentInfo ["campaignName"]);
			UpdateDialog ("Sponsor Name: " + tournamentInfo ["sponsorName"]);
			UpdateDialog ("Start Date: " + tournamentInfo ["startDate"]);
			UpdateDialog ("End Date: " + tournamentInfo ["endDate"]);
			UpdateDialog ("Logo: " + tournamentInfo ["logo"]);
		}
		
		UpdateDialog ("Done!");
		ShowDialog ();
		
		// Update the UI with the tournament information
	}
	
	public void OnPropellerSDKVirtualGoodList (Dictionary<string, object> virtualGoodInfo)
	{
		UpdateDialog ("OnPropellerSDKVirtualGoodList() callback called...");
		
		string transactionId = (string)virtualGoodInfo ["transactionID"];
		
		UpdateDialog ("Transaction ID: " + transactionId);
		
		List<string> virtualGoods = (List<string>)virtualGoodInfo ["virtualGoods"];
		
		if (virtualGoods.Count == 0) {
			UpdateDialog ("No virtual goods have been awarded");
		} else {
			foreach (string virtualGood in virtualGoods) {
				UpdateDialog ("Received '" + virtualGood + "'");
			}
		}
		
		// Store the virtual goods for consumption
		
		m_virtualGoodsTransactionId = transactionId;
		
		UpdateDialog ("Acknowledging receipt of virtual goods");
		UpdateDialog ("Done!");
		ShowDialog ();
		
		InvokeAcknowledgeVirtualGoods ();
	}
	
	public void OnPropellerSDKVirtualGoodRollback (string transactionId)
	{
		UpdateDialog ("OnPropellerSDKVirtualGoodRollback() callback called...");
		UpdateDialog ("Done!");
		ShowDialog ();
		
		// Rollback the virtual good transaction for the given transaction ID
	}
	
	private void Awake ()
	{
		if (!m_bInitialized) {
			GameObject.DontDestroyOnLoad (gameObject);
			
			if (!Application.isEditor) {
				m_listener = new PropellerListenerExample (this);
				
				Initialize ();
			}
			
			m_bInitialized = true;
		} else {
			GameObject.Destroy (gameObject);
		}
	}
	
	private void Initialize ()
	{
		languageCodeText = "en";
		
		UpdateNotificationStatus (PropellerSDK.NotificationType.push);
		UpdateNotificationStatus (PropellerSDK.NotificationType.local);
		PropellerSDK.SetNotificationListener (new PropellerNotificationListenerExample (this));
		
		dialogRect = new Rect ();
		dialogContent = new List<string> ();
		showDialog = false;
		
		whiteTexture = new Texture2D (1, 1);
		whiteTexture.SetPixel (0, 0, Color.white);
		whiteTexture.wrapMode = TextureWrapMode.Repeat;
		whiteTexture.Apply ();
		
		GUIStyle comboBoxListStyle = new GUIStyle ();
		comboBoxListStyle.padding = new RectOffset (PADDING, PADDING, PADDING, PADDING);
		comboBoxListStyle.fontSize = FONT_SIZE;
		comboBoxListStyle.normal.textColor = Color.black;
		comboBoxListStyle.normal.background = whiteTexture;
		
		GUIContent[] languageCodeList = new GUIContent[6];
		languageCodeList [0] = new GUIContent ("Language Code: en");
		languageCodeList [1] = new GUIContent ("Language Code: fr");
		languageCodeList [2] = new GUIContent ("Language Code: it");
		languageCodeList [3] = new GUIContent ("Language Code: de");
		languageCodeList [4] = new GUIContent ("Language Code: es");
		languageCodeList [5] = new GUIContent ("Language Code: pt");
		
		lastLanguageCodeSelected = 0;
		languageCodeComboBox = new ComboBox (
			languageCodeList [lastLanguageCodeSelected],
			languageCodeList,
			comboBoxListStyle);
		
		GUIContent[] notificationToggleList = new GUIContent[6];
		notificationToggleList [0] = new GUIContent ("Notifications Enabled: all");
		notificationToggleList [1] = new GUIContent ("Notifications Enabled: push");
		notificationToggleList [2] = new GUIContent ("Notifications Enabled: local");
		notificationToggleList [3] = new GUIContent ("Notifications Disabled: all");
		notificationToggleList [4] = new GUIContent ("Notifications Disabled: push");
		notificationToggleList [5] = new GUIContent ("Notifications Disabled: local");
		
		lastNotificationToggleSelected = 0;
		notificationToggleComboBox = new ComboBox (
			notificationToggleList [lastNotificationToggleSelected],
			notificationToggleList,
			comboBoxListStyle);
	}
	
	public void ShowDialog ()
	{
		showDialog = true;
	}
	
	private void HideDialog ()
	{
		showDialog = false;
	}
	
	private void ClearDialog ()
	{
		dialogContent.Clear ();
	}
	
	public void UpdateDialog (string text)
	{
		dialogContent.Add (text);
	}
	
	public void UpdateNotificationStatus (PropellerSDK.NotificationType type)
	{
		bool enabled = PropellerSDK.IsNotificationEnabled (type);
		
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
		
		ShowDialog ();
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
		
		PropellerSDK.SubmitMatchResult (matchResult);
		PropellerSDK.Launch (listener);
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
		loginInfo.Add ("email", "testguy@fuelpowered.com");
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
	
}
