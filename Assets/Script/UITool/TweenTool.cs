﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenTool : MonoBehaviour {

	public enum TweenType{
		Position,
		Rotation,
		Scale
	}


	[SerializeField]
	TweenType tweentype; 

	[SerializeField]
	Vector3 from;

	[SerializeField]
	Vector3 to;

	[SerializeField]
	AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

	[SerializeField]
	float TweenTime = 1;

	[SerializeField]
	bool isRoop;

	[SerializeField]
	bool isPopupWnd = false;

	[HideInInspector]
	public GameObject showGameObject;

	bool isRun = false;

	bool runForward = true;

	Vector3 orginV3;

	float oriTime = 0;


	// Use this for initialization
	public void PlayForward(){
		Play (true);
	}

	public void PlayReverse(){
		Play (false);
	}



	void Play (bool isForward) {
		runForward = isForward;
		oriTime = isForward?0:TweenTime;
		isRun = true;
	}

	void Stop(){
		isRun = false;
		if (tweentype == TweenType.Position) {
			transform.localPosition = orginV3 ;
		} 
		else if (tweentype == TweenType.Rotation) {
			transform.Rotate (orginV3);
		}
		else {
			transform.localScale = orginV3;
		}
	}


	void Start(){
		if (tweentype == TweenType.Position) {
			orginV3 = transform.localPosition;
		} 
		else if (tweentype == TweenType.Rotation) {
			orginV3 = transform.localEulerAngles;
		}
		else {
			orginV3 = transform.localScale;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isRun == true) {
			if (isPopupWnd) {
				showGameObject.SetActive (true);
			}
			if (runForward) {
				oriTime = oriTime + Time.deltaTime;
			} else {
				oriTime = oriTime - Time.deltaTime;
			}

			Vector3 deltaV3 = from + (to - from) * animationCurve.Evaluate (oriTime / TweenTime);

			if (tweentype == TweenType.Position) {
				transform.localPosition = deltaV3;
			} 
			else if (tweentype == TweenType.Rotation) {
				transform.Rotate (deltaV3);
			}
			else {
				transform.localScale = deltaV3;
			}

			if (runForward) {
				if (oriTime >= TweenTime) {
					isRun = false;
				}
			}
			else {
				if (oriTime <= 0) {
					if (isPopupWnd) {
						showGameObject.SetActive (false);
					}
					isRun = false;
				}
			}
		}
	}

	//void 
}
