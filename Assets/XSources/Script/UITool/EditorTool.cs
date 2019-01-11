#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

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

	[MenuItem("MyProject/TestAny")]
	public static void TestRemove(){
		//List<int> intList = new List<int> (new int[1]{5});
		Debug.Log (new List<int>(new int[]{4})[0]);
		/*List<int> exclub = new List<int> ();
		exclub.Add (3);

		List<int> intList = new List<int> ();
		for (int i = 0; i < 5; i++) {
			if (i != 0) {
				intList.Add (i);
			}
		}

		foreach (int i in ExclubeList(intList,exclub)) {
			Debug.Log (i);
		}*/
	}

	private static List<int> ExclubeList(List<int> intList, List<int>exclub){
		List<int> checkList = intList;
		for (int i = 0; i < checkList.Count; i++) {
			if (exclub.Contains (checkList [i])) {
				checkList.Remove (checkList [i]);
			}
		}

		return checkList;
	}
}
#endif