using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberSetting : MonoBehaviour {
	[SerializeField]
	Text text;

	int showRatio = 0;

	int prevRatio;

	float upSpeed = 0;

	bool isRun;

	[SerializeField]
	float speed = 0;

	public delegate void OnComplete();
	public OnComplete onComplete;

	bool isSet = false;

	[SerializeField]
	bool isReturnMin = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (isSet) {
			if (isRun) {
				if (!isReturnMin) {
					if (prevRatio < showRatio) {
						prevRatio = (int)DataUtil.LimitFloat (Mathf.CeilToInt (prevRatio + upSpeed * Time.deltaTime), showRatio, isReturnMin);
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
				else {
					if (prevRatio > showRatio) {
						prevRatio = (int)DataUtil.LimitFloat (Mathf.CeilToInt (prevRatio + upSpeed * Time.deltaTime), showRatio, isReturnMin);
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
		} 
		else {
			isRun = false;
		}
	}

	public void SetNumber (int number) {
		text.text = number.ToString();
	}

	public void SetShowUp(int number) {
		isRun = false;
		upSpeed = (number - showRatio) / speed;

		prevRatio = showRatio;

		showRatio = number;
		isSet = true;
	}

	public void SetPlus(int number){
		showRatio = showRatio + number;

		text.text = showRatio.ToString();
	}

	public void ResetNumber () {
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
