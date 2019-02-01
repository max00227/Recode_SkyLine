using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class ParameterConvert {

	public static Dictionary<string, int> GetCharaAbility(SoulLargeData data,int charaLv){
		Dictionary<string, int> charaAbility = new Dictionary<string, int>();
	
		float radio = Mathf.Pow (charaLv, 0.7f) + Mathf.Ceil (charaLv / 10)*0.5f;

		charaAbility.Add ("Atk", (int)Mathf.Ceil (data.abilitys["Atk"] * radio));
		charaAbility.Add ("Def", (int)Mathf.Ceil (data.abilitys["Def"] * radio));
		charaAbility.Add ("mAtk", (int)Mathf.Ceil (data.abilitys["mAtk"] * radio));
		charaAbility.Add ("mDef", (int)Mathf.Ceil (data.abilitys["mDef"] * radio));
		charaAbility.Add ("Hp", (int)Mathf.Ceil (data.abilitys["Hp"] * radio));
        if (data.job == 2 || data.job == 4)
        {
            charaAbility.Add("Spc", (int)Mathf.Ceil(data.abilitys["Spc"] * radio));
        }

        return charaAbility;
	}

	public static Dictionary<string, int> GetEnemyAbility(SoulLargeData data,int monsterLv){
		Dictionary<string, int> monsterAbility = new Dictionary<string, int>();

		float calculate = Mathf.Pow (monsterLv, 0.2f + (0.005f * monsterLv)) + (24 * monsterLv / 120);

		float radio = calculate<2f?1:calculate;

		monsterAbility.Add ("Atk", (int)Mathf.Ceil (data.abilitys["Atk"] * radio));
		monsterAbility.Add ("Def", (int)Mathf.Ceil (data.abilitys["Def"] * radio));
		monsterAbility.Add ("mAtk", (int)Mathf.Ceil (data.abilitys["mAtk"] * radio));
		monsterAbility.Add ("mDef", (int)Mathf.Ceil (data.abilitys["mDef"] * radio));
		monsterAbility.Add ("Hp", (int)Mathf.Ceil (data.abilitys["Hp"] * radio));

		return monsterAbility;
	}

	public static float JobRatioCal(int atkJob, int defJob){
		if (atkJob == 0 && defJob == 1) {
			return 1.5f;
		}
		if (atkJob == 1 && defJob == 2) {
			return 1.5f;
		}
		if (atkJob == 2 && defJob == 0) {
			return 1.5f;
		}
		if (atkJob == 3 && defJob == 4) {
			return 1.5f;
		}
		if (atkJob == 4 && defJob == 3) {
			return 1.5f;
		}
		return 1f; 
	}

	public static float AttriRatioCal(int atkAttr, int defAttr){
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