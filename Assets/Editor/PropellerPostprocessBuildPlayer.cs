using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;

public class PropellerPostprocessBuildPlayer : MonoBehaviour
{
	public enum UnityAPILevel
	{
		UNSUPPORTED,
		UNITY_2_6,
		UNITY_2_6_1,
		UNITY_3_0,
		UNITY_3_0_0,
		UNITY_3_1,
		UNITY_3_2,
		UNITY_3_3,
		UNITY_3_4,
		UNITY_3_5,
		UNITY_4_0,
		UNITY_4_0_1,
		UNITY_4_1,
		UNITY_4_2,
		UNITY_4_3,
		UNITY_4_5,
		UNITY_4_6,
		UNITY_5_0
	};

	private struct ExecutionResult
	{
		public bool success;
		public string output;
		public string error;
		public System.Exception exception;
	}

	private static string GetPluginRootPath ()
	{
		return GetPluginRootPath (Application.dataPath);
	}

	private static string GetPluginRootPath (string path)
	{
		if (File.Exists (path + "/Plugins/PropellerSDK.cs"))
		{
			return path;
		}

		string[] directories = null;

		try
		{
			directories = Directory.GetDirectories (path);
		}
		catch (System.Exception)
		{
			return null;
		}

		if (directories == null)
		{
			return null;
		}

		string pluginRootPath = null;

		foreach (string directory in directories)
		{
			pluginRootPath = GetPluginRootPath (directory);

			if (pluginRootPath != null)
			{
				break;
			}
		}

		return pluginRootPath;
	}

	private static ExecutionResult Execute (string command, params string[] arguments)
	{
		Process process = new Process ();
		process.EnableRaisingEvents = true;
		process.StartInfo.FileName = command;
		process.StartInfo.Arguments = string.Join (" ", arguments, 0, arguments.Length);

		// flags to be able to read build script output
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.UseShellExecute = false;

		ExecutionResult result = new ExecutionResult ();

		try
		{
			process.Start ();
			process.WaitForExit ();

			result.output = process.StandardOutput.ReadToEnd ();
			result.error = process.StandardError.ReadToEnd ();
			result.success = string.IsNullOrEmpty (result.error);
		}
		catch (System.Exception exception)
		{
			result.exception = exception;
			result.success = false;
		}

		return result;
	}

	private static string GetExecutionResultOutput (ExecutionResult result)
	{
		string output = "";

		if (!string.IsNullOrEmpty (result.output))
		{
			output += "\n";
			output += result.output;
		}

		if (!string.IsNullOrEmpty (result.error))
		{
			output += "\n";
			output += result.error;
		}

		if (result.exception != null)
		{
			output += "\n";
			output += result.exception.Message;
		}

		return output;
	}

	[PostProcessBuild]
	public static void OnPostprocessBuild (BuildTarget target, string pathToBuiltProject)
	{
#if UNITY_IOS
		string pluginRootPath = GetPluginRootPath ();

		if (pluginRootPath == null)
		{
			UnityEngine.Debug.Log ("[PropellerSDK] Unable to find the plugin root path");
			return;
		}

		if (File.Exists (pathToBuiltProject + "/Classes/PropellerSDK.h"))
		{
			UnityEngine.Debug.Log ("[PropellerSDK] iOS project build already exists, skipping automatic SDK injection");
			return;
		}

		UnityEngine.Debug.Log ("[PropellerSDK] Setting the post build script permissions...");

		ExecutionResult result = Execute (
			"chmod",
			"755",
			"\"" + pluginRootPath + "/Propeller/PostbuildScripts/PostbuildPropellerScript\"");

		if (result.success)
		{
			UnityEngine.Debug.Log ("[PropellerSDK] Setting the post build script permissions succeeded!" + GetExecutionResultOutput (result));
		}
		else
		{
			UnityEngine.Debug.LogError ("[PropellerSDK] Setting the post build script permissions failed!" + GetExecutionResultOutput (result));
			return;
		}

#if UNITY_2_6
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_2_6_1
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_0
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_0_0
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_1
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_2
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_3
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_4
		UnityAPILevel unityApiLevel = UNSUPPORTED;
#elif UNITY_3_5
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_3_5;
#elif UNITY_4_0
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_0;
#elif UNITY_4_0_1
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_0_1;
#elif UNITY_4_1
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_1;
#elif UNITY_4_2
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_2;
#elif UNITY_4_3
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_3;
#elif UNITY_4_5
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_5;
#elif UNITY_4_6
        UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_4_6;
#elif UNITY_5_0
		UnityAPILevel unityApiLevel = UnityAPILevel.UNITY_5_0;
#else
		UnityAPILevel unityApiLevel = UnityAPILevel.UNSUPPORTED;
#endif

		UnityEngine.Debug.Log ("[PropellerSDK] Injecting Propeller SDK into the Xcode project...");

		result = Execute (
			pluginRootPath + "/Propeller/PostbuildScripts/PostbuildPropellerScript",
			"\"" + pathToBuiltProject + "\"",
			"\"" + pluginRootPath + "\"",
			((int) unityApiLevel).ToString());

		if (result.success)
		{
			UnityEngine.Debug.Log ("[PropellerSDK] Injection of the Propeller SDK into the Xcode project succeeded!" + GetExecutionResultOutput (result));
		}
		else
		{
			UnityEngine.Debug.LogError ("[PropellerSDK] Injection of the Propeller SDK into the Xcode project failed!" + GetExecutionResultOutput (result));
			return;
		}
#endif

	}

}
