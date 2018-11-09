using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class SkillController : MonoBehaviour {
	[SerializeField]
	private FightController fightController;

	public delegate void OnTriggerComplete();
	public OnTriggerComplete onTriggerComplete;

	[HideInInspector]
	public SkillLargeData[] charaNorSkill;
	private SkillLargeData[] monsterNorSkill;

	private SkillLargeData[] charaTriggerSkill;
	private SkillLargeData[] charaRoundSkill;
	private SkillLargeData[] monsterTriggerSkill;

	private int charaCount;
	private int monsterCount;

	private RuleLargeData selLockRuleData;

	public void SetData(SoulLargeData[] charaData, SoulLargeData[] monsterData){
		charaCount = charaData.Length;
		monsterCount = monsterData.Length;
		charaNorSkill = new SkillLargeData[charaCount];
		charaTriggerSkill = new SkillLargeData[charaCount];
		charaRoundSkill = new SkillLargeData[charaCount];
		monsterNorSkill = new SkillLargeData[monsterCount];
		monsterTriggerSkill = new SkillLargeData[monsterCount];

		for (int i = 0; i < charaCount; i++) {
			if (charaData [i]._norSkill != null) {
				charaNorSkill [i] = charaData [i]._norSkill;
			}

			if (charaData [i]._actSkill != null) {
				if (charaData [i]._actSkill.launchType == 0) {
					charaTriggerSkill [i] =charaData [i]._actSkill; 	
				} else {
					charaRoundSkill [i] = charaData [i]._actSkill;
				}
			}
		}

		for (int i = 0; i < monsterCount; i++) {
			if (monsterData [i]._norSkill != null) {
				if (monsterData[i]._norSkill.launchType == 0) {
					monsterTriggerSkill [i] = monsterData[i]._norSkill;
				} 
				else {
					monsterNorSkill [i] = monsterData[i]._norSkill;
				}
			}
		}
	}

	private void SetTriggerSkill(){
		
	}

	public void OnTriggerSkill(int orgIdx, int targetIdx, TargetType tType, List<DamageData> damageData = null){
		if (tType == TargetType.Player) {
			if (charaTriggerSkill [orgIdx] != null) {
				OnSkillSelfRule (orgIdx, targetIdx, charaTriggerSkill [orgIdx], tType, damageData);
			}
			if (monsterTriggerSkill [targetIdx] != null) {
				OnSkillUnSelfRule (orgIdx, targetIdx, monsterTriggerSkill [orgIdx], tType, damageData);
			}
		} 
		else {
			if (monsterTriggerSkill [orgIdx] != null) {
				OnSkillSelfRule (orgIdx, targetIdx, monsterTriggerSkill [orgIdx], tType, damageData);
			}
			if (charaTriggerSkill [targetIdx] != null) {
				OnSkillUnSelfRule (orgIdx, targetIdx, charaTriggerSkill [orgIdx], tType, damageData);
			}
		}
	}


	private void OnSkillSelfRule (int orgIdx, int targetIdx, SkillLargeData data, TargetType tType, List<DamageData> damageData){
		int parameter = 0;
		bool[] meets = new bool[data.ruleData.Count];
		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData [i].rule [0]) {
			case (int)Rule.None:
				meets [i] = true;
				break;
			case (int)Rule.HpLess:
				meets [i] = fightController.OnRuleMeets (orgIdx, data.ruleData [i].rule [0], data.ruleData [i].rule [1], tType);
				break;
			case (int)Rule.HpBest:
				meets [i] = fightController.OnRuleMeets (orgIdx, data.ruleData [i].rule [0], data.ruleData [i].rule [1], tType);
				break;
			case (int)Rule.OnDmg:
				meets [i] = damageData != null;
				break;
			default:
				meets [i] = true;
				break;
			}
		}

		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData [i], tType, parameter);
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				for (int i = 0; i < data.ruleData.Count; i++) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData[i], tType, parameter);
				}
			}
		}
	}

	private void OnSkillUnSelfRule (int orgIdx, int targetIdx, SkillLargeData data, TargetType tType, List<DamageData> allDamage){
		bool[] meets = new bool[data.ruleData.Count];
		int parameter = 0;
		foreach(DamageData damageData in allDamage){
			if(damageData.hpRatio<=0){
				for (int i = 0; i < data.ruleData.Count; i++) {
					if (data.ruleData [i].rule [0] == (int)Rule.Death) {
						meets [i] = true;
						if (data.isOr) {
							OnEffectTarget (orgIdx, targetIdx, data.ruleData [i], tType);
							return;
						}
					}
				}
			}
		}

		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData[i].rule [0]) {
			case (int)Rule.norDmg:
				meets [i] = allDamage.Count > 0;
				foreach (DamageData damageData in allDamage) {
					parameter += damageData.damage;
				}
				break;
			case (int)Rule.norDmgP:
				if (allDamage.Count == 1 && allDamage [0].damageType == DamageType.Magic) {
					meets [i] = true;
					parameter = allDamage [0].damage;
				}
				else{
					meets[i] = false;
				}
				break;
			case (int)Rule.norDmgM:
				if (allDamage.Count == 2) {
					meets [i] = true;
					parameter = allDamage [1].damage;
				} 
				else if (allDamage.Count == 1) {
					if (allDamage [0].damageType == DamageType.Magic) {
						meets [i] = true;
						parameter = allDamage [0].damage;
					} 
					else {
						meets [i] = false;
					}
				} 
				else {
					meets [i] = false;

				}
				break;
			default:
				meets [i] = true;
				break;
			}

		}
		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData [i], tType, parameter);
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				for (int i = 0; i < data.ruleData.Count; i++) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData[i], tType, parameter);
				}
			}
		}
	}
		
	private void OnEffectTarget(int orgIdx, int targetIdx, RuleLargeData data, TargetType tType, int paramater = 0){
		List<int> idxList = new List<int>();
		TargetType targetType;
		if (data.target >= 1 && data.target <= 4) {
			targetType = tType;
			if (data.target > 1) {
				for (int i = 0; i < charaCount; i++) {
					idxList.Add (i);
				}
			}
		} else if (data.target > 4 && data.target <= 7) {
			targetType = ReverseTarget (tType);
			if (data.target < 7) {
				for (int i = 0; i < monsterCount; i++) {
					idxList.Add (i);
				}
			}
		}

		switch (data.target) {
		case (int)Target.None:
			break;
		case (int)Target.Self:
			idxList.Add (targetIdx);
			break;
		case (int)Target.DirTeam:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, TargetType.Player);
			return;
		case (int)Target.OnlyMate://移除發動者
			idxList.Remove (orgIdx);
			break;
		case (int)Target.DirEnemy:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, TargetType.Enemy);
			return;
		case (int)Target.Trigger:
			idxList.Add (targetIdx);
			break;
		}

		OnSkillEffect (idxList, data, tType, paramater);
	}

	private void OnSkillEffect(List<int> idxList, RuleLargeData data, TargetType tType = TargetType.Player, int paramater = 0){
		if (data.normalEffect [0] != 0) {
			OnNormal (idxList, data, tType, paramater);
		}

		if (data.statusEffect [0] != 0) {
			OnStatus (idxList, data, tType, paramater);
		}
	}


	/*Recovery = 1,
	Act = 2,
	Cover = 3,
	RmAlarm = 4,
	RmNerf = 5,
	Dmg = 6,
	Exchange = 7,
	Call = 8,*/
	private void OnNormal(List<int> idxList, RuleLargeData data, TargetType tType = TargetType.Player, int paramater = 0){
		switch (data.normalEffect [0]) {
		case (int)Normal.Recovery:
			break;
		}
	}


	/*UnDef = 1,
	UnNerf = 2,
	AddNerf = 3,
	Suffer = 4,
	Maximum = 5,
	Ability = 6,
	UnDirect = 7*/
	private void OnStatus(List<int> idxList, RuleLargeData data, TargetType tType = TargetType.Player, int paramater = 0){
		
	}

	public void SelectSkillTarget(TargetType tType, int idx){
		OnSkillEffect (new List<int> (new int[1]{ idx }), selLockRuleData, tType);
	}

	public void OverRecovery(int orgIdx , int targetIdx, int over, TargetType tType){
		if (tType == TargetType.Player) {
			if (charaTriggerSkill [orgIdx] != null) {
				foreach (RuleLargeData data in charaTriggerSkill[orgIdx].ruleData) {
					if (data.rule [0] == (int)Rule.Over) {
						OnEffectTarget (orgIdx, targetIdx, data, tType);
					}
				}
			}
		} 
		else {
			if (monsterTriggerSkill [orgIdx] != null) {
				foreach (RuleLargeData data in monsterTriggerSkill[orgIdx].ruleData) {
					if (data.rule [0] == (int)Rule.Over) {
						OnEffectTarget (orgIdx, targetIdx, data, tType);
					}
				}
			}
		}
	}
		
	public TargetType ReverseTarget(TargetType tType){
		if (tType == TargetType.Player) {
			return TargetType.Enemy;
		} 
		else {
			return TargetType.Player;
		}
	}
}
/// <summary>
/// Rule type.
/// None (無),HpLess (血量(少於)),HpBest (血量(多於含)),Nerf (異常狀態),norDmg (自然傷害值(自)),norDmgP (自然傷害值(物自))
/// norDmgM (自然傷害值(魔自)),SpDmg (自然傷害值(他)),DeathCount (隊友死亡數),Over (溢補值),Death (自己死亡)
/// </summary>
public enum Rule {
	None = 0,
	HpLess = 1,
	HpBest = 2,
	Nerf = 3,
	norDmg = 4,
	norDmgP = 5,
	norDmgM = 6,
	OnDmg = 7,
	DeathCount = 8,
	Over = 9,
	Death = 10
}

