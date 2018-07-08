using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Task<T> {
	public class Callbacks
	{
		
	}

	public delegate void OnStartTaskDelegate(Callbacks callbacks);
	private Callbacks callbacks;

	Queue<OnStartTaskDelegate> tasks ;

	public Task(){
		
	}
}
