using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour 
{

	void Start () 
	{
	
	}
	
	void Update ()
	{
	
	}

	void OnTriggerEnter2D( Collider2D collider )
	{
		if ( collider.gameObject.CompareTag("Boundary") )
		{
			Destroy (gameObject.transform.parent.gameObject);
		}
	}
}
