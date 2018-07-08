#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTool : MonoBehaviour {
	[MenuItem("GameObject/GroupSelect",false,-10000)]
	public static void GroupSelect(){
		GameObject newGO = new GameObject ();
		newGO.name = "GameObject";
		newGO.transform.parent = Selection.activeGameObject.transform.parent;
		Debug.Log (Selection.activeGameObject.name);
		moveGO (newGO.transform);
		//go.transform.parent = 
		//go.name = "GameObject";

	}

	static void moveGO(Transform newGO){
		foreach (var go in Selection.gameObjects) {
			if (go.transform.parent != newGO.transform) {
				go.transform.parent = newGO.transform;
			}
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
#endif