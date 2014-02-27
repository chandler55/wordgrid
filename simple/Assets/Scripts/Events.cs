using UnityEngine;
using System.Collections;

public class Events 
{
	public class PlayerHit : BaseEvent
	{
		public PlayerHit() 
		{
		}
	}

	public class GameOverEvent : BaseEvent 
	{
		public GameOverEvent() 
		{
		}
	}

	public class StartGameEvent : BaseEvent 
	{
		public StartGameEvent() 
		{
		}
	}

	public class RestartGameEvent : BaseEvent 
	{
		public RestartGameEvent() 
		{
		}
	}

	public class IncrementScore : BaseEvent
	{
		public IncrementScore()
		{
		}
	}

	public class LoadLevel : BaseEvent
	{
		public readonly string LevelName;

		public LoadLevel( string levelName )
		{
			LevelName = levelName;
		}
	}

	public class ScoreEvent : BaseEvent 
	{
		public readonly int Score;
		
		public ScoreEvent( int score ) 
		{
			Score = score;
		}
	}
}
