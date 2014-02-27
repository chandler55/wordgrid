using UnityEngine;
using System.Collections;

public class Hiscore : MonoBehaviour, IEventListener
{

	private int highscore = 0;

	// Use this for initialization
	void Start () 
	{
		UpdateHiscore( PlayerPrefs.GetInt("highscore", 0) );

		EventManager.Instance.AttachListener(this, "ScoreEvent", HandleScoreEvent );
		
	}

	void OnDestroy()
	{
		PlayerPrefs.SetInt("highscore", highscore);
		if ( EventManager.Instance )
		{
			EventManager.Instance.DetachListener(this);
		}
	}

	void UpdateHiscore( int score )
	{
		highscore = score;
		
		tk2dTextMesh hiscoreText = GetComponent<tk2dTextMesh>();
		if ( hiscoreText )
		{
			hiscoreText.text = "Hiscore : " + highscore;
		}
	}

	bool HandleScoreEvent( IEvent evt )
	{
		Events.ScoreEvent scoreEvent = evt as Events.ScoreEvent;
		if ( highscore < scoreEvent.Score )
		{
			UpdateHiscore( scoreEvent.Score );
		}
		return false;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
