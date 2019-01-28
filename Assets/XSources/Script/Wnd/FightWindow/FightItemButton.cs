using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class FightItemButton : MonoBehaviour {
	public delegate void OnComplete();
	public OnComplete onComplete;

    public Color[] conditionColor;

	[SerializeField]
	FilledBarController hpBar;

	[SerializeField]
	ConditionView[] conditionViews;

    [SerializeField]
	Button btn;

    [SerializeField]
    Outline outline;

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

	/*public void SetRatioTxt(int ratio, bool isShow = false){
		if (isShow) {
			ratioTxt.SetShowUp (ratio);
			ratioTxt.onComplete = Callback;
		} else {
			ratioTxt.SetNumber (ratio);
		}

        ratioTxt.SetNumber(ratio);

    }*/

    public void UnLockButton(){
		isLock = true;
		btn.interactable = true;
	}

	public void SetConditonText(List<int> condition){
        for (int i = 0; i < conditionViews.Length; i++)
        {
            for (int j = 0; j < condition.Count; j++)
            {
                if ((int)conditionViews[i].conditionType == j)
                {
                    {
                        if (condition[j] != 0)
                        {
                            conditionViews[i].conditionText.SetNumber(condition[j]);
                        }
                        else
                        {
                            conditionViews[i].conditionText.gameObject.SetActive(false);
                        }
                    }
                }
            }
            conditionViews[i].minusText.gameObject.SetActive(false);
        }
    }

    public void InitConditonText(List<int> condition, int? level)
    {
        int viewCount = 0;
        for (int i = 0; i < condition.Count; i++)
        {
            if (viewCount <= 1)
            {
                conditionViews[viewCount].minusText.gameObject.SetActive(false);
            }
            if (condition[i] != 0)
            {
                conditionViews[viewCount].conditionType = (ConditionType)Enum.ToObject(typeof(ConditionType), i);
                conditionViews[viewCount].conditionText.gameObject.SetActive(true);
                conditionViews[viewCount].conditionText.SetColor(conditionColor[i]);
                conditionViews[viewCount].conditionText.SetNumber(condition[i]);

                viewCount++;
            }
            else
            {
                if (viewCount <= 1)
                {
                    conditionViews[viewCount].conditionText.gameObject.SetActive(false);
                }
            }
           
        }



        if (level != null)
        {
            outline.effectColor = conditionColor[(int)level - 1];
        }
        else {
            outline.effectColor = Color.black;
        }
    }

    public void SetMinus(int[] minus) {
        for (int i = 0; i < conditionViews.Length; i++)
        {
            for (int j = 0; j < minus.Length; j++)
            {
                if ((int)conditionViews[i].conditionType == j)
                {
                    {
                        if (minus[j] != 0)
                        {
                            conditionViews[i].minusText.gameObject.SetActive(true);
                            conditionViews[i].minusText.SetNumber(minus[j] * -1);
                        }
                        else
                        {
                            conditionViews[i].minusText.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    public void CloseMinus()
    {
        for (int i = 0; i < conditionViews.Length; i++)
        {
            conditionViews[i].minusText.gameObject.SetActive(false);
        }
    }

    public void ResetRatio(){
		//ratioTxt.ResetNumber ();
		//ratioTxt.SetColor (Color.black);
	}

	/// <summary>
	/// 設置血量條.
	/// <param name="hpRatio">Bar條參數</param>
	/// <param name="isShow">效果表現</param>
	/// <param name="isUp">是否上升</param>
	public void SetHpBar(float hpRatio, bool isShow = true, bool isUp = false){
		hpBar.SetBar (hpRatio, isShow, isUp);

		if (isShow) {
			hpBar.OnRun ();
		}
	}

	public void Callback(){
		onComplete.Invoke ();
	}

    [Serializable]
    public struct ConditionView {
        public NumberSetting conditionText;
        public NumberSetting minusText;
        public ConditionType conditionType;
    }

    public enum ConditionType {
        Copper = 0,
        Silver = 1,
        Gold = 2
    }
}
