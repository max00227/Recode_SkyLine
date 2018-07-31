using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundSEController : MonoBehaviour {
	[HideInInspector]
	public List<GroundController> seGrounds;
	bool setComplete;
	float showTime;
	int showedCount;
	int plusDamage;
	float showDamage;
	float plusSpeed;
	Text damageTxt;
	bool isRun;
	TweenTool tweenTool;

	int lightDir;

	int lightJob;

	public delegate void OnRecycle(GroundSEController rg);

	public OnRecycle onRecycle;

	// Use this for initialization
	void Start() {
		damageTxt = GetComponent<Text> ();
		tweenTool = GetComponent<TweenTool> ();
	}

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			showTime -= Time.deltaTime;
			showDamage = LimitInt (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage);
			damageTxt.text = "＋"+showDamage.ToString ();
			if (showedCount < seGrounds.Count)
			{
				if (showTime <= 0) {
					tweenTool.PlayForward ();

					if (lightDir == 0) {
						seGrounds [showedCount].ChangeSprite ();
					} 
					else {
						//seGrounds [reversedCount].OnLight (lightDir, lightJob);
					}

					showedCount++;
					showTime = 0.75f;
				}
			}
			else {
				if (showTime <= 0) {
					setComplete = false;
				}
			}
		}
	}

	public void SetGroundSE(List<GroundController> grounds, int dir = 0, int job = 0)
	{
		plusDamage = 0;
		seGrounds = grounds;
		foreach (var ground in seGrounds) {
			ground.onShowed = OnShowed;
			ground.onShowing = OnShowing;
		}
		showedCount = 0;
		showTime = 0;
		lightDir = dir;
		lightJob = job;

		setComplete = true;
		isRun = false;
	}

	private void OnShowed(GroundController gc){
		gc.onShowed = null;
		if (gc == seGrounds [seGrounds.Count - 1]) {
			if (onRecycle != null) {
				onRecycle.Invoke (this);
			}
		}
	}

	private void OnShowing(int ratio, GroundController gc){
		gc.onShowing = null;
		plusSpeed = ratio * 2;
		plusDamage += ratio;
	}

	private int LimitInt(int input, int limit){
		if (input > limit) {
			return limit;
		}
		return input;
	}

	public void Run(){
		isRun = true;
	}
}
