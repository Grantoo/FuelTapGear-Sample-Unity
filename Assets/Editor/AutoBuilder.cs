using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public static class AutoBuilder {
 
	static string GetProjectName()
	{
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}
 
	static string[] GetScenePaths()
	{
		//note: GetScenePaths() gets ALL scenes in order even non-checked one - so lets create our own list
		string[] scenes = { "Assets/Scenes/Title.unity",  "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GamePlay.unity"};

		/* TODO: may be able to EditorBuildSettings.scenes[i]. to get if the scene is checked
		string[] scenes = new string[EditorBuildSettings.scenes.Length];
 
		for(int i = 0; i < scenes.Length; i++)
		{
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}
 		*/

		return scenes;
	}
 
	[MenuItem("File/AutoBuilder/Windows/32-bit")]
	static void PerformWinBuild ()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
		BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/Win/" + GetProjectName() + ".exe",BuildTarget.StandaloneWindows,BuildOptions.None);
	}
 
	[MenuItem("File/AutoBuilder/Windows/64-bit")]
	static void PerformWin64Build ()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
		BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/Win64/" + GetProjectName() + ".exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
	}
 
	[MenuItem("File/AutoBuilder/iOS")]
	static void PerformiOSBuild ()
	{
		// according to the documentation, this method isn't available when
		// running the Editor in batch mode. setting the build target via
		// the buildTarget command-line switch
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);

		//script hook for jenkins building of iOS

		string[] arguments = System.Environment.GetCommandLineArgs();

		if ((arguments != null) && (arguments.Length == 14))
		{
			string outputPath = arguments[11];//must match this index with num command line args and where your arg is on the line
			string buildNumber = arguments[12];
			string targetDebugEnv = arguments[13];
			PlayerSettings.bundleVersion = buildNumber;
			PlayerSettings.iOS.buildNumber = buildNumber;
			PlayerSettings.iOS.requiresFullScreen = true;

			SetTargetDebugEnv(BuildTargetGroup.iOS, targetDebugEnv);

			BuildPipeline.BuildPlayer(GetScenePaths(), outputPath, BuildTarget.iOS, BuildOptions.None);
		}
	}

	[MenuItem("File/AutoBuilder/Android")]
	static void PerformAndroidBuild ()
	{
		// according to the documentation, this method isn't available when
		// running the Editor in batch mode. setting the build target via
		// the buildTarget command-line switch
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

		//script hook for jenkins building of Android
		string[] arguments = System.Environment.GetCommandLineArgs();

		if ((arguments != null) && (arguments.Length == 18)) {
			string outputPath = arguments[11];//must match this index with num command line args :(
			string buildNumber = arguments[12];
			string targetDebugEnv = arguments[13];
			string keystorePath = arguments[14];
			string keystorePass = arguments[15];
			string keystoreAlias = arguments[16];
			string keystoreAliasPass = arguments[17];

			char[] versionDelimiter = {'.'};
			string[] versionParts = buildNumber.Split(versionDelimiter);

			if (versionParts == null) {
				// return error, undefined parts
				return;
			}

			if (versionParts.Length == 0) {
				// return error, no parts parsed
				return;
			}

			string versionBuildNumber = versionParts[versionParts.Length - 1];

			if (versionBuildNumber == null) {
				// return error, last part is undefined
				return;
			}

			if (versionBuildNumber.Length == 0) {
				// return error, last part is an empty string
				return;
			}

			int bundleVersionCode = -1;

			if (!int.TryParse(versionBuildNumber, out bundleVersionCode)) {
				// return error, could not parse int from version build number
				return;
			}

			if (bundleVersionCode <= 0) {
				// return error, invalid bundle version code
				return;
			}
			
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

			PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

			PlayerSettings.bundleVersion = buildNumber;

			SetAndroidKeystoreProperties(targetDebugEnv, keystorePath, keystorePass, keystoreAlias, keystoreAliasPass);

			SetTargetDebugEnv(BuildTargetGroup.Android, targetDebugEnv);

			BuildPipeline.BuildPlayer(GetScenePaths(), outputPath, BuildTarget.Android, BuildOptions.None);
		}
	}

	[MenuItem("File/AutoBuilder/Web/Standard")]
	static void PerformWebBuild ()
	{
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayer);
		BuildPipeline.BuildPlayer(GetScenePaths(), "Builds/Web",BuildTarget.WebPlayer,BuildOptions.None);
	}

	private static void SetAndroidKeystoreProperties(string targetDebugEnv, string keystorePath, string keystorePass, string keystoreAlias, string keystoreAliasPass)
	{
		if ((targetDebugEnv == null) || !targetDebugEnv.Equals("none")) {
			keystorePath = System.Environment.GetEnvironmentVariable("HOME") + "/.android/debug.keystore";
			keystorePass = "android";
			keystoreAlias = "androiddebugkey";
			keystoreAliasPass = "android";
		}

		if (!string.IsNullOrEmpty(keystorePath)) {
			PlayerSettings.Android.keystoreName = keystorePath;
		}
		
		if (!string.IsNullOrEmpty(keystorePass)) {
			PlayerSettings.Android.keystorePass = keystorePass;
		}
		
		if (!string.IsNullOrEmpty(keystoreAlias)) {
			PlayerSettings.Android.keyaliasName = keystoreAlias;
		}
		
		if (!string.IsNullOrEmpty(keystoreAliasPass)) {
			PlayerSettings.Android.keyaliasPass = keystoreAliasPass;
		}
	}

	private static void SetTargetDebugEnv(BuildTargetGroup buildTargetGroup, string targetDebugEnv)
	{
		string scriptingDefineSymbols = "PROPELLER_SDK";
		
		if (targetDebugEnv == null) {
			UnityEngine.Debug.Log ("[PropellerSDK] Undefined target debug environment - defaulting to a release build");
		} else if (targetDebugEnv.Equals ("none")) {
			UnityEngine.Debug.Log ("[PropellerSDK] Configuring the build environment for release");
		} else if (targetDebugEnv.Equals ("internal") ||
		           targetDebugEnv.Equals ("sandbox") ||
		           targetDebugEnv.Equals ("production")) {
			UnityEngine.Debug.Log ("[PropellerSDK] Configuring the build environment for debug (" + targetDebugEnv + ")");
			scriptingDefineSymbols += ";PROPELLER_SDK_DEBUG";
			scriptingDefineSymbols += ";PROPELLER_SDK_DEBUG_" + targetDebugEnv.ToUpper ();
		} else {
			UnityEngine.Debug.Log ("[PropellerSDK] Unsupported target debug environment (" + targetDebugEnv + ") - defaulting to a release build");
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
	}

}
