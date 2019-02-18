using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenRotation : TweenUI {

	public Vector3 from;

	public Vector3 to;


	[SerializeField]
	bool isPopupWnd = false;


	Vector3 orginV3;

	public delegate void RunFinish(TweenRotation tt);

	public RunFinish runFinish;

	void Stop(){
		isRun = false;
		transform.Rotate (orginV3);
	}


	void Start(){
		orginV3 = transform.localEulerAngles;
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

			Vector3 deltaV3 = from + (to - from) * mainAniCurve.Evaluate (oriTime / TweenTime);

            transform.rotation = Quaternion.Euler(deltaV3);
            

            if (runForward)
            {
                if (oriTime >= TweenTime)
                {
                    if (!isLoop)
                    {
                        isRun = false;

                        if (runFinish != null)
                        {
                            runFinish.Invoke(this);
                        }
                    }
                    else
                    {
                        recTime = Time.realtimeSinceStartup;
                    }
                }
            }
            else
            {
                if (oriTime <= 0)
                {
                    if (!isLoop)
                    {
                        isRun = false;
                        if (runFinish != null)
                        {
                            runFinish.Invoke(this);
                        }
                    }
                    else
                    {
                        recTime = Time.realtimeSinceStartup;
                    }
                }
            }
		}
	}

	public void SetFromAndTo(Vector3 f, Vector3 t){
		from = f;
		to = t;
	}


	public void resetPosition(){
		transform.Rotate (from);
	} 
}
