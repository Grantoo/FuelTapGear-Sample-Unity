using UnityEngine;
using System.Collections;

public class gearMount : MonoBehaviour 
{
	public float spinvelocity = 0.0f;
	public int direction = 0;
	public float relativegamevelocity = 1.0f;
	private float gamevelocity = 0.0f;
	private float angle = 0.0f;

	public void SetGameVelocity (float velocity) 
	{
		gamevelocity = velocity * relativegamevelocity;
	}
	
	void Start () 
	{
	}
	
	void Update () 
	{
		if (direction == 1) 
		{
			angle -= (spinvelocity + gamevelocity);
		} 
		else 
		{
			angle += (spinvelocity + gamevelocity);
		}

		transform.rotation = Quaternion.Euler(0, 0, angle);

	}
}




