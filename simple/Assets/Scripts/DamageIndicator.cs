using UnityEngine;
using System.Collections;

public class DamageIndicator : MonoBehaviour, IEventListener
{
	public tk2dSprite 	damageSprite;
	public float 		flashDuration = 0.1f;
	
	private float 	m_flashTimer = 0.0f;
	private bool 	m_indicatingDamage = false;
	void Start ()
	{
		EventManager.Instance.AttachListener(this, "PlayerHit", this.HandlePlayerHit);
	}
	
	void Update () 
	{
		if ( m_indicatingDamage )
		{
			m_flashTimer += Time.deltaTime;
			if ( m_flashTimer < flashDuration )
			{
				damageSprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - (m_flashTimer / flashDuration));
			}
			else
			{
				damageSprite.color = new Color(1.0f, 1.0f, 1.0f, 0 );
				m_indicatingDamage = false;
			}
		}
	}

	void IndicateDamage()
	{
		damageSprite.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		m_flashTimer = 0.0f;
		m_indicatingDamage = true;
	}

	bool HandlePlayerHit(IEvent evt)
	{
		IndicateDamage();
		return false;
	}
}
