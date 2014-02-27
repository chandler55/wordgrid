using UnityEngine;
using System.Collections;

public class GameOverUI : MonoBehaviour, IEventListener
{
	public GameObject gameoverText;
	public GameObject gameOverTextEndPos;

	public GameObject restartButton;
	public GameObject restartButtonEndPos;
	

	private Vector3 	m_gameOverTextInitialPos;
	private Vector3		m_restartButtonInitialPos;

	// Use this for initialization
	void Start ()
	{
		m_gameOverTextInitialPos = gameoverText.transform.position;
		m_restartButtonInitialPos = restartButton.transform.position;

		EventManager.Instance.AttachListener(this, "GameOverEvent", this.HandleGameOverEvent);
		EventManager.Instance.AttachListener(this, "StartGameEvent", this.HandleStartGameEvent);
		EventManager.Instance.AttachListener(this, "RestartGameEvent", this.HandleRestartGameEvent);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnLoadLevel()
	{
		gameoverText.transform.position = m_gameOverTextInitialPos;
		restartButton.transform.position = m_restartButtonInitialPos;
	}

	bool HandleGameOverEvent( IEvent evt )
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AdMobPlugin.GetInstance().Show();
#endif

		Go.to( gameoverText.transform, 0.5f, new GoTweenConfig().position( gameOverTextEndPos.transform.position ).setEaseType(GoEaseType.BounceOut) );
		Go.to( restartButton.transform, 0.5f, new GoTweenConfig().position( restartButtonEndPos.transform.position ).setEaseType(GoEaseType.BounceOut) );
		
		return false;
	}

	bool HandleStartGameEvent( IEvent evt )
	{
		if ( gameoverText )
		{
			gameoverText.transform.position = m_gameOverTextInitialPos;
		}
		if ( restartButton )
		{
			restartButton.transform.position = m_restartButtonInitialPos;
		}

		return false;
	}

	bool HandleRestartGameEvent( IEvent evt )
	{
		restartButton.SetActive( false );
		return false;
	}

	public void RestartGame()
	{	
#if UNITY_ANDROID && !UNITY_EDITOR
		AdMobPlugin.GetInstance().Hide();
#endif

		EventManager.Instance.QueueEvent( new Events.RestartGameEvent() );
	}
}
