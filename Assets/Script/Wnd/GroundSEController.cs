﻿using System.Collections;
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
	float hpRatio;
	FightItemButton callbackTarget;

	bool isRun;

	SpecailEffectType seType;

	int lightRandom;

	int delegateNumber;

	List<int> showNumber;

	int charaIdx;


	public delegate void OnRecycle(GroundSEController rg);

	public OnRecycle onRecycle;

	public delegate void OnExtraUp(int idx, int upRatio);

	public OnExtraUp onExtraUp;

	public delegate void OnRecycleDamage(GroundSEController rg, float ratio, FightItemButton target);

	public OnRecycleDamage onRecycleDamage;

	private int upRatio;

	[SerializeField]
	private TweenPostion extraLight;

	[SerializeField]
	private GameObject damageTxts;

	[SerializeField]
	private TweenPostion damageLight;

	[SerializeField]
	private Text[] damageTxt;

    [SerializeField]
    private Transform lightParant;


	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			showTime -= Time.deltaTime;
			if ((int)seType == 1) {
				damageTxts.SetActive (true);
				showDamage = DataUtil.LimitInt (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage);
				foreach (Text txt in damageTxt) {
					txt.text = "＋" + showDamage.ToString ();
				}
				if (showedCount < seGrounds.Count) {
					if (showTime <= 0) {
						foreach (Text txt in damageTxt) {
							txt.GetComponent<TweenScale> ().PlayForward ();
						}

						StartCoroutine (seGrounds [showedCount].ChangeSpriteWait (showNumber [showedCount]));


						showedCount++;
						showTime = 0.6f;
					}
				} else {
					if (showTime <= 0) {
						setComplete = false;
					}
				}
			} 
			else if ((int)seType == 2) {
				extraLight.runFinish = OnRunFinish;
				extraLight.gameObject.SetActive (true);
				extraLight.PlayForward ();
				setComplete = false;
			} 
			else {
				damageLight.runFinish = OnRunFinish;
				damageLight.gameObject.SetActive (true);
				damageLight.PlayForward ();
				setComplete = false;
			}
		}
	}

	public void SetReverseSE(List<GroundController> grounds, List<Vector3> positions)
	{
		showNumber = new List<int> ();
		plusDamage = 0;
		seGrounds = grounds;

		for (int i = 0; i < positions.Count; i++) {
			damageTxt [i].gameObject.SetActive (true);
			damageTxt [i].transform.localPosition = positions [i];
		}

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

	/// <summary>
	/// Sets the extra S.
	/// </summary>
	/// <param name="grounds">原點</param>
	/// <param name="dir">方向</param>
	/// <param name="idxs">角色編號</param>
	/// <param name="upInt">上升值</param>
	public void SetExtraSE(List<GroundController> grounds, Vector3 dir, int idxs, int upInt){
        extraLight.SetParabola(grounds[0].transform.localPosition, dir);

        SetLightParent(grounds[0].transform.localPosition, dir);

		upRatio = upInt;

		showedCount = 0;
		showTime = 0;

		seType = SpecailEffectType.ExtraRatio;

		charaIdx = idxs;

		setComplete = true;
		isRun = false;
	}

	public void SetDamageShow(Vector3 org, FightItemButton target, float ratio){
		SetLightParent(org, target.transform.localPosition);

		hpRatio = ratio;
		callbackTarget = target;

        seType = SpecailEffectType.Damage;
		damageLight.SetParabola (org, target.transform.localPosition);
		setComplete = true;
		isRun = false;
	}

    public void SetLightParent(Vector3 f, Vector3 t) {
        lightParant.localPosition = f;
		Vector3 relativePos = f - t;
		float angle = Quaternion.LookRotation (relativePos).eulerAngles.x;
		if (relativePos.x > 0) {
			lightParant.rotation = Quaternion.Euler (0, 0, 180 - angle);
		} 
		else {
			lightParant.rotation = Quaternion.Euler (0, 0, angle - 360);
		}
    }


	private void OnRunFinish(TweenPostion tp){
        lightParant.rotation = Quaternion.identity;
        lightParant.localPosition = Vector3.zero;
		tp.gameObject.SetActive (false);
		tp.resetPosition ();

		if (onExtraUp != null) {
			onExtraUp.Invoke (charaIdx, upRatio);
		}

		tp.runFinish = null;
		if (onRecycle != null) {
			onRecycle.Invoke (this);
		}

		if (onRecycleDamage != null) {
			onRecycleDamage.Invoke (this, hpRatio, callbackTarget);
		}
	}

	private void OnShowed(GroundController gc, int number){
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

		if (gc == seGrounds [seGrounds.Count - 1]) {
			if (number == showNumber [showedCount-1]) {
				if (onRecycle != null) {
					onRecycle.Invoke (this);
				} else {
					Debug.Log ("Null : " + seGrounds [0].name);
				}
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

	public void Run(){
		isRun = true;
	}

	public void CloseSE(){
		extraLight.gameObject.SetActive (false);
		foreach (Text txt in damageTxt) {
			txt.gameObject.SetActive (false);
		}
		damageTxts.SetActive (false);
	}
}
