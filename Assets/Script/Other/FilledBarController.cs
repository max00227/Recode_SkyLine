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

	float amountSpace;

	public void SetBar(float fillAmount){
		if (changeColor) {
			for (int i = setting.Length-1; i >= 0; i--) {
				if (fillAmount <=  setting[i].section) {
					bar.color = setting [i].barColor;
				}
			}

		}
		bar.fillAmount = fillAmount;
	}

	[Serializable]
	struct BarSetting{
		[Range(0,1)]
		public float section;
		public Color barColor;
	}
}
