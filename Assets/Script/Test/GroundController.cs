using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundController : MonoBehaviour {
	int _groundRate;

	public GroundType _groundType;

	public GroundController matchController;

    private int? charaIdx;

    [HideInInspector]
    public int _layer;

	bool isActive;

    [HideInInspector]
    public UIPolygon image;

	GroundType defaultType;

    [SerializeField]
    Sprite[] GetSprites;

	// Use this for initialization
	void Start () {
        defaultType = _groundType;
        _layer = 1;

    }

    // Update is called once per frame
    public void ResetType(){
		_groundType = defaultType;
        charaIdx = null;
        _layer = 1;

        if (image != null)
        {
            ChangeSprite();
        }
    }

	public void ChangeType(GroundType type = GroundType.None, int? idx = null){
        charaIdx = idx;

        if (type == GroundType.None)
        {
            if (_groundType == GroundType.None)
            {
                _groundType = GroundType.Copper;
            }
            else if (_groundType == GroundType.Copper)
            {
                _groundType = GroundType.Silver;
            }
            else if (_groundType == GroundType.Silver)
            {
                _groundType = GroundType.gold;
            }
        }
        else
        {
            _groundType = type;
        }


        _layer = 0;

        if (image != null)
        {
            ChangeSprite();
        }
    }

    public void ChangeSprite()
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

    public void UpLayer() {
        if ((int)_groundType == 0) {
            _layer++;
        }
    }
}
