using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineConnecter : MonoBehaviour {
	private bool setComplete = false;

	private float dist;

	private RectTransform rectTransform;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (setComplete) {
			//GetComponent<RectTransform> ().sizeDelta = new Vector2 (GetComponent<RectTransform> ().rect.width, dist);
		}
	}

	public void SetConnect(Vector3 start, Vector3 end){
		rectTransform = GetComponent<RectTransform>();
		rectTransform.Rotate (Vector3.back * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg));
		//GetComponent<RectTransform>().Rotate (Vector3.back * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg));
		//dist = Vector3.Distance (start, end);

		setComplete = true;
		//transform.localRotation = Vector3.forward * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg);
	}
}
