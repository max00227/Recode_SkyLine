﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public class SkillController : MonoBehaviour {
	[SerializeField]
	private FightController fightController;

	[SerializeField]
	private FightUIController fightUIController;

	public delegate void OnTriggerComplete();
	public OnTriggerComplete onTriggerComplete;

	[HideInInspector]
	private Dictionary<int, SkillLargeData> playerTriggerSkill;
	private Dictionary<int, SkillLargeData> playerPermanentSkill;
	private Dictionary<int, SkillLargeData> playerRoundSkill;

	private Dictionary<int, SkillLargeData> enemyNorSkill;
	private Dictionary<int, SkillLargeData> enemyPermanentSkill;
	private Dictionary<int, SkillLargeData> enemyTriggerSkill;

	private int playerCount;
	private int monsterCount;

	private RuleLargeData selLockRuleData;

	private int mainOrgIdx;
	private int mainTargetIdx;
	private int mainOrgRadio;
	private int mainSkillId;
	private string[] mainTarget;
    private Dictionary<int, SkillLargeData> orgSkills;
    private Dictionary<int, SkillLargeData> targetSkills;


    private ChessData mainOrgData;
	private ChessData mainTargetData;

	public void SetData(SoulLargeData[] playerData, SoulLargeData[] enemyData){
		playerCount = playerData.Length;
		monsterCount = enemyData.Length;
		playerTriggerSkill = new Dictionary<int, SkillLargeData> ();
		playerRoundSkill = new Dictionary<int, SkillLargeData> ();
		enemyNorSkill = new Dictionary<int, SkillLargeData> ();
		enemyTriggerSkill = new Dictionary<int, SkillLargeData> ();

		for (int i = 0; i < playerCount; i++) {
            if (playerData[i]._skill != null)
            {
                if (playerData[i]._skill.type == 1)
                {
                    if (playerData[i]._skill.launchType == 0)
                    {
                        playerTriggerSkill.Add(i, playerData[i]._skill);
                    }
                    else
                    {
                        playerRoundSkill.Add(i, playerData[i]._skill);
                    }
                }
				if (playerData [i]._skill.type == 0) {					
					playerPermanentSkill.Add (i, playerData [i]._skill);
				}
            }
		}

		for (int i = 0; i < monsterCount; i++) {
			if (enemyData[i]._skill != null)
            {
				if (enemyData [i]._skill.type == 1) {
					if (enemyData [i]._skill.launchType == 0) {
						enemyTriggerSkill.Add (i, enemyData [i]._skill);
					} else {
						enemyNorSkill.Add (i, enemyData [i]._skill);
					}
				} 
				if (enemyData [i]._skill.type == 0) {					
					enemyPermanentSkill.Add (i, enemyData [i]._skill);
				}
			}
		}
	}

	private void SetTriggerSkill(){
		
	}

	public void OnPermanentSkill(){
		foreach (KeyValuePair<int, SkillLargeData> kv in playerPermanentSkill) {
            mainTarget = new string[2] { "", "P" };
			foreach(RuleLargeData ruleData in kv.Value.ruleData){
				OnEffectTarget (ruleData, false);
			}
		}
		foreach (KeyValuePair<int, SkillLargeData> kv in enemyPermanentSkill) {
            mainTarget = new string[2] { "", "E" };
            foreach (RuleLargeData ruleData in kv.Value.ruleData){
				OnEffectTarget (ruleData, false);
			}
		}
	}

	/// <summary>
	/// 攻擊時觸發技能
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="allDamage">傷害資料</param>
	public void OnTriggerSkill(ChessData orgData, ChessData targetData, List<DamageData> allDamage){
		mainOrgIdx = allDamage [0].orgIdx;
		mainTargetIdx = allDamage [0].targetIdx;
		mainTarget = allDamage [0].tType;
		mainOrgData = orgData;
		mainTargetData = targetData;

        orgSkills = mainTarget[0] == "P" ? playerTriggerSkill : enemyTriggerSkill;
        targetSkills = mainTarget[0] == "P" ? playerTriggerSkill : enemyTriggerSkill;


		if (orgSkills.ContainsKey(mainOrgIdx)) {
			OnSkillSelfRule (orgSkills[mainOrgIdx], allDamage);
		}
		if (targetSkills.ContainsKey(mainTargetIdx)) {
			OnSkillUnSelfRule (targetSkills[mainTargetIdx], allDamage);
		}
	}

	public void OnRoundSkill(){
		foreach (KeyValuePair<int, SkillLargeData> kv in playerRoundSkill) {
			mainOrgIdx = kv.Key;
			mainOrgData = fightController.GetChessData ("P", mainOrgIdx);
			mainOrgRadio = fightController.GetRadio ("P", mainOrgIdx);
			mainTarget = new string[2]{"","P"};
			OnSkillSelfRule (kv.Value);
		}
	}

	/// <summary>
	/// 觸發條件為攻擊者
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="data">技能資料</param>
	/// <param name="allDamage">傷害資料</param>
	private void OnSkillSelfRule (SkillLargeData data, List<DamageData> allDamage = null){
		mainSkillId = data.id;

		int parameter = 0;
		bool[] meets = new bool[data.ruleData.Count];
		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData [i].rule [0]) {
			case (int)Rule.None:
			case (int)Rule.HpLess:
			case (int)Rule.HpBest:
				meets [i] = fightController.OnRuleMeets (mainOrgIdx, data.ruleData [i].rule, mainTarget[0]);
				break;
			case (int)Rule.OnDmg:
				meets [i] = allDamage !=null && allDamage.Count > 0;
				break;
			}
		}

		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					if (mainTarget[0] == "E"
						|| (mainTarget[0] == "P" && fightUIController.GetEnerge (data.ruleData [i].energe))) {
						OnEffectTarget (AddParameter (false, data.ruleData [i], parameter));
					}
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				OnMultiEffectTarget (data.ruleData, parameter, true);
			}
		}
	}

	/// <summary>
	/// 觸發條件為被攻擊者
	/// <param name="data">技能資料</param>
	/// <param name="allDamage">傷害資料</param>
	private void OnSkillUnSelfRule (SkillLargeData data, List<DamageData> allDamage = null){
		bool[] meets = new bool[data.ruleData.Count];
		int parameter = 0;
		foreach(DamageData damageData in allDamage){
			if(damageData.hpRatio<=0){
				for (int i = 0; i < data.ruleData.Count; i++) {
					if (data.ruleData [i].rule [0] == (int)Rule.Death) {
						meets [i] = true;
						if (data.isOr) {
							OnEffectTarget (data.ruleData [i]);
							return;
						}
					}
				}
			}
		}

		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData[i].rule [0]) {
			case (int)Rule.None:
				meets [i] = true;
				break;
			case (int)Rule.norDmg:
				meets [i] = allDamage.Count > 0;
				foreach (DamageData damageData in allDamage) {
					parameter += damageData.damage;
				}
				break;
			case (int)Rule.norDmgP:
				if (allDamage.Count > 0 && allDamage [0].damageType == DamageType.Physical) {
					meets [i] = true;
					parameter = allDamage [0].damage;
				}
				else{
					meets[i] = false;
				}
				break;
			case (int)Rule.norDmgM:
				//兩種傷害時，傷害一 物理：傷害二 魔法
				if (allDamage.Count == 2) {
					meets [i] = true;
					parameter = allDamage [1].damage;
				} 
				//但種傷害時，因傷害一可能為物理傷害，所以必須檢查
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
			}
		}
		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					if (mainTarget[1] == "E"
                        || (mainTarget[1] == "P" && fightUIController.GetEnerge (data.ruleData [i].energe))) {
						OnEffectTarget (AddParameter (true, data.ruleData [i], parameter), false);
					}
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				OnMultiEffectTarget (data.ruleData, parameter, false);
			}
		}
	}

	/// <summary>
	/// 符合多個Rule且不為isOr時使用
	private void OnMultiEffectTarget(List<RuleLargeData> rules, int parameter, bool isSelf = true){
		int needEnerge = 0;
		foreach (RuleLargeData rule in rules) {
			needEnerge += rule.energe;
		}
		if (!isSelf) {
            if (mainTarget[0] == "E"
                || (mainTarget[0] == "P" && fightUIController.GetEnerge (needEnerge))) {
				for (int i = 0; i < rules.Count; i++) {
					OnEffectTarget (AddParameter (true, rules [i], parameter), isSelf);
				}
			}
		} 
		else {
			if (mainTarget[1] == "E"
                || (mainTarget[0] == "P" && fightUIController.GetEnerge (needEnerge))) {
				for (int i = 0; i < rules.Count; i++) {
					OnEffectTarget (AddParameter (true, rules [i], parameter), isSelf);
				}
			}
		}
	}

	/// <summary>
	/// 決定技能效果目標
	/// <param name="data">技能效果資料</param>
	/// <param name="paramater">效果參數</param>
	private void OnEffectTarget(RuleLargeData data, bool isSelf = true){
		List<int> idxList = new List<int>();

		if (data.target >= 1 && data.target <= 4) {
            if (data.target > 1) {
				for (int i = 0; i < playerCount; i++) {
					idxList.Add (i);
				}
			}
		} else if (data.target > 4 && data.target <= 7) {
            mainTarget[1] = mainTarget[1] == "P" ? "E" : "P";
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
			if (isSelf) {
				idxList.Add (mainOrgIdx);
			} else {
				idxList.Add (mainTargetIdx);
			}
			break;
		case (int)Target.DirTeam:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, "P");
			return;
		case (int)Target.OnlyMate://移除發動者
			idxList.Remove (mainOrgIdx);
			break;
		case (int)Target.DirEnemy:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, "E");
			return;
		case (int)Target.Trigger:
			if (isSelf) {
				idxList.Add (mainTargetIdx);
			} else {
				idxList.Add (mainOrgIdx);
			}
			break;
		}

		fightController.OnSkillEffect (mainOrgIdx, idxList, data, mainTarget, mainSkillId);
	}



	public void SelectSkillTarget(string targetString, int idx){
		fightController.OnSkillEffect (mainOrgIdx, new List<int> (new int[1]{ idx }), selLockRuleData, mainTarget, mainSkillId);
	}

	/// <summary>
	/// 補血超過該角色上限時觸發
	/// <param name="org">補血者</param>
	/// <param name="target">被補血者</param>
	/// <param name="over">超過數值</param>
	/// <param name="tType">被補血者陣營</param>
	public void OverRecovery(int org , int target, int over, string[] tType){
		mainOrgIdx = org;
		mainTargetIdx = target;
		mainTarget = tType.ToString().Split('_');
        targetSkills = mainTarget[1] == "P" ? playerTriggerSkill : enemyTriggerSkill; 

		if (targetSkills.ContainsKey(mainOrgIdx)) {
			foreach (RuleLargeData data in playerTriggerSkill[mainOrgIdx].ruleData) {
				if (data.rule [0] == (int)Rule.Over) {
					mainOrgData = fightController.GetChessData (mainTarget[0], mainOrgIdx);
					OnEffectTarget (AddParameter (false, data, over));
				}
			}
		}
	}

	/// <summary>
	/// 將當RuleLatgeData.Effect參數數量大於1又為0時補上缺少的Parameter，IsRev為True時會用TargetData
	private RuleLargeData AddParameter(bool isRev, RuleLargeData data, int parameter = 0){
		RuleLargeData rData = data.DeepCopy ();
		if (rData.effect.Length > 1) {
			if (rData.effect [1] == 0) {
				if (parameter != 0) {
					rData.effect [1] = parameter;
				} else {
					rData.effect [1] = GetParameter (isRev, data);
				}
			} 
			else {
				if (parameter != 0 && rData.convType == Const.converseType.Ratio) {
					rData.effect [1] = rData.effect [1] * parameter / 100;	
				}
			}
		} 

		return rData;
	}
		
	private int GetParameter(bool isRev, RuleLargeData data){
		int param = 0;
		foreach (KeyValuePair<string,int> kv in data.abilitys) {
			int abiChange = 100;
			if (kv.Value != 0) {
				if (data.convType == 0) {
					param += kv.Value;
				} 
				else {
					if (isRev) {
						if (mainTargetData.soulData != null) {
							foreach(var value in mainTargetData.abiChange.Values){
								abiChange += value.ContainsKey (kv.Key) ? value [kv.Key] : 0;
							}
							param += mainTargetData.soulData.abilitys [kv.Key] * kv.Value / 100 * abiChange / 100;
						}
					} 
					else {
						foreach (var value in mainOrgData.abiChange.Values) {
							abiChange += value.ContainsKey (kv.Key) ? value [kv.Key] : 0;
						}

						param += mainOrgData.soulData.abilitys [kv.Key] * kv.Value / 100 * mainOrgRadio / 100 * abiChange / 100;
					}
				}
			}
		}
		return param;
	}

	public void ShowRuleData(){
		foreach (KeyValuePair<int, SkillLargeData> kv in playerTriggerSkill) {
			foreach (RuleLargeData data in kv.Value.ruleData) {
				Debug.Log (data.id + " , " + data.abilitys.Count);
			}
		}
	}
}
/// <summary>
/// Rule type.
/// None (無),HpLess (血量(少於)),HpBest (血量(多於含)),Nerf (異常狀態),norDmg (自身傷害(全)),norDmgP (自身傷害(物))
/// norDmgM (自身傷害(魔)),OnDmg (對方傷害),DeathCount (隊友死亡數),Over (溢補值),Death (自己死亡)
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
	Death = 10,
	JobCntUp = 11,
	LayerCntUp =12,
	JobCntDown = 13,
	LayerCntDown = 14,
}

