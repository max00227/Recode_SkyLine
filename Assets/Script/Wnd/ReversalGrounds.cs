using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReversalGrounds : MonoBehaviour {
   // ReversalGrounds rg;
	[HideInInspector]
	public List<GroundController> reversalGrounds;
    bool setComplete;
    float reversalTime;
    int reversedCount;
	int plusDamage;
	float showDamage;
	float plusSpeed;
	Text damageTxt;
	bool isRun;
	TweenTool tweenTool;

    public delegate void OnRecycle(ReversalGrounds rg);

    public OnRecycle onRecycle;

    // Use this for initialization
    void Start() {
		damageTxt = GetComponent<Text> ();
		tweenTool = GetComponent<TweenTool> ();
    }

    public ReversalGrounds New() {
        return new ReversalGrounds();
    }

	// Update is called once per frame
	void Update () {
		if (setComplete && isRun) {
			reversalTime -= Time.deltaTime;
			showDamage = LimitInt (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage);
			damageTxt.text = "＋"+showDamage.ToString ();
            if (reversedCount < reversalGrounds.Count)
            {
                if (reversalTime <= 0) {
					tweenTool.PlayForward ();
                    reversalGrounds[reversedCount].ChangeSprite();

                    reversedCount++;
					reversalTime = 0.75f;
                }
            }
            else {
				if (reversalTime <= 0) {
					setComplete = false;
				}
            }
        }
	}

	public void SetReversal(List<GroundController> grounds)
	{
		plusDamage = 0;
		reversalGrounds = grounds;
		foreach (var ground in reversalGrounds) {
			ground.onReversed += OnReversed;
			ground.onReversing += OnReversing;
		}
		reversedCount = 0;
		reversalTime = 0;
		setComplete = true;
		isRun = false;
	}

	private void OnReversed(GroundController gc){
		gc.onReversed -= OnReversed;
		if (gc == reversalGrounds [reversalGrounds.Count - 1]) {
			if (onRecycle != null) {
				onRecycle.Invoke (this);
			}
		}
	}

	private void OnReversing(int ratio, GroundController gc){
		gc.onReversing -= OnReversing;
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
