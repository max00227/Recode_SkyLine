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
	int showDamage;
	float plusSpeed;
	DamageData damageData;
	FightItemButton callbackTarget;

	bool isRun;

	SpecailEffectType seType;

	int lightRandom;

	int delegateNumber;

	List<int> showNumber;

	int charaIdx;


	public delegate void OnRecycle(GroundSEController gse);

	public OnRecycle onRecycle;

	public delegate void OnExtraUp(GroundSEController gse,int idx, int upRatio);

	public OnExtraUp onExtraUp;

	public delegate void OnRecycleDamage(GroundSEController gse, DamageData damageData, FightItemButton target, Vector3 pos);

	public OnRecycleDamage onRecycleDamage;

	private int upRatio;

	[SerializeField]
	private TweenPostion extraLight;

	[SerializeField]
	private GameObject ratioTxts;

	[SerializeField]
	private TweenPostion damageLight;

	[SerializeField]
	private Text[] ratioTxt;

	[SerializeField]
	private GameObject damageTxts;

	[SerializeField]
	private Text[] damageTxt;
	int damageTxtIdx;

    [SerializeField]
    private Transform lightParant;

	float speedRatio;

	bool isShowDamage = false;

	Color[] attriColor;

	Vector3 tPos;


	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			if ((int)seType == 1) {
				showTime -= Time.deltaTime;

				ratioTxts.SetActive (true);
				showDamage = (int)DataUtil.LimitFloat (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage, false);
				foreach (Text txt in ratioTxt) {
					txt.text = "＋" + showDamage.ToString ();
				}

				if (showedCount < seGrounds.Count) {
					if (showTime <= 0) {
						foreach (Text txt in ratioTxt) {
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
			} else if ((int)seType == 2) {
				extraLight.runFinish = OnRunFinish;
				extraLight.gameObject.SetActive (true);
				extraLight.PlayForward ();
				setComplete = false;
			} else if ((int)seType == 3) {
				damageLight.runFinish = OnRunFinish;
				damageLight.gameObject.SetActive (true);
				damageLight.PlayForward ();
				setComplete = false;
			} else {
				damageTxts.SetActive (true);
				damageTxt[damageTxtIdx].gameObject.SetActive (true);
				if (!isShowDamage) {
					damageTxt [damageTxtIdx].GetComponent<TweenPostion> ().PlayForward ();
				}
				showDamage = (int)DataUtil.LimitFloat (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage, false);
				damageTxt[damageTxtIdx].text = showDamage.ToString ();
				isShowDamage = true;
				if (showDamage >= plusDamage) {
					showTime -= Time.deltaTime;
					if (showTime <= 0) {
						OnRunFinish ();
					}
				}
			}
		}
	}

	public void SetReverseSE(List<GroundController> grounds, List<Vector3> positions)
	{
		showNumber = new List<int> ();
		plusDamage = 0;
		seGrounds = grounds;

		for (int i = 0; i < positions.Count; i++) {
			ratioTxt [i].gameObject.SetActive (true);
			ratioTxt [i].transform.localPosition = positions [i];
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
		extraLight.SetParabola (grounds [0].transform.localPosition, dir);

        SetLightParent(grounds[0].transform.localPosition, dir);

		upRatio = upInt;

		showedCount = 0;
		showTime = 0;

		seType = SpecailEffectType.ExtraRatio;

		charaIdx = idxs;

		setComplete = true;
		isRun = false;
	}

	public void SetAttackShow(Vector3 orgPos, Vector3 targetPos, FightItemButton target, DamageData data){
		SetLightParent(orgPos, targetPos);

		damageData = data;
		callbackTarget = target;
		tPos = targetPos;

        seType = SpecailEffectType.Attack;
		damageLight.SetParabola (orgPos, targetPos);
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

	public void SetDamageShow(DamageData damageData, Vector3 orgPos){
		showDamage = 0;
		plusDamage = damageData.damage;
 		speedRatio = Random.Range (0.5f, 1.2f);
		plusSpeed = damageData.damage / speedRatio;
		seType = SpecailEffectType.Damage;
		showTime = 0.5f;

		damageTxtIdx = System.Convert.ToInt32 (damageData.isCrt);
		damageTxt [damageTxtIdx].GetComponent<TweenPostion> ().SetJump (orgPos, orgPos + Vector3.right * Random.Range (-50, 50), speedRatio);
		damageTxt [damageTxtIdx].color = Const.attriColor [damageData.attributes];
		setComplete = true;
		isRun = false;
		isShowDamage = false;
	}


	private void OnRunFinish(TweenPostion tp = null){
		AllReset ();

		if (tp != null) {
			tp.gameObject.SetActive (false);
			tp.resetPosition ();
			tp.runFinish = null;
		}

		if (onExtraUp != null) {
			onExtraUp.Invoke (this, charaIdx, upRatio);
			onExtraUp = null;
		}

		if (onRecycle != null) {
			onRecycle.Invoke (this);
			onRecycle = null;
		}

		if (onRecycleDamage != null) {
			onRecycleDamage.Invoke (this, damageData, callbackTarget, tPos);
			onRecycleDamage = null;
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
		foreach (Text txt in ratioTxt) {
			txt.gameObject.SetActive (false);
		}
		ratioTxts.SetActive (false);

		foreach (Text txt in damageTxt) {
			txt.gameObject.SetActive (false);
			txt.color = Color.black;
		}
		damageTxts.SetActive (false);
	}

	public void AllReset(){
		lightParant.rotation = Quaternion.identity;
		lightParant.localPosition = Vector3.zero;
		extraLight.gameObject.SetActive (false);
		damageLight.gameObject.SetActive (false);

		damageTxts.SetActive (false);
		foreach (Text txt in damageTxt) {
			txt.transform.localPosition = Vector3.zero;
			txt.text = "";
			txt.gameObject.SetActive (false);
		}
	}
}
