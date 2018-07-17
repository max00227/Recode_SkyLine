using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineConnecter : MonoBehaviour {
	private bool setComplete = false;

	private float dist;

	private RectTransform rectTransform;

	private float connectTime;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (setComplete) {
			if (rectTransform.rect.height < dist) {
				connectTime += Time.deltaTime;
				rectTransform.sizeDelta = new Vector2 (rectTransform.rect.width, connectTime * 2328);
			} else {
				rectTransform.sizeDelta = new Vector2 (rectTransform.rect.width, dist);
				setComplete = false;
				connectTime = 0;
			}
		}
	}

	public void SetConnect(Vector3 start, Vector3 end){
		rectTransform = GetComponent<RectTransform>();
		rectTransform.Rotate (Vector3.back * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg));
		//GetComponent<RectTransform>().Rotate (Vector3.back * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg));
		dist = Vector3.Distance (start, end);

		connectTime = 0;
		setComplete = true;
		//transform.localRotation = Vector3.forward * (Mathf.Atan2 (end.x - start.x, end.y - start.y) * Mathf.Rad2Deg);
	}
}
