using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenTool : MonoBehaviour {


	[SerializeField]
	Vector2 from;

	[SerializeField]
	Vector2 to;

	[SerializeField]
	AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

	[SerializeField]
	float TweenTime = 1;

	[SerializeField]
	bool isPopupWnd=true;

	public GameObject showGameObject;

	bool isRun = false;

	bool runForward = true;

	Vector2 orginPos;

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

			transform.localPosition = from + (to - from) * animationCurve.Evaluate (oriTime / TweenTime);

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
