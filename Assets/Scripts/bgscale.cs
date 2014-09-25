using UnityEngine;
using System.Collections;

public class bgscale : MonoBehaviour 
{
	//work in progress
	//just for experiments

	public GameObject mmbackground;

	void Start () 
	{
		Debug.Log ("Setting background scale.");


		var _width = Screen.width;
		var _height = Screen.height;

		Debug.Log ("screen width" + Screen.width);
		Debug.Log ("screen height" + Screen.height);

		var _scaleW = _width / 2048.0;
		var _scaleH = _height / 2048.0;


		Debug.Log ("_scaleW" + _scaleW);
		Debug.Log ("_scaleH" + _scaleH);

		transform.localScale = new Vector3((float)_scaleW, (float)_scaleH);


		Debug.Log ("transform" + transform.localScale);
	}
	
	void Update () 
	{

	}
}