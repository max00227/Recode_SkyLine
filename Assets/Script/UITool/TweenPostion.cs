using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenPostion : MonoBehaviour {

	public Vector3 from;

	public Vector3 to;

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
	float recTime;

	public delegate void RunFinish(TweenPostion tt);

	public RunFinish runFinish;

	// Use this for initialization
	public void PlayForward(){
		Play (true);
	}

	public void PlayReverse(){
		Play (false);
	}



	void Play (bool isForward) {
		runForward = isForward;
		recTime = Time.realtimeSinceStartup;
		isRun = true;
	}

	void Stop(){
		isRun = false;
			
		transform.localPosition = orginV3 ;

	}


	void Start(){
		orginV3 = transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (isRun == true) {
			if (isPopupWnd) {
				showGameObject.SetActive (true);
			}
			if (runForward) {
				oriTime = Time.realtimeSinceStartup - recTime;
			} else {
				oriTime = TweenTime - (Time.realtimeSinceStartup - recTime);
			}

			Vector3 deltaV3 = from + (to - from) * animationCurve.Evaluate (oriTime / TweenTime);

			transform.localPosition = deltaV3;


			if (runForward) {
				if (oriTime >= TweenTime) {
					isRun = false;

					if (runFinish != null) {
						runFinish.Invoke (this);
					}
				}
			}
			else {
				if (oriTime <= 0) {
					if (isPopupWnd) {
						showGameObject.SetActive (false);
					}
					isRun = false;
					if (runFinish != null) {
						runFinish.Invoke (this);
					}
				}
			}
		}
	}

	public void SetFromAndTo(Vector3 f, Vector3 t){
		from = f;
		to = t;
		transform.localPosition = f;
	}


	public void resetPosition(){
		transform.localPosition = from;
	} 
}
