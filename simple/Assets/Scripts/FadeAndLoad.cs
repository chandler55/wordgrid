using UnityEngine;
using System.Collections;

public class FadeAndLoad : MonoBehaviour, IEventListener
{
	public tk2dSprite 	blackSprite;
	public float		duration = 1.0f;

	private float		m_timer = 0.0f;
	private bool 		m_fading = false;
	private string 		m_levelName;

	void Start () 
	{
		EventManager.Instance.AttachListener(this, "LoadLevel", this.HandleLoadLevel);
		blackSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	}

	void OnDestroy()
	{
		if ( EventManager.Instance )
		{
			EventManager.Instance.DetachListener(this);
		}
	}
	
	void Update () 
	{
		if ( m_fading )
		{
			m_timer += Time.deltaTime;
			if ( m_timer >= duration )
			{
				m_fading = false;
				Application.LoadLevel( m_levelName );

			}

			blackSprite.color = new Color(1.0f, 1.0f, 1.0f, m_timer / duration);
		}
	}

	bool HandleLoadLevel( IEvent evt )
	{
		Events.LoadLevel loadLevelEvent = evt as Events.LoadLevel;
		m_levelName = loadLevelEvent.LevelName;
		m_fading = true;
		m_timer = 0.0f;

		return false;
	}
}
