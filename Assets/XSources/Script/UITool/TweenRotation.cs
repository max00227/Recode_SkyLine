using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenRotation : TweenUI {

	public Vector3 from;

	public Vector3 to;


	[SerializeField]
	bool isPopupWnd = false;


	Vector3 orginV3;

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
            if (delayFinish)
            {
                if (isPopup && popupGo != null)
                {
                    popupGo.SetActive(true);
                }
                CalOriTime();

                Vector3 deltaV3 = from + (to - from) * mainAniCurve.Evaluate(oriTime / TweenTime);

                transform.rotation = Quaternion.Euler(deltaV3);


                if (runForward)
                {
                    if (oriTime >= TweenTime)
                    {
                        if (!isLoop)
                        {
                            TweenEnd();
                        }
                        else
                        {
                            ResetRecTime();
                        }
                    }
                }
                else
                {
                    if (oriTime <= 0)
                    {
                        if (!isLoop)
                        {
                            TweenEnd();
                        }
                        else
                        {
                            ResetRecTime();
                        }
                    }
                }
            }
            else {
                oriTime = Time.realtimeSinceStartup - recTime;
                if (oriTime >= delay)
                {
                    recTime = Time.realtimeSinceStartup;
                    delayFinish = true;
                }
            }
        }
	}

	public void SetFromAndTo(Vector3 f, Vector3 t){
		from = f;
		to = t;
	}


    public override void Reset()
    {
        transform.Rotate (from);
        if (isPopup && popupGo != null)
        {
            popupGo.SetActive(false);
        }
    }
}
