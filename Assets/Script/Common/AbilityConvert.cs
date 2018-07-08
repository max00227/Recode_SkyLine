using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class AbilityConvert {

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
}