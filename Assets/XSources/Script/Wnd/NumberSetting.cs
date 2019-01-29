using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberSetting : MonoBehaviour {
	[SerializeField]
	Text text;

    [SerializeField]
    TextMeshProUGUI textMesh;

	int showRatio = 0;

	int prevRatio;

	float upSpeed = 0;

	bool isRun;

	[SerializeField]
	float speed = 0;

	public delegate void OnComplete();
	public OnComplete onComplete;

	bool isSet = false;

    public enum SymbolType
    {
        None,
        Plus = '+',
        Minus = '-',
        Multiply = 'x',
        Divided = '/',
        Percent = '%'
    }

    public SymbolType symbolType;

    [SerializeField]
	bool isReturnMin = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (isSet) {
			if (isRun) {
				if (!isReturnMin) {
					if (prevRatio < showRatio) {
						prevRatio = (int)DataUtil.LimitFloat (Mathf.CeilToInt (prevRatio + upSpeed * Time.deltaTime), showRatio, isReturnMin);
                        SetText(prevRatio.ToString());
                    }
                    else {
                        SetText(showRatio.ToString());

                        isRun = false;
						isSet = false;
						prevRatio = showRatio;
						if (onComplete != null) {
							onComplete.Invoke ();
							onComplete = null;
						}
					}
				} 
				else {
					if (prevRatio > showRatio) {
						prevRatio = (int)DataUtil.LimitFloat (Mathf.CeilToInt (prevRatio + upSpeed * Time.deltaTime), showRatio, isReturnMin);
                        SetText(prevRatio.ToString());
                    }
                    else {
                        SetText(showRatio.ToString());
                        isRun = false;
						isSet = false;
						prevRatio = showRatio;
						if (onComplete != null) {
							onComplete.Invoke ();
							onComplete = null;
						}
					}
				}
			}
		} 
		else {
			isRun = false;
		}
	}

	public void SetNumber (int number) {
        SetText(number.ToString());
	}

	public void SetShowUp(int number) {
		isRun = false;
		upSpeed = (number - showRatio) / speed;

		prevRatio = showRatio;

		showRatio = number;
		isSet = true;
	}

	public void SetPlus(int number){
		showRatio = showRatio + number;

        SetText(showRatio.ToString());
    }

    public void ResetNumber () {
		showRatio = 0;
        SetText(showRatio.ToString());
    }

    public void SetText(string content) {
        string finalContent;
        if (symbolType == SymbolType.None)
        {
            finalContent = content;
        }
        else if (symbolType == SymbolType.Percent)
        {
            finalContent = content + ((char)symbolType).ToString();
        }
        else
        {
            finalContent = ((char)symbolType).ToString() + content;
        }


        if (text != null)
        {
            text.text = finalContent;
        }
        if (textMesh != null)
        {
            textMesh.text = finalContent;
        }
    }

    public void SetColor (Color color) {
        if (text != null)
        {
            text.color = color;
        }
        if (textMesh != null)
        {
            textMesh.color = color;
        }
	}

	public void run(){
		isRun = true;
	}
}
