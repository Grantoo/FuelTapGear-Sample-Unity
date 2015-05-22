using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class PropellerImports : MonoBehaviour
{

#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern bool iOSSetUserConditions(string conditions);
	[DllImport ("__Internal")]
	public static extern bool iOSSyncUserValues();
#endif

	static PropellerImports ()
	{
	}

}
