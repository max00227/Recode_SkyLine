using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundController : MonoBehaviour {
	int _groundRate;

	public GroundType _groundType;

	public GroundController matchController;

    public int? charaIdx;

    [HideInInspector]
    public int _layer;

    [HideInInspector]
	public bool isActived;

    [HideInInspector]
    public UIPolygon image;

	GroundType defaultType;

    GroundType? prevType;

    [SerializeField]
    Sprite[] GetSprites;

    int typeIdx;

	// Use this for initialization
	void Start () {
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
        prevType = null;
        isActived = false;
        typeIdx = 1;
    }

    // Update is called once per frame
    public void ResetType(bool isResetGround=false){
        if (gameObject.name == "29") {
            Debug.Log("Reset");
        }
        if (isResetGround)
        {
            _groundType = defaultType;
            typeIdx = 1;
            prevType = null;
        }
        else {
            if (prevType != null)
            {
                _groundType = (GroundType)prevType;
                typeIdx = (int)_groundType + 1;
                prevType = null;
            }
        }
        charaIdx = null;
        _layer = 1;

        if (image != null)
        {
            ChangeSprite();
        }
    }

	public void ChangeType(GroundType type = GroundType.None, int? idx = null){
        charaIdx = idx;

        if ((int)_groundType != 0 || (int)type != 0)
        {
            prevType = _groundType;
        }
        if (type == GroundType.None)
        {
            if ((int)_groundType == 0)
            {
                _groundType = GroundType.Copper;
            }
            else if ((int)_groundType == 1)
            {
                _groundType = GroundType.Silver;
            }
            else if ((int)_groundType == 2)
            {
                _groundType = GroundType.gold;
            }
        }
        else
        {
            _groundType = type;
        }

        if (image != null)
        {
            ChangeSprite();
        }
        _layer = 0; 
    }

    public void ChangeSprite()
    {
        if (image != null)
        {
            if ((int)_groundType == 99)
            {
                image.sprite = GetSprites[4];
            }
            else if ((int)_groundType < 4)
            {
                image.sprite = GetSprites[(int)_groundType];
            }
        }
    }

    public void SetType() {
        prevType = null;
    }

    public void UpLayer() {
        if ((int)_groundType == 0) {
            _layer++;
        }
    }
}