/// <summary>
/// Target type
/// None(無),Self(自身),DirTeam(指定成員),OnlyMate(僅隊友),Team(全隊),Enemy(全敵人),DirEnemy(指定敵人),Trigger(觸發者)
/// </summary>
public enum Target {
	None = 0,
	Self = 1,
	DirTeam = 2,
	OnlyMate = 3,
	Team = 4,
	Enemy = 5,
	DirEnemy = 6,
	Trigger = 7
}



/// <summary>
/// Effect type.
/// None(無),Recovery(回血),Act(激活),Cover(覆蓋),RmAlarm(警戒解除)
/// ,RmNerf(異常解除),Dmg(固定傷害),Exchange(位置對調),Call(召喚)
/// </summary>
public enum Normal {
	None = 0,
	Recovery = 1,
	Act = 2,
	Cover = 3,
	RmAlarm = 4,
	RmNerf = 5,
	Dmg = 6,
	Exchange = 7,
	Call = 8,
}

/// <summary>
/// Effect type.
/// None(無),UnDef(無視防禦),UnNerf(異常免疫),AddNerf(附加異常),Suffer(根性)
/// ,Maximum(傷害值最大化),Ability(能力變化),UnDirect(反鎖定)
/// </summary>
public enum Status {
	None = 0,
	UnDef = 1,
	UnNerf = 2,
	AddNerf = 3,
	Suffer = 4,
	Maximum = 5,
	Ability = 6,
	UnDirect = 7
}

/// <summary>
/// Converse type.
/// None(無),Amount(純數值),Ratio(比率)
/// </summary>
public enum converseType {
	None = 0,
	Amount = 1,
	Ratio = 2
}

