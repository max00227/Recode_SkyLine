using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T _instance = null;
	public static T Instance
	{
		get
		{
			// Instance requiered for the first time, we look for it
			if( _instance == null )
			{
				_instance = GameObject.FindObjectOfType (typeof(T)) as T;

				if (_instance == null)
				{
					throw new Exception("No instance of " + typeof(T).ToString());
				}

				if (!isInitialized)
				{
					isInitialized = true;
					_instance.Init ();
				}
			}

			return _instance;
		}
	}

	private static bool isInitialized;

	// If no other monobehaviour request the instance in an awake function
	// executing before this one, no need to search the object.
	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
		}
		else if (_instance != this)
		{
			Debug.LogError ("Another instance of " + GetType () + " is already exist! Destroying self...");
			DestroyImmediate (this);
			return;
		}

		if (!isInitialized)
		{
			isInitialized = true;
			_instance.Init();
		}
	}


	/// <summary>
	/// This function is called when the instance is used the first time
	/// Put all the initializations you need here, as you would do in Awake
	/// </summary>
	public virtual void Init()
	{
	}

	/// Make sure the instance isn't referenced anymore when the user quit, just in case.
	private void OnApplicationQuit()
	{
		_instance = null;
	}
}