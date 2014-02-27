using UnityEngine;
using System.Collections;

public class Scroller : MonoBehaviour, IEventListener 
{
	public Vector3		scrollingSpeed;

	private Vector3 	m_initialScrollingSpeed;

	// Use this for initialization
	void Start () 
	{
		m_initialScrollingSpeed = scrollingSpeed;

		EventManager.Instance.AttachListener(this, "GameOverEvent", this.HandleGameOverEvent);
		EventManager.Instance.AttachListener(this, "StartGameEvent", this.HandleStartGameEvent);
	}

	void OnDisable()
	{
		if ( EventManager.Instance )
		{
			EventManager.Instance.DetachListener(this);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		gameObject.transform.position += scrollingSpeed * Time.deltaTime;
	}

	bool HandleGameOverEvent( IEvent evt )
	{
		scrollingSpeed = Vector3.zero;
		return false;
	}
	
	bool HandleStartGameEvent( IEvent evt )
	{
		scrollingSpeed = m_initialScrollingSpeed;
		return false;
	}
}
