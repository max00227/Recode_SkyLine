using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatioSetting : MonoBehaviour {
	[SerializeField]
	Text text;

	int showRatio = 0;

	int prevRatio;

	float upSpeed = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetRatio(int ratio){
		upSpeed = (ratio - showRatio) / 1.5f;
		if (upSpeed > 0) {
			Debug.Log (upSpeed);
		}
		prevRatio = showRatio;
		showRatio = ratio;

		text.text = showRatio.ToString();
	}

	public void SetExtra(){
		showRatio = showRatio + 25;
		upSpeed = 25 * 4;

		text.text = showRatio.ToString();
	}

	public void SetColor(Color color){
		text.color = color;
	}
}
