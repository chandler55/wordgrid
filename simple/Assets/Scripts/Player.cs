using UnityEngine;
using System.Collections;

public class Player : Singleton<Player> 
{
	public Transform 		topBoundary;
	public ParticleEmitter	smokeEmitter;
	public AudioSource		explosionSound;
	public AudioSource		jumpSound;

	public Transform		hoveringAnimTop;
	public Transform		hoveringAnimBottom;
	
	private Vector3 		m_currentVelocity;
	
	private	bool			m_gameStarted = false;

	private Vector3		m_hoveringAnimTop;
	private Vector3		m_hoveringAnimBottom;

	private	GoTween			m_tweener;
	void Start () 
	{
		m_hoveringAnimTop = hoveringAnimTop.position;
		m_hoveringAnimBottom = hoveringAnimBottom.position;
		
		rigidbody2D.gravityScale = 0.0f;
		smokeEmitter.emit = false;

		// start hovering toward up
		m_tweener = Go.to(gameObject.transform, 0.5f, new GoTweenConfig().position( m_hoveringAnimBottom ).onComplete(c => OnHoverDownComplete() ) );
	}
	
	void Update ()
	{
		if ( Input.GetMouseButtonDown(0) )
		{
			if ( !m_gameStarted )
			{
				m_gameStarted = true;
				rigidbody2D.gravityScale = 0.35f;
				GameManager.GetInstance().StartGame();

				m_tweener.destroy();
			}
			
			Jump();
		}
	}

	void OnHoverUpComplete()
	{
		if ( !m_gameStarted )
		{
			m_tweener = Go.to(gameObject.transform, 0.5f, new GoTweenConfig().position( m_hoveringAnimBottom ).onComplete(c => OnHoverDownComplete() ) );
		}
	}

	void OnHoverDownComplete()
	{
		if ( !m_gameStarted )
		{
			m_tweener = Go.to(gameObject.transform, 0.5f, new GoTweenConfig().position( m_hoveringAnimTop ).onComplete(c => OnHoverUpComplete() ) );
		}
	}
	
	void Jump()
	{
		if ( jumpSound )
		{
			jumpSound.Play();
		}

		if ( topBoundary.position.y > gameObject.transform.position.y )
		{
			if ( rigidbody2D )
			{
				rigidbody2D.velocity = new Vector2(0, 40);
			}
		}
	}
	
	void OnTriggerEnter2D( Collider2D collider )
	{
		EventManager.Instance.QueueEvent( new Events.PlayerHit() );
		Die();
		GameManager.GetInstance().GameOver();
	}

	void Die()
	{
		tk2dSpriteAnimator animatedSprite = GetComponent<tk2dSpriteAnimator>();
		if ( animatedSprite )
		{
			animatedSprite.Stop();
		}

		if ( explosionSound )
		{
			explosionSound.Play();
		}

		if ( smokeEmitter )
		{
			smokeEmitter.emit = true;
		}
		
		Destroy (rigidbody2D);
		Destroy (this);
	}
}
