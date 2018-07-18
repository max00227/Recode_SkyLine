using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class ParameterConvert {

	public static Dictionary<string, int> GetCharaAbility(CharaLargeData data,int charaLv){
		Dictionary<string, int> charaAbility = new Dictionary<string, int>();
	
		float radio = Mathf.Pow (charaLv, 0.7f) + Mathf.Ceil (charaLv / 10)*0.5f;
		Debug.Log (radio);

		charaAbility.Add ("Atk", (int)Mathf.Ceil (data.atk * radio));
		charaAbility.Add ("Def", (int)Mathf.Ceil (data.def * radio));
		charaAbility.Add ("MAtk", (int)Mathf.Ceil (data.mAtk * radio));
		charaAbility.Add ("MDef", (int)Mathf.Ceil (data.mDef * radio));
		charaAbility.Add ("Hp", (int)Mathf.Ceil (data.hp * radio));

		return charaAbility;
	}

	public static float JobRatioCalculation(int atkJob,int defJob){
		if (atkJob == 1 && defJob == 2) {
			return 1.5f;
		}
		if (atkJob == 2 && defJob == 3) {
			return 1.5f;
		}
		if (atkJob == 3 && defJob == 1) {
			return 1.5f;
		}
		if (atkJob == 4 && defJob == 5) {
			return 1.5f;
		}
		if (atkJob == 5 && defJob == 4) {
			return 1.5f;
		}
		return 1f; 
	}

	public static float AttriRatioCalculation(int atkAttr,int defAttr){
		if (atkAttr == 0) {
			if (defAttr == 0) {
				return 1.5f;
			}
			return 1;
		} 
		else {
			if (atkAttr == 1 && defAttr == 2) {
				return 1.5f;
			}
			if (atkAttr == 2 && defAttr == 3) {
				return 1.5f;
			}
			if (atkAttr == 3 && defAttr == 4) {
				return 1.5f;
			}
			if (atkAttr == 4 && defAttr == 1) {
				return 1.5f;
			}
			if (atkAttr == 5 && defAttr == 6) {
				return 1.5f;
			}
			if (atkAttr == 6 && defAttr == 5) {
				return 1.5f;
			}
			return 1;
		}
	}
}