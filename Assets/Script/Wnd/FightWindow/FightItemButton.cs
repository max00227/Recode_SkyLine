﻿using System.Collections;
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

	bool isLock = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init(){
		isLock = false;
		btn.interactable = true;
	}

	public void SetEnable(bool isOpen, bool dead = false){
		if (isLock == false) {
			btn.interactable = isOpen;
			isLock = dead;
		}
	}

	public void SetRatioTxt(int ratio, bool isShow = false){
		if (isShow) {
			ratioTxt.SetShowUp (ratio);
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
		hpBar.SetBar (hpRatio, true);
		hpBar.OnRun ();
	}

	public void NumberShowRun(){
		ratioTxt.run ();
	}

	public void SetExtra(int upRatio){
		ratioTxt.SetPlus (upRatio);
	}

	public void Callback(){
		onComplete.Invoke ();
	}
}
