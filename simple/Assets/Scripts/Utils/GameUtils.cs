using UnityEngine;
using System.Collections;

public class GameUtils : MonoBehaviour 
{

	public static void Assert (bool condition, string message = "") 
	{
		if (!condition)
		{
			Debug.LogError(message);
		}
	}

	void Start () 
	{
	
	}
	
	void Update () 
	{
	
	}
}
