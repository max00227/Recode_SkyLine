using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundController : MonoBehaviour {
	int _groundRate;

	public GroundType _groundType;

	public GroundController matchController;

    private int? charaIdx;

	int _layer;

	bool isActive;

	GroundType defaultType;

	// Use this for initialization
	void Start () {
		defaultType = _groundType;
	}
	
	// Update is called once per frame
	public void ResetType(){
		_groundType = defaultType;
        charaIdx = null;
	}

	public void ChangeType(GroundType type = GroundType.None, int? idx = null){
        charaIdx = idx;

		if (type == GroundType.None) {
			if (_groundType == GroundType.None) {
				_groundType = GroundType.Copper;
			} else if (_groundType == GroundType.Copper) {
				_groundType = GroundType.Silver;
			} else if (_groundType == GroundType.Silver) {
				_groundType = GroundType.gold;
			}
		} else {
			_groundType = type;
		}
	}
}
