using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatioSetting : MonoBehaviour {
	[SerializeField]
	Text text;

	int showRatio = 0;

	int prevRatio;

	float upSpeed = 0;

	bool isRun;

	public delegate void OnComplete();

	public OnComplete onComplete;

	public bool isSet = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (isSet) {
			if (isRun) {
				if (prevRatio < showRatio) {
					prevRatio = DataUtil.LimitInt (Mathf.CeilToInt (prevRatio + upSpeed * Time.deltaTime), showRatio);
					text.text = prevRatio.ToString ();
				} 
				else {
					text.text = showRatio.ToString ();
					isRun = false;
					isSet = false;
					prevRatio = showRatio;
					if (onComplete != null) {
						onComplete.Invoke ();
						onComplete = null;
					}
				}
			}
		} 
		else {
			isRun = false;
		}
	}

	public void SetRatio (int ratio) {
		text.text = ratio.ToString();
	}

	public void SetShowUp(int ratio) {
		isRun = false;
		upSpeed = (ratio - showRatio) / 0.5f;

		prevRatio = showRatio;

		showRatio = ratio;
		isSet = true;
	}

	public void SetExtra(){
		showRatio = showRatio + 25;

		text.text = showRatio.ToString();
	}

	public void ResetRatio () {
		showRatio = 0;
		text.text = showRatio.ToString();

	}

	public void SetColor (Color color) {
		text.color = color;
	}

	public void run(){
		isRun = true;
	}
}
