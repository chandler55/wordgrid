using UnityEngine;
using System.Collections;

public class CameraFollowPlayer : MonoBehaviour 

{

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if ( player )
		{
			transform.position = new Vector3( player.transform.position.x, player.transform.position.y, transform.position.z );
		}
	}
}
