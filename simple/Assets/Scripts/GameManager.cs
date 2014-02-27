using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>, IEventListener
{
	public enum GameState
	{
		GameState_TapToPlay,
		GameState_Flying,
		GameState_Gameover,
	}

	private GameState m_gameState = GameState.GameState_TapToPlay;
	public GameState CurrentGameState
	{
		get
		{
			return m_gameState;
		}
	}

	void Start () 
	{
		Application.targetFrameRate = 60;
		EventManager.Instance.AttachListener(this, "RestartGameEvent", this.HandleRestartGameEvent);
		
	}
	
	void Update ()
	{
	
	}

	bool HandleRestartGameEvent( IEvent evt )
	{
		EventManager.Instance.QueueEvent( new Events.LoadLevel("flappybird") );
		return false;
	}

	public void StartGame()
	{
		EventManager.Instance.QueueEvent( new Events.StartGameEvent() );
		m_gameState = GameState.GameState_Flying;

		Debug.Log ("Start game");
	}

	public void GameOver()
	{
		EventManager.Instance.QueueEvent( new Events.GameOverEvent() );
		m_gameState = GameState.GameState_Gameover;

		Debug.Log ("Gameover");
	}

	void FadeAndLoadLevel( string levelName )
	{

	}
}
