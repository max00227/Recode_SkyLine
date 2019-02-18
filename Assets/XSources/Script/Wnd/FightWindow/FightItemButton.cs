using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

[RequireComponent(typeof(Button))]
public class FightItemButton : MonoBehaviour {
	public delegate void OnComplete();
	public OnComplete onComplete;

    public enum ButtonFaction {
        Player,
        Enemy
    }

    public TweenColor[] levelLight;

    public Image board;

    public TMP_FontAsset[] conditionFonts;

    public ButtonFaction buttonFaction;

    public Color[] conditionColor;

    bool[] lightOpen = new bool[3];

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

	public void SetConditonText(int[] condition){
        for (int i = 0; i < conditionViews.Length; i++)
        {
            for (int j = 0; j < condition.Length; j++)
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

    public void InitConditonText(int[] condition, int? level)
    {
        int viewCount = 0;
        if (condition.Sum()==0)
        {
            foreach (ConditionView cv in conditionViews) {
                cv.conditionText.gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < condition.Length; i++)
        {
            if (viewCount <= 1)
            {
                conditionViews[viewCount].minusText.gameObject.SetActive(false);
            }
            if (condition[i] != 0)
            {
                conditionViews[viewCount].conditionType = (ConditionType)Enum.ToObject(typeof(ConditionType), i);
                conditionViews[viewCount].conditionText.gameObject.SetActive(true);
                conditionViews[viewCount].conditionText.SetFont(conditionFonts[i]);
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
            levelLight[(int)level - 1].PlayForward();
            for(int i = 0; i < lightOpen.Length; i++)
            {
                if (i < level) { 
                    if(lightOpen[i] == false) {
                        levelLight[i].PlayForward();
                        lightOpen[i] = true;
                    }
                }
                else {
                    if (lightOpen[i] == true)
                    {
                        levelLight[i].PlayReverse();
                        lightOpen[i] = false;
                    }
                }
            }
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
                            conditionViews[i].minusText.SetNumber(minus[j]);
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
