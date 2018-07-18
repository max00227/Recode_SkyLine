using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReversalGrounds : MonoBehaviour {
   // ReversalGrounds rg;
    List<GroundController> reversalGrounds;
    bool setComplete;
    float reversalTime;
    int reversedCount;
	int plusDamage;
	float showDamage;
	float plusSpeed;
	Text damageTxt;

    public delegate void OnRecycle(ReversalGrounds rg);

    public OnRecycle onRecycle;

    // Use this for initialization
    void Start() {
		damageTxt = GetComponent<Text> ();
    }

    public ReversalGrounds New() {
        return new ReversalGrounds();
    }

	// Update is called once per frame
	void Update () {
        if (setComplete) {
			reversalTime -= Time.deltaTime;
			showDamage = LimitInt (Mathf.CeilToInt (showDamage + Time.deltaTime * plusSpeed), plusDamage);
			damageTxt.text = showDamage.ToString ();
            if (reversedCount < reversalGrounds.Count)
            {
                if (reversalTime <= 0) {
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
			ground.onReversed = OnReversed;
			ground.onReversing = OnReversing;
		}
		reversedCount = 0;
		reversalTime = 0;
		setComplete = true;
	}

	private void OnReversed(GroundController gc){
		if (gc == reversalGrounds [reversalGrounds.Count - 1]) {
			onRecycle.Invoke(this);
		}
	}

	private void OnReversing(int damage){
		plusSpeed = damage * 2;
		plusDamage += damage;
	}

	private int LimitInt(int input, int limit){
		if (input > limit) {
			return limit;
		}
		return input;
	}
}
