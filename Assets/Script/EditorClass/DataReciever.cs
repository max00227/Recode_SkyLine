using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using model.data;

public class DataReciever : MonoBehaviour {
	GameObject[] target;

	[EnumPopup(typeof(DataType),true)]
	public DataType dataType;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GetData(object param){
		foreach (var v in (List<object>)param) {
			var data = JsonConversionExtensions.readJson (v.ToString ());

			SetData (data);
		}
	}

	public void SetData(Dictionary<string, object> data){
		switch (dataType) {
		case DataType.Chara:
			int id;
			int lv;
				
			CharaLargeData charaData = MasterDataManager.GetCharaData (Int32.Parse (data ["id"].ToString ()));

			Dictionary<string ,int> ability = ParameterConvert.GetCharaAbility (charaData, Int32.Parse (data ["lv"].ToString ()));
			foreach (KeyValuePair<string, int> kv in ability) {
				Debug.Log (kv.Key + " , " + kv.Value);
			}
			break;
		case DataType.Skill:
			break;
		case DataType.Rule:
			break;
		}
	}
}

public enum DataType{
	Chara,
	Skill,
	Rule
}
