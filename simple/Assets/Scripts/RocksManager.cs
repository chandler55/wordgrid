using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocksManager : MonoBehaviour, IEventListener 
{

	public GameObject rockPairPrefab;

	private GameObject 	m_currentRockPair;
	private float 		m_startingPointX = 0.0f;
	private float 		m_rockSpacing = 20.0f;

	private bool		m_gameStarted = false;
	private	Scroller	m_scroller;

	private List<GameObject> listRockPairs = new List<GameObject>();

	void Start () 
	{		
		m_startingPointX = gameObject.transform.position.x;
		m_scroller = gameObject.GetComponent<Scroller>();
		if ( m_scroller )
		{
			m_scroller.enabled = false;
		}

		EventManager.Instance.AttachListener(this, "GameOverEvent", this.HandleGameOverEvent);
		EventManager.Instance.AttachListener(this, "StartGameEvent", this.HandleStartGameEvent);
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
		if ( m_gameStarted )
		{
			if ( m_currentRockPair.gameObject.transform.position.x + m_rockSpacing < m_startingPointX )
			{
				Vector3 spawnPos = new Vector3( m_currentRockPair.gameObject.transform.position.x + m_rockSpacing, 0, 0);
				m_currentRockPair = SpawnRockPair( spawnPos );
			}
		}
	}

	GameObject SpawnRockPair( Vector3 spawnPos )
	{
		GameObject go = Instantiate( rockPairPrefab, spawnPos, Quaternion.identity ) as GameObject;
		go.transform.parent = gameObject.transform;
		go.transform.localPosition = new Vector3( go.transform.localPosition.x, go.transform.localPosition.y, 0 );
		listRockPairs.Add(go);
		return go;
	}

	bool HandleGameOverEvent( IEvent evt )
	{
		return false;
	}

	bool HandleStartGameEvent( IEvent evt )
	{
		m_gameStarted = true;

		m_currentRockPair = SpawnRockPair( gameObject.transform.position );

		if ( m_scroller )
		{
			m_scroller.enabled = true;
		}

		return false;
	}
}
