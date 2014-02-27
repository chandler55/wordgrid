using UnityEngine;
using System.Collections;

public class GameStartUI : MonoBehaviour, IEventListener
{

	private float	m_alphaValue = 1.0f;
	private float	m_alphaSpeed = 2.0f;
	private bool 	m_fading = false;

	// Use this for initialization
	void Start () 
	{
		EventManager.Instance.AttachListener(this, "StartGameEvent", this.HandleStartGameEvent);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( m_fading )
		{
			m_alphaValue -= Time.deltaTime * m_alphaSpeed;

			if ( m_alphaValue < 0 )
			{
				m_alphaValue = 0;
			}

			tk2dSprite[] sprites = GetComponentsInChildren<tk2dSprite>();
			foreach ( tk2dSprite s in sprites )
			{
				s.color = new Color(s.color.r, s.color.g, s.color.b, m_alphaValue);
			}

			if ( m_alphaValue < 0 )
			{
				m_fading = false;
			}
		}
	}

	bool HandleStartGameEvent( IEvent evt )
	{
		FadeOut();
		
		return false;
	}

	void FadeOut()
	{
		m_fading = true;
	}
}
