using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool DelegateEventHandler(IEvent evt);

public interface IEventListener
{
}

public interface IEvent {
	string GetName();
}

public class BaseEvent : IEvent {
	public string GetName() { return this.GetType().Name; }
}

public class EventManager : MonoBehaviour
{
	public bool LimitQueueProcesing = false;
	public float QueueProcessTime = 0.0f;
	private static EventManager s_Instance = null;
	private Hashtable m_listenerTable = new Hashtable();
	private Queue m_eventQueue = new Queue();
	
	// override so we don't have the typecast the object
	public static EventManager Instance 
	{
		get 
		{
			if (s_Instance == null) 
			{
				//GameObject go = new GameObject("EventManager");
				//go.AddComponent<EventManager>();
				//s_Instance = go.GetComponent<EventManager>();
				s_Instance = GameObject.FindObjectOfType (typeof(EventManager)) as EventManager;
			}
			return s_Instance;
		}
	}

	void Awake()
	{
		if ( s_Instance )
		{
			//Destroy(gameObject);
		}

		s_Instance = this;

		DontDestroyOnLoad(gameObject);
	}
	
	//Add a listener to the event manager that will receive any events of the supplied event name.
	public bool AttachListener(IEventListener listener, string eventName, DelegateEventHandler handler)
	{
		if (listener == null || eventName == null)
		{
			Debug.Log("Event Manager: AttachListener failed due to no listener or event name specified.");
			return false;
		}
		
		if (!m_listenerTable.ContainsKey(eventName)) {
			m_listenerTable.Add(eventName, new ArrayList());
		}
		
		ArrayList listenerList = m_listenerTable[eventName] as ArrayList;
		if (listenerList.Contains(handler))
		{
			Debug.Log("Event Manager: Listener: " + listener.GetType().ToString() + " is already in list for event: " + eventName);
			return false; //listener already in list
		}
		
		listenerList.Add(handler);
		return true;
	}
	
	public bool DetachListener (IEventListener listener) {
		foreach (DictionaryEntry pair in m_listenerTable) {
			ArrayList listenerList = pair.Value as ArrayList;
			listenerList.Remove (listener);
		}
		return true;
	}
	
	//Remove a listener from the subscribed to event.
	public bool DetachListener(IEventListener listener, string eventName, DelegateEventHandler handler)
	{
		if (!m_listenerTable.ContainsKey(eventName))
			return false;
		
		ArrayList listenerList = m_listenerTable[eventName] as ArrayList;
		if (!listenerList.Contains(handler))
			return false;
		
		listenerList.Remove(handler);
		return true;
	}
	
	//Trigger the event instantly, this should only be used in specific circumstances,
	//the QueueEvent function is usually fast enough for the vast majority of uses.
	public bool TriggerEvent(IEvent evt)
	{
		string eventName = evt.GetName();
		if (!m_listenerTable.ContainsKey(eventName))
		{
			Debug.Log("Event Manager: Event \"" + eventName + "\" has no listeners!");
			return false; //No listeners for event so ignore it
		}
		
		ArrayList listenerList = m_listenerTable[eventName] as ArrayList;
		foreach (DelegateEventHandler listener in listenerList)
		{
			if (listener(evt))
				return true; //Event consumed.
		}
		
		return true;
	}
	
	//Inserts the event into the current queue.
	public bool QueueEvent(IEvent evt)
	{
		if (!m_listenerTable.ContainsKey(evt.GetName()))
		{
			Debug.Log("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetName());
			return false;
		}
		
		m_eventQueue.Enqueue(evt);
		return true;
	}
	
	//Every update cycle the queue is processed, if the queue processing is limited,
	//a maximum processing time per update can be set after which the events will have
	//to be processed next update loop.
	void Update()
	{
		float timer = 0.0f;
		while (m_eventQueue.Count > 0)
		{
			if (LimitQueueProcesing)
			{
				if (timer > QueueProcessTime)
					return;
			}
			
			IEvent evt = m_eventQueue.Dequeue() as IEvent;
			if (!TriggerEvent(evt))
				Debug.Log("Error when processing event: " + evt.GetName());
			
			if (LimitQueueProcesing)
				timer += Time.deltaTime;
		}
	}
	
	public void OnApplicationQuit()
	{
		m_listenerTable.Clear();
		m_eventQueue.Clear();
		s_Instance = null;
	}
}