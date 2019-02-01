using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FightSEController : MonoBehaviour {
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


	public delegate void OnRecycle(FightSEController gse);

	public OnRecycle onRecycle;

	public delegate void OnExtraUp(FightSEController gse,int idx, int upRatio);

	public OnExtraUp onExtraUp;

	public delegate void OnRecycleDamage(FightSEController gse,int damageIdx ,DamageData damageData, FightItemButton target, Vector3 pos);

	public OnRecycleDamage onRecycleDamage;

	private int upRatio;

    [SerializeField]
    private ParticleSystem extraParticle;

    [SerializeField]
    private Color[] particleColor;


	[SerializeField]
	private TweenPostion damageLight;

	[SerializeField]
	private GameObject damageTxts;

	[SerializeField]
	private TextMeshProUGUI[] damageTxt;
	int damageTxtIdx;

    [SerializeField]
    private Transform lightParant;

	float speedRatio;

	bool isShowDamage = false;

	Color[] attriColor;

	Vector3 tPos;

    int damageIdx;


	// Use this for initialization
	void Start() {
	}

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			if (seType == SpecailEffectType.Attack) {
				damageLight.runFinish = OnRunFinish;
				damageLight.gameObject.SetActive (true);
				damageLight.PlayForward ();
				setComplete = false;
			} else {
				damageTxts.SetActive (true);
				damageTxt[damageTxtIdx].gameObject.SetActive (true);
				if (!isShowDamage) {
                    damageTxt[damageTxtIdx].GetComponent<TweenPostion>().PlayForward(Random.Range(0, 2));
				}

                if (plusDamage != 0)
                {
                    showDamage = (int)DataUtil.LimitFloat(Mathf.CeilToInt(showDamage + Time.deltaTime * plusSpeed), plusDamage, false);
                    damageTxt[damageTxtIdx].text = showDamage.ToString();
                }
				isShowDamage = true;
				if (showDamage >= plusDamage) {
					showTime -= Time.deltaTime;
					if (showTime <= 0) {
                        OnRunFinish();
					}
				}
			}
		}
	}

	public void SetAttackShow(Vector3 orgPos, Vector3 targetPos, int idx ,FightItemButton target, DamageData data){
		SetLightParent(orgPos, targetPos);

		damageData = data;
		callbackTarget = target;
		tPos = targetPos;
        damageIdx = idx;

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

    public void SetDamageShow(int idx, DamageData damageData, Vector3 orgPos)
    {
        showDamage = 0;
        plusDamage = damageData.damage[idx];
        speedRatio = Random.Range(0.5f, 1.2f);
        
        seType = SpecailEffectType.Damage;
        showTime = 0.5f;
        damageIdx = idx;

        if (damageData.damage[idx] == 0)
        {
            damageTxt[damageTxtIdx].text = "Miss";
            plusSpeed = 6 / speedRatio;
        }
        else {
            plusSpeed = damageData.damage[idx] / speedRatio;
        }


        damageTxtIdx = System.Convert.ToInt32(damageData.isCrt[idx]);
        damageTxt[damageTxtIdx].GetComponent<TweenPostion>().SetJump(orgPos, orgPos + Vector3.right * Random.Range(-50, 50), speedRatio);
        damageTxt[damageTxtIdx].color = Const.attriColor[damageData.attributes];
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
            onRecycleDamage.Invoke(this, damageIdx, damageData, callbackTarget, tPos);
			onRecycleDamage = null;
		}
	}

	public void Run(){
		isRun = true;
	}

	public void CloseSE(){
		foreach (TextMeshProUGUI txt in damageTxt) {
			txt.gameObject.SetActive (false);
			txt.color = Color.black;
		}
		damageTxts.SetActive (false);
	}

	public void AllReset(){
		lightParant.rotation = Quaternion.identity;
		lightParant.localPosition = Vector3.zero;
		damageLight.gameObject.SetActive (false);

		damageTxts.SetActive (false);
		foreach (TextMeshProUGUI txt in damageTxt) {
			txt.transform.localPosition = Vector3.zero;
			txt.text = "";
			txt.gameObject.SetActive (false);
		}
	}
}
