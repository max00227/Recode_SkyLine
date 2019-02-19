using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenScale : TweenUI {

	public Vector3 from;

	public Vector3 to;

	[SerializeField]
	bool isRoop;

	Vector3 orginV3;

    void Stop(){
		isRun = false;
		transform.localScale = orginV3;
	}


	void Start(){
		orginV3 = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if (isRun == true) {
            if (isPopup && popupGo != null)
            {
                popupGo.SetActive (true);
			}
            CalOriTime();

            Vector3 deltaV3 = from + (to - from) * mainAniCurve.Evaluate (oriTime / TweenTime);

			transform.localScale = deltaV3;

			if (runForward) {
				if (oriTime >= TweenTime) {
                    TweenEnd();
				}
			}
			else {
				if (oriTime <= 0) {
                    if (isPopup && popupGo != null){
                        popupGo.SetActive(false);
                    }
                    TweenEnd();
                }
            }
		}
	}

	public void SetFromAndTo(Vector3 f, Vector3 t){
		from = f;
		to = t;
    }


	public override void Reset(){
		transform.localScale = from;
        if (isPopup && popupGo != null)
        {
            popupGo.SetActive(false);
        }
    } 
}
