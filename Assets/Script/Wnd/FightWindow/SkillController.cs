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
	private Dictionary<int, SkillLargeData> charaTriggerSkill;
	private Dictionary<int, SkillLargeData> charaRoundSkill;

	private Dictionary<int, SkillLargeData> enemyNorSkill;
	private Dictionary<int, SkillLargeData> enemyTriggerSkill;

	private int charaCount;
	private int monsterCount;

	private RuleLargeData selLockRuleData;

	private int dirOrgIdx;
	private int dirTargetIdx;
	private int dirOrgRadio;
	private TargetType dirTargetType;

	private SoulLargeData dirOrgData;
	private SoulLargeData dirTargetData;

	public void SetData(SoulLargeData[] charaData, SoulLargeData[] monsterData){
		charaCount = charaData.Length;
		monsterCount = monsterData.Length;
		charaNorSkill = new SkillLargeData[charaCount];
		charaTriggerSkill = new Dictionary<int, SkillLargeData> ();
		charaRoundSkill = new Dictionary<int, SkillLargeData> ();
		enemyNorSkill = new Dictionary<int, SkillLargeData> ();
		enemyTriggerSkill = new Dictionary<int, SkillLargeData> ();

		for (int i = 0; i < charaCount; i++) {
			if (charaData [i]._norSkill != null) {
				charaNorSkill [i] = charaData [i]._norSkill;
			}

			if (charaData [i]._actSkill != null) {
				if (charaData [i]._actSkill.launchType == 0) {
					charaTriggerSkill.Add(i,charaData[i]._actSkill); 	
				} else {
					charaRoundSkill.Add(i,charaData[i]._actSkill); 	
				}
			}
		}

		for (int i = 0; i < monsterCount; i++) {
			if (monsterData [i]._norSkill != null) {
				if (monsterData[i]._norSkill.launchType == 0) {
					enemyTriggerSkill.Add(i, monsterData[i]._norSkill);
				} 
				else {
					enemyNorSkill.Add(i, monsterData[i]._norSkill);
				}
			}
		}
	}

	private void SetTriggerSkill(){
		
	}

	/// <summary>
	/// 攻擊時觸發技能
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="allDamage">傷害資料</param>
	public void OnTriggerSkill(SoulLargeData orgData, SoulLargeData targetData, List<DamageData> allDamage){
		dirOrgIdx = allDamage [0].orgIdx;
		dirTargetIdx = allDamage [0].targetIdx;
		dirTargetType = allDamage [0].tType;
		dirOrgData = orgData;
		dirTargetData = targetData;

		if (allDamage[0].tType == TargetType.Enemy) {
			if (charaTriggerSkill.ContainsKey(dirOrgIdx)) {
				OnSkillSelfRule (charaTriggerSkill [dirOrgIdx], allDamage);
			}
			if (enemyTriggerSkill.ContainsKey(dirTargetIdx)) {
				OnSkillUnSelfRule (enemyTriggerSkill [dirTargetIdx], allDamage);
			}
		} 
		else {
			if (enemyTriggerSkill.ContainsKey(dirOrgIdx)) {
				OnSkillSelfRule (enemyTriggerSkill [dirOrgIdx], allDamage);
			}
			if (charaTriggerSkill.ContainsKey(dirTargetIdx)) {
				OnSkillUnSelfRule (charaTriggerSkill [dirTargetIdx], allDamage);
			}
		}
	}

	public void OnRoundSkill(){
		foreach (KeyValuePair<int, SkillLargeData> kv in charaRoundSkill) {
			dirOrgIdx = kv.Key;
			dirOrgData = fightController.GetSoulData (TargetType.Player, dirOrgIdx);
			dirOrgRadio = fightController.GetRadio (TargetType.Player, dirOrgIdx);
			dirTargetType = TargetType.Player;
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
		int parameter = 0;
		bool[] meets = new bool[data.ruleData.Count];
		for (int i = 0; i < data.ruleData.Count; i++) {
			switch (data.ruleData [i].rule [0]) {
			case (int)Rule.None:
				meets [i] = true;
				break;
			case (int)Rule.HpLess:
				meets [i] = fightController.OnRuleMeets (dirOrgIdx, data.ruleData [i].rule [0], data.ruleData [i].rule [1], dirTargetType);
				break;
			case (int)Rule.HpBest:
				meets [i] = fightController.OnRuleMeets (dirOrgIdx, data.ruleData [i].rule [0], data.ruleData [i].rule [1], dirTargetType);
				break;
			case (int)Rule.OnDmg:
				meets [i] = allDamage !=null && allDamage.Count > 0;
				break;
			}
		}

		if (data.isOr) {
			for (int i = 0; i < data.ruleData.Count; i++) {
				if (meets [i] == true) {
					OnEffectTarget (AddParameter (false, data.ruleData [i], parameter));
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				for (int i = 0; i < data.ruleData.Count; i++) {
					OnEffectTarget (AddParameter (false, data.ruleData [i], parameter));
				}
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
					OnEffectTarget (AddParameter (true, data.ruleData [i], parameter));
				}
			}
		} 
		else {
			if (!DataUtil.CheckArray<bool> (meets, false)) {
				for (int i = 0; i < data.ruleData.Count; i++) {
					OnEffectTarget (AddParameter (true, data.ruleData [i], parameter));
				}
			}
		}
	}
		
	/// <summary>
	/// 決定技能效果目標
	/// <param name="data">技能效果資料</param>
	/// <param name="paramater">效果參數</param>
	private void OnEffectTarget(RuleLargeData data){
		List<int> idxList = new List<int>();
		TargetType effectTarget = TargetType.Both;
		if (data.target >= 1 && data.target <= 4) {
			effectTarget = dirTargetType;
			if (data.target > 1) {
				for (int i = 0; i < charaCount; i++) {
					idxList.Add (i);
				}
			}
		} else if (data.target > 4 && data.target <= 7) {
			effectTarget = ReverseTarget (dirTargetType);
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
			idxList.Add (dirTargetIdx);
			break;
		case (int)Target.DirTeam:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, TargetType.Player);
			return;
		case (int)Target.OnlyMate://移除發動者
			idxList.Remove (dirOrgIdx);
			break;
		case (int)Target.DirEnemy:
			selLockRuleData = data;
			fightController.OnSelectSkillTarget (idxList, TargetType.Enemy);
			return;
		case (int)Target.Trigger:
			idxList.Add (dirTargetIdx);
			break;
		}

		fightController.OnSkillEffect (dirOrgIdx, idxList, data, effectTarget);
	}



	public void SelectSkillTarget(TargetType tType, int idx){
		fightController.OnSkillEffect (dirOrgIdx, new List<int> (new int[1]{ idx }), selLockRuleData, tType);
	}

	/// <summary>
	/// 補血超過該角色上限時觸發
	/// <param name="org">補血者</param>
	/// <param name="target">被補血者</param>
	/// <param name="over">超過數值</param>
	/// <param name="tType">被補血者陣營</param>
	public void OverRecovery(int org , int target, int over, TargetType tType){
		dirOrgIdx = org;
		dirTargetIdx = target;
		dirTargetType = tType;

		if (tType == TargetType.Player) {
			if (charaTriggerSkill.ContainsKey(dirOrgIdx)) {
				foreach (RuleLargeData data in charaTriggerSkill[dirOrgIdx].ruleData) {
					if (data.rule [0] == (int)Rule.Over) {
						dirOrgData = fightController.GetSoulData (TargetType.Player, dirOrgIdx);
						OnEffectTarget (AddParameter (false, data, over));
					}
				}
			}
		} 
		else {
			if (enemyTriggerSkill.ContainsKey(dirOrgIdx)) {
				foreach (RuleLargeData data in enemyTriggerSkill[dirOrgIdx].ruleData) {
					if (data.rule [0] == (int)Rule.Over) {
						dirOrgData = fightController.GetSoulData (TargetType.Enemy, dirOrgIdx);
						OnEffectTarget (AddParameter (false, data, over));
					}
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
		foreach (KeyValuePair<string,int> kv in data.abilitys) {
			if (kv.Value != 0) {
				if (data.convType == 0) {
					return kv.Value;
				} 
				else {
					if (isRev) {
						if (dirTargetData != null) {
							return dirTargetData.abilitys [kv.Key] * kv.Value / 100;
						}
					} 
					else {
						return dirOrgData.abilitys [kv.Key] * kv.Value / 100 * dirOrgRadio / 100;
					}
				}
			}
		}
		return 0;
	}
		
	public TargetType ReverseTarget(TargetType tType){
		if (tType == TargetType.Player) {
			return TargetType.Enemy;
		} 
		else {
			return TargetType.Player;
		}
	}

	public void ShowRuleData(){
		foreach (KeyValuePair<int, SkillLargeData> kv in charaTriggerSkill) {
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
	Death = 10
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
/// Effect type.
/// None(無),Recovery(回血),Act(激活),Cover(覆蓋),RmAlarm(警戒解除)
/// ,RmNerf(異常解除),Dmg(固定傷害),Exchange(位置對調),Call(召喚)
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

