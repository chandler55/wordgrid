using UnityEngine;
using System.Collections;

public class RockPair : MonoBehaviour 
{
	public tk2dSprite rockTop;
	public tk2dSprite rockBottom;
	public AudioSource	coinSound;

	private float 	rockTopMin = 38.0f;
	private float 	rockTopMax = 13.0f;
	private float 	m_distanceBetweenRocks = 47.0f;
	private bool 	m_pointConsumed = false;

	void Start () 
	{
		Vector3 rockTopPos = new Vector3(0, Random.Range( rockTopMax, rockTopMin ), 0 );
		Vector3 rockBottomPos = new Vector3( 0, rockTopPos.y - m_distanceBetweenRocks, 0 );

		rockTop.gameObject.transform.localPosition = rockTopPos;
		rockBottom.gameObject.transform.localPosition = rockBottomPos;
	}
	
	void Update () 
	{
		if ( !m_pointConsumed )
		{
			if ( Player.GetInstance() && Player.GetInstance().gameObject.transform.position.x > gameObject.transform.position.x )
			{
				EventManager.Instance.QueueEvent(new Events.IncrementScore() );
				m_pointConsumed = true;

				if ( coinSound )
				{
					coinSound.Play();
				}
			}
		}
	}
}