/// <summary>
/// Target type
/// None(無),Self(自身),DirTeam(指定成員),OnlyMate(僅隊友),Team(全隊),Enemy(全敵人),DirEnemy(指定敵人),Trigger(觸發者)
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
/// Normal Effect type.
/// None(無),Recovery(回血),Act(激活),Cover(覆蓋),RmAlarm(警戒解除)
/// ,RmNerf(異常解除),Dmg(固定傷害),Exchange(位置對調),Call(召喚)
/// ,Revive(復活),Energe(恢復能量),DelJob(移除指定職業格子)
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
	Revive = 9,
	Energe = 10,
	DelJob = 11
}

/// <summary>
/// Status Effect type.
/// None(無),UnDef(無視防禦),UnNerf(異常免疫),AddNerf(附加異常),Suffer(根性)
/// ,Maximum(傷害值最大化),Ability(能力變化),UnDirect(反鎖定)
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
/// Nerf Effect type.
/// None(無),Damage(傷害),UnTake(無法擺放),Death(死亡),RcyDown(恢復下降)
/// ,DmgUp(受傷上升),AtkDown(攻擊下降),Confusion(隨意攻擊)
public enum Nerf {
	None = 0,
	Hit = 1,
	UnTake = 2,
	Death = 3,
	Confusion = 4,
	UnAct = 5
}