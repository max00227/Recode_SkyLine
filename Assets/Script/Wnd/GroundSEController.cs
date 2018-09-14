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

	bool isRun;

	SpecailEffectType seType;

	int lightRandom;

	int delegateNumber;

	List<int> showNumber;

	int charaIdx;

	Vector3 endPos;

	public delegate void OnRecycle(GroundSEController rg);

	public OnRecycle onRecycle;

	public delegate void OnExtraUp(int idx);

	public OnExtraUp onExtraUp; 

	[SerializeField]
	private TweenPostion extraLight;

	[SerializeField]
	private GameObject damageTxts;

	[SerializeField]
	private TweenPostion damageLight;

	[SerializeField]
	private Text[] damageTxt;


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

						//seGrounds [showedCount].ChangeSprite (showNumber[showedCount]);
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
				extraLight.SetFromAndTo (seGrounds [0].transform.localPosition, endPos);
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

	public void SetExtraSE(List<GroundController> grounds, Vector3 dir, int idxs){
		seGrounds = grounds;

		extraLight.transform.localPosition = dir;

		endPos = dir;

		showedCount = 0;
		showTime = 0;

		seType = SpecailEffectType.ExtraRatio;

		charaIdx = idxs;

		setComplete = true;
		isRun = false;
	}

	public void SetDamageShow(Vector3 org, Vector3 dir){
		seType = SpecailEffectType.Damage;
		damageLight.SetFromAndTo (org, dir);
		setComplete = true;
		isRun = false;
	}


	private void OnRunFinish(TweenPostion tp){
		tp.gameObject.SetActive (false);
		tp.resetPosition ();

		if (onExtraUp != null) {
			onExtraUp.Invoke (charaIdx);
		}

		tp.runFinish = null;
		if (onRecycle != null) {
			onRecycle.Invoke (this);
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
