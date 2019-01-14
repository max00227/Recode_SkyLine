using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TweenPostion : TweenUI {

	public Vector3 from;

	public Vector3 to;

	[SerializeField]
	bool isRoop;

	[SerializeField]
	bool isPopupWnd = false;

    public enum TweenType{
		Normal,
		Parabola,
		Jump
	}
    [SerializeField]
	TweenType tweenType;

	Vector3 orginV3;



    //float parabolaPower;

    float distance;

	[SerializeField]
	PowerRange powerRange;

	float parabolaPower;

    public delegate void RunFinish(TweenPostion tt);

	public RunFinish runFinish;

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

            Vector3 deltaV3;
			if (tweenType == TweenType.Normal)
            {
                deltaV3 = from + (to - from) * mainAniCurve.Evaluate(oriTime / TweenTime);
            }
			else if (tweenType == TweenType.Jump) {
				deltaV3 = new Vector3 (from.x + (to.x - from.x) * oriTime / TweenTime, from.y + parabolaPower * mainAniCurve.Evaluate (oriTime / TweenTime), from.z);
			}
            else {
                deltaV3 = new Vector3(from.x + (distance * (oriTime / TweenTime)), parabolaPower * mainAniCurve.Evaluate(oriTime / TweenTime), from.z);
            }


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

	public void SetFromAndTo(Vector3 f, Vector3 t, int acIdx = 0)
    {
        mainAniCurve = animationCurves[acIdx];

        from = f;
		to = t;
		transform.localPosition = f;
	}

	public void SetParabola(Vector3 f, Vector3 t, int acIdx = 0) {
        mainAniCurve = animationCurves[acIdx];

        from = Vector3.zero;
        transform.localPosition = f;

        distance = Vector3.Distance(f, t);
        to = Vector3.forward * distance;


		int isUp = (int)Mathf.Pow (-1, UnityEngine.Random.Range (0, 2));

		parabolaPower = UnityEngine.Random.Range (powerRange.min, powerRange.max) * isUp;
    }

	public void SetJump(Vector3 f, Vector3 t, float speed, int acIdx = 0)
    {
		SetFromAndTo (f, t, acIdx);
		TweenTime = speed;
		distance = Vector3.Distance(f, t);
		parabolaPower = UnityEngine.Random.Range (powerRange.min, powerRange.max);
	}




    public void resetPosition(){
		transform.localPosition = from;
	} 

	[Serializable]
	struct PowerRange{
		public int min;
		public int max;
	}
}
