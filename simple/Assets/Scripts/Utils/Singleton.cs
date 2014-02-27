using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
	static T s_Instance;

	public static T GetInstance() 
	{ 
		if (!s_Instance)
		{
			//Debug.LogError ("Unable to find " + typeof (T).Name + ". Is it in the scene?");
		}
		return s_Instance;
	}
	
	public void Awake () 
	{
		if (s_Instance != null)
		{
			Debug.LogError ("There's already an instance of MyManager. Is it more than once in the scene?", this);
		}
		s_Instance = (T)(object)this;
	}
}