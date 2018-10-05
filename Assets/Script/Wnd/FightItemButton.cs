using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FightItemButton : MonoBehaviour {
	public delegate void OnComplete();
	public OnComplete onComplete;


	[SerializeField]
	FilledBarController hpBar;

	[SerializeField]
	NumberSetting ratioTxt;

	[SerializeField]
	Button btn;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetEnable(bool isOpen){
		btn.enabled = isOpen;
	}

	public void SetRatioTxt(int ratio, bool isShow = false){
		if (isShow) {
			Debug.Log (this.name + " : " + ratio);
			ratioTxt.SetShowUp (ratio, 0.5f);
			ratioTxt.onComplete = Callback;
		} else {
			ratioTxt.SetNumber (ratio);
		}
	}

	public void SetTextColor(Color color){
		ratioTxt.SetColor (color);
	}

	public void ResetRatio(){
		ratioTxt.ResetNumber ();
		ratioTxt.SetColor (Color.black);
	}

	public void SetHpBar(float hpRatio){
		hpBar.SetBar (hpRatio);
	}

	public void NumberShowRun(){
		ratioTxt.run ();
	}

	public void SetExtra(int upRatio){
		Debug.Log (this.name + " : " + upRatio);
		ratioTxt.SetPlus (upRatio);
	}

	public void Callback(){
		onComplete.Invoke ();
	}
}
