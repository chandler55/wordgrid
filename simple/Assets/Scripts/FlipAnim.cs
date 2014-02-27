using UnityEngine;
using System.Collections;

public class FlipAnim : MonoBehaviour
{
	public float duration = 1.0f;

	public GameObject object1;
	public GameObject object2;

	private float m_counter = 0.0f;

	void Update () 
	{
		m_counter += Time.deltaTime;
		if ( m_counter > duration )
		{
			m_counter = 0.0f;

			if ( object1 && object2 )
			{
				if ( object1.activeSelf )
				{
					object1.SetActive( false );
					object2.SetActive( true );
				}
				else
				{
					object1.SetActive( true );
					object2.SetActive( false );
				}
			}
		}
	}
}
