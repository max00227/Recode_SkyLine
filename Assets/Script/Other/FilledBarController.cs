using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FilledBarController : MonoBehaviour {
	[SerializeField]
	Image bar;

	[SerializeField]
	BarSetting[] setting;

	[SerializeField]
	bool changeColor = false;

	[SerializeField]
	bool isReturnMin = false;

	[SerializeField]
	float speedRatio;

	public delegate void OnComplete();
	public OnComplete onComplete;

	float amountSpace;
	float upSpeed;
	float final;
	bool isSet;
	bool isRun;

	void Update () {
		if (isSet) {
			if (isRun) {
				bar.fillAmount = DataUtil.LimitFloat (bar.fillAmount + upSpeed * Time.deltaTime, final, isReturnMin);
				if (changeColor) {
					for (int i = setting.Length - 1; i >= 0; i--) {
						if (bar.fillAmount <= setting [i].section) {
							bar.color = setting [i].barColor;
						}
					}
				}
				if (!isReturnMin) {
					if (bar.fillAmount >= final) {
						Complete ();
						return;
					}
				} 
				else {
					if (bar.fillAmount <= final) {
						Complete ();
						return;
					}
				}
			}
		} 
		else {
			isRun = false;
		}
	}

	private void Complete(){
		isRun = false;
		isSet = false;
		bar.fillAmount = final;
		if (onComplete != null) {
			onComplete.Invoke ();
			onComplete = null;
		}
	}

	public void SetBar(float fillAmount, bool isShow = false, bool isUp = true){
		if (isShow = false) {
			if (changeColor) {
				for (int i = setting.Length - 1; i >= 0; i--) {
					if (fillAmount <= setting [i].section) {
						bar.color = setting [i].barColor;
					}
				}

			}
			isSet = false;

			bar.fillAmount = fillAmount;
		} 
		else {
            if (isSet && isRun)
            {
                if (isUp) {
                    if (fillAmount >= final)
                    {
                        return;
                    }
                    else {
                        final = fillAmount;
                    }
                }
                else
                {
                    if (fillAmount <= final)
                    {
                        return;
                    }
                    else
                    {
                        final = fillAmount;
                    }
                }
            }
            else
            {
                final = fillAmount;
            }
			upSpeed = (fillAmount - bar.fillAmount) / speedRatio;
			isSet = true;
			isRun = false;
		}
	}

	public void OnRun(){
		isRun = true;
	}

	[Serializable]
	struct BarSetting{
		[Range(0,1)]
		public float section;
		public Color barColor;
	}
}
