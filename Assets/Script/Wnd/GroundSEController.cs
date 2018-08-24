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

	List<Vector3> positions;

	bool isRun;
	TweenTool tweenTool;

	SpecailEffectType seType;

	int lightRandom;

	int delegateNumber;

	List<int> showNumber;

	List<int> charaIdx;

	public delegate void OnRecycle(GroundSEController rg);

	public OnRecycle onRecycle;

	public delegate void OnExtraUp(int idx);

	public OnExtraUp onExtraUp; 

	[SerializeField]
	private TweenTool extraLight;

	[SerializeField]
	private Text damageTxt;

	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			showTime -= Time.deltaTime;
			if ((int)seType == 1) {
				damageTxt.gameObject.SetActive (true);
				showDamage = LimitInt (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage);
				damageTxt.text = "＋" + showDamage.ToString ();
				if (showedCount < seGrounds.Count) {
					if (showTime <= 0) {
						damageTxt.GetComponent<TweenTool>().PlayForward ();

						seGrounds [showedCount].ChangeSprite (showNumber[showedCount]);


						showedCount++;
						showTime = 0.75f;
					}
				} else {
					if (showTime <= 0) {
						setComplete = false;
					}
				}
			} else {
				if (showedCount < positions.Count) {
					if (showTime <= 0) {

						extraLight.runFinish = OnRunFinish;
						extraLight.SetFromAndTo (seGrounds [0].transform.localPosition, positions [showedCount]);
						extraLight.gameObject.SetActive (true);
						extraLight.PlayForward ();
					
						showedCount++;
						showTime = 0.6f;
					}
				}
				else {
					if (showTime <= 0) {
						setComplete = false;
					}
				}
			}
		}
	}

	public void SetReverseSE(List<GroundController> grounds)
	{
		showNumber = new List<int> ();
		plusDamage = 0;
		seGrounds = grounds;

		foreach (var ground in seGrounds) {
			if (ground.onShowedFst == null) {
				ground.onShowedFst = OnShowed;
				ground.onShowingFst = OnShowing;

				showNumber.Add (1);
			} else {
				if (ground.onShowedSec == null) {
					ground.onShowedSec = OnShowed;
					ground.onShowingSec = OnShowing;

					showNumber.Add (2);
				} 
				else {
					ground.onShowedThr = OnShowed;
					ground.onShowingThr = OnShowing;

					showNumber.Add (3);
				}
			}
		}

		showedCount = 0;
		showTime = 0;

		seType = SpecailEffectType.Reverse;



		setComplete = true;
		isRun = false;
	}

	public void SetExtraSE(List<GroundController> grounds, List<Vector3> dir, List<int> idxs){
		seGrounds = grounds;

		positions = dir;

		showedCount = 0;
		showTime = 0;

		seType = SpecailEffectType.ExtraRatio;

		charaIdx = idxs;

		setComplete = true;
		isRun = false;
	}

	private void OnRunFinish(TweenTool tt){
		extraLight.gameObject.SetActive (false);
		extraLight.resetPosition ();
		onExtraUp.Invoke (charaIdx [showedCount - 1]);
		tt.runFinish = null;
		if (showedCount == positions.Count) {
			if (onRecycle != null) {
				onRecycle.Invoke (this);
			}
		}
	}

	private void OnShowed(GroundController gc, int number){
		damageTxt.gameObject.SetActive (false);
		switch(number){
		case 1:
			gc.onShowedFst = null;
			break;
		case 2:
			gc.onShowedSec = null;
			break;
		case 3:
			gc.onShowedThr = null;
			break;
		}

		if (gc == seGrounds [seGrounds.Count - 1] && number == showNumber [showedCount]) {
			if (onRecycle != null) {
				onRecycle.Invoke (this);
			} else {
				Debug.Log ("Null : " + seGrounds [0].name);
			}
		}
	}

	private void OnShowing(int ratio, GroundController gc, int number){
		switch(number){
		case 1:
			gc.onShowingFst = null;
			break;
		case 2:
			gc.onShowingSec = null;
			break;
		case 3:
			gc.onShowingThr = null;
			break;
		}

		if (ratio == 25) {
			plusSpeed = ratio * 2;
		} 
		else {
			plusSpeed = ratio * 4;
		}
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
