using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class SkillController : MonoBehaviour {
	[SerializeField]
	private FightController fightController;

	public delegate void OnTriggerComplete();
	public OnTriggerComplete onTriggerComplete;

	private SkillLargeData[] charaNorSkill;
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
			charaNorSkill [i] = charaData [i]._NorSkill;
			if (charaData [i]._ActSkill.launchType == 0) {
				charaTriggerSkill [i] = charaData [i]._ActSkill; 	
			} 
			else {
				charaRoundSkill [i] = charaData [i]._ActSkill; 	
			}
		}

		for (int i = 0; i < monsterCount; i++) {
			if (monsterData [i]._NorSkill != null) {
				if (monsterData [i]._NorSkill.launchType == 0) {
					monsterTriggerSkill [i] = monsterData [i]._NorSkill;
				} 
				else {
					monsterNorSkill [i] = monsterData [i]._NorSkill;
				}
			}
		}
	}

	private void SetTriggerSkill(){
		
	}

	private void OnTriggerSkill(int orgIdx ,int targetIdx, TargetType tType, DamageData damageData){
		if (tType = TargetType.Player) {
			if (charaTriggerSkill [orgIdx] != null) {
				OnSkillRule (orgIdx, orgIdx, charaTriggerSkill [orgIdx], tType, damageData);
			}
		} else {
			if (monsterTriggerSkill [orgIdx] != null) {
				OnSkillRule (orgIdx, orgIdx, monsterTriggerSkill [orgIdx], tType, damageData);
			}
		}
	}

	private void OnSkillRule (int orgIdx,int targetIdx ,SkillLargeData data, TargetType tType,DamageData damageData){
		bool[] meets = new bool[data.ruleData.Count];
		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData[i].RuleType [0]) {
			case 0:
				meets [i] = true;
				break;
			case 1:
				meets[i] = fightController.OnRuleMeets (orgIdx, data.ruleData[i].RuleType [0], data.ruleData[i].RuleType [1],tType);
				break;
			case 2:
				meets[i] = fightController.OnRuleMeets (orgIdx, data.ruleData[i].RuleType [0], data.ruleData[i].RuleType [1],tType);
				break;
			}
		}
		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData [i], tType);
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				for (int i = 0; i < data.ruleData.Count; i++) {
					OnEffectTarget (orgIdx, targetIdx, data.ruleData[i], tType);
				}
			}
		}
	}

	private void OnEffectTarget(int orgIdx ,int targetIdx, RuleLargeData data, TargetType tType){
		List<int> idxList = new List<int>();
		TargetType targetType;
		if (data.Target >= 1 && data.Target <= 4) {
			targetType = tType;
			if (data.Target > 1) {
				for (int i = 0; i < charaCount; i++) {
					idxList.Add (i);
				}
			}
		} else if (data.Target > 4 && data.Target <= 7) {
			targetType = ReverseTarget (tType);
			if (data.Target < 7) {
				for (int i = 0; i < monsterCount; i++) {
					idxList.Add (i);
				}
			}
		}

		switch (data.Target) {
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

		OnSkillEffect (idxList, data, tType);
	}

	private void OnSkillEffect(List<int> idxList, RuleLargeData data, TargetType tType = TargetType.Player){
		
	}

	public void SelectSkillTarget(TargetType tType, int idx){
		
		OnSkillEffect (new List<int> (new int[1]{ idx }), selLockRuleData, tType);
	}

	public void OverRecovery(int orgIdx , int targetIdx, int over, TargetType tType){
		if (tType == TargetType.Player) {
			if (charaTriggerSkill [orgIdx] != null) {
				foreach (RuleLargeData data in charaTriggerSkill[orgIdx].ruleData) {
					if (data.RuleType [0] == 9) {
						OnEffectTarget (orgIdx, targetIdx, data, tType);
					}
				}
			}
		} 
		else {
			if (monsterTriggerSkill [orgIdx] != null) {
				foreach (RuleLargeData data in monsterTriggerSkill[orgIdx].ruleData) {
					if (data.RuleType [0] == 9) {
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

	/// <summary>
	/// Rule type.
	/// None (無),HpLess (血量(少於)),HpBest (血量(多於含)),Nerf (異常狀態),norDmg (自然傷害值(自)),norDmgP (自然傷害值(物自))
	/// norDmgM (自然傷害值(魔自)),SpDmg (自然傷害值(他)),DeathCount (隊友死亡數),Over (溢補值),SelfDeath (自己死亡)
	/// </summary>
	private enum ruleType {
		None = 0,
		HpLess = 1,
		HpBest = 2,
		Nerf = 3,
		norDmg = 4,
		norDmgP = 5,
		norDmgM = 6,
		SpDmg = 7,
		DeathCount = 8,
		Over = 9,
		SelfDeath = 10
	}

	/// <summary>
	/// Target type
	/// None(無),Self(自身),DirTeam(指定成員),OnlyMate(僅隊友),Team(全隊),Enemy(全敵人),DirEnemy(指定敵人),Trigger(觸發者)
	/// </summary>
	private enum Target {
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
	/// None(無),Recovery(回血),UnDef(無視防禦),Act(激活),Cover(覆蓋),(警戒解除),RmNerf(異常解除),ImNerf(異常免疫)
	/// Reflect(反擊),Dmg(固定傷害),AddNerf(附加異常),Suffer(根性),Exchange(位置對調),Maximum(傷害值最大化),Buff(能力上升),Call(召喚)
	/// </summary>
	private enum effectType {
		None = 0,
		Recovery = 1,
		UnDef = 2,
		Act = 3,
		Cover = 4,
		RmNerf = 5,
		ImNerf = 6,
		Reflect = 7,
		Dmg = 8,
		AddNerf = 9,
		Suffer = 10,
		Exchange = 11,
		Maximum = 12,
		Buff = 13,
		Call = 14,
		Nerf = 15,
		UnDirect = 16
	}

	/// <summary>
	/// Converse type.
	/// None(無),Amount(純數值),Ratio(比率)
	/// </summary>
	private enum converseType {
		None = 0,
		Amount = 1,
		Ratio = 2
	}
}
