using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using model.data;

public class FightController : MonoBehaviour {
	[HideInInspector]
	public CharaLargeData[] characters;
	[HideInInspector]
	public MonsterLargeData[] monsters;

	private int[] monsterCdTimes = new int[5];

	private int[] jobRatio;

	LinkedList<int> lockOrderIdx;

	private Dictionary<int,AccordingData[]> fightPairs;

	public delegate void FightComplete ();

	public FightComplete onComplete;

	private int[] cdTime;

	private int[] skillCdTime;
	private int[] skillInitCD;

	private int[] protectJob;

	private int[] jobActLevel;

	private int[] charaFullHp;
	private int[] monsterFullHp;

	private int monsterProtect;

	private int charaProtect;

	private Dictionary<int, AccordingData[]> charaAccording;

	private Dictionary<int, AccordingData[]> monsterAccording;

	private List<int>[] charaBuffStatus;
	private List<int>[] monsterBuffStatus;

	int resetRatio;

	bool isLock = false;


	public delegate void OnShowFight(int orgIdx ,DamageData damageData, InitiatorType iType);

	public OnShowFight onShowFight;

	public delegate void OnProtect(bool hasProtect);

	public OnProtect onProtect;

	public delegate void OnSkillCDEnd(int idx);

	public OnSkillCDEnd onSkillCDEnd;

	public Dictionary<int, List<DamageData>> damages;

	public delegate void OnLockButton(int idx, InitiatorType iType);

	public OnLockButton onLockButton;

	public bool selSkillIdx = false;


	void Update(){
		if (Input.GetKeyDown (KeyCode.P)) {
			for (int i = 0; i < 5; i++) {
				Debug.Log (monsterAccording [0] [i].minus);
			}
		}
	}


	public void SetData(){

		SetCharaData ();
		SetMonsterData ();
		for (int i = 0; i < monsterCdTimes.Length; i++) {
			monsterCdTimes [i] = 5;
		}

		GetAccordingDataDic ();

		CallbackProtect ();

		UnLockOrder ();
	}

	private void SetMonsterData(){
		monsterProtect = 0;

		string enemyDataPath = "/ClientData/EnemyData.txt";

		System.IO.StreamReader sr = new System.IO.StreamReader (Application.dataPath + enemyDataPath);
		string json = sr.ReadToEnd();

		EnemyLargeData enemyData = JsonConversionExtensions.ConvertJson<EnemyLargeData>(json);

		monsters = new MonsterLargeData[enemyData.TeamData[0].Team.Count];
		monsterFullHp = new int[enemyData.TeamData[0].Team.Count];
		monsterBuffStatus = new List<int>[MyUserData.GetTeamData(0).Team.Count];

		for (int i = 0;i<enemyData.TeamData[0].Team.Count;i++) {
			monsters[i] = MasterDataManager.GetMonsterData (enemyData.TeamData[0].Team[i].id);
			monsters [i].Merge (ParameterConvert.GetMonsterAbility (monsters [i], enemyData.TeamData[0].Team[i].lv));
			monsters [i].MergeSkill (monsters [i].NorSkill); 

			monsterBuffStatus [i] = new List<int> ();
			if (monsters [i].job == 2) {
				monsterProtect++;
			}
		}
	}

	private void SetCharaData(){
		characters = new CharaLargeData[MyUserData.GetTeamData(0).Team.Count];
		charaFullHp = new int[MyUserData.GetTeamData(0).Team.Count];
		skillCdTime = new int[MyUserData.GetTeamData(0).Team.Count];
		skillInitCD = new int[MyUserData.GetTeamData(0).Team.Count];
		charaBuffStatus = new List<int>[MyUserData.GetTeamData(0).Team.Count];
		protectJob = new int[5]{ 0, 0, 0, 0, 0 };




		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters[i] = MasterDataManager.GetCharaData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].Merge (ParameterConvert.GetCharaAbility (characters [i], MyUserData.GetTeamData (0).Team [i].lv));
			characters [i].MergeSkill (characters [i].ActSkill, characters [i].NorSkill); 

			charaFullHp [i] = characters [i].hp;


			charaBuffStatus [i] = new List<int> ();
			skillCdTime [i] = (int)MasterDataManager.GetSkillData (characters [i].NorSkill).CD;
			skillInitCD [i] = (int)MasterDataManager.GetSkillData (characters [i].NorSkill).CD;
			if (characters [i].job == 2) {
				charaProtect++;
			}
		}
	}

	public void SetProtect(int[] protects){
		protectJob = protects;

		for (int i = 0; i < monsterAccording.Count; i++) {
			for (int j = 0; j < monsterAccording.ElementAt (i).Value.Length; j++) {
				monsterAccording.ElementAt (i).Value [j].minus = protectJob [characters [monsterAccording.ElementAt (i).Value [j].index].job];
			}
		}
	}

	private void OnFight(InitiatorType type){
		ShowLog (type.ToString (), 2);
		damages = new Dictionary<int, List<DamageData>> ();
		int count = type == InitiatorType.Player ? characters.Length : monsters.Length;
		for (int i = 0; i < count; i++) {
			int orgHp = type == InitiatorType.Player ? characters[i].hp : monsters[i].hp;
			if (orgHp > 0) {
				if (fightPairs.ContainsKey (i)) {
					AccordingData[] order;
					fightPairs.TryGetValue (i, out order);
					int orgJob = type == InitiatorType.Player ? characters [i].job : monsters [i].job;

					//判斷是否全體攻擊
					bool isAll = false;
					if (orgJob >= 3) {
						if (type == InitiatorType.Player) {
							isAll = true;
						}
						else {
							if (jobActLevel [i] >= 2) {
								isAll = true;
							}
						}
					}
					for (int j = 0;j<order.Length;j++) {
						int targetHp = type==InitiatorType.Player?monsters[order[j].index].hp : characters[order[j].index].hp;
						if (targetHp > 0) {
							int damage;
							float hpRatio;
							if (type == InitiatorType.Player) {
								int minus = order [j].minus;

								//敵方隊伍含有盾職減傷
								if (monsters [i].job != 3) {
									minus = minus * (10 - monsterProtect);
								}


								damage = CalDamage (characters [i].atk, monsters [order [j].index].def, jobRatio [orgJob], order [j].attriJob, order [j].minus, jobActLevel [orgJob], isAll);

								ShowLog (string.Format ("傷害值 : {0}", damage), 2);
								ShowLog (string.Format("敵人編號{0}滿血值 : {1}",order [j].index ,monsterFullHp [order [j].index]),1);
								ShowLog (string.Format ("玩家編號{0}攻擊敵人編號{1}受傷前 : {2}", i, order [j].index, monsters [order [j].index].hp), 1);

								monsters [order [j].index].hp -= damage;
								if (monsters [order [j].index].hp <= 0) {
									monsters [order [j].index].hp = 0;
									if (monsters [order [j].index].job == 2) {
										monsterProtect--;
									}
								}
								ShowLog (string.Format ("玩家編號{0}攻擊敵人編號{1}受傷後 : {2}", i, order [j].index, monsters [order [j].index].hp),1);
								hpRatio = (float)monsters [order [j].index].hp / (float)monsterFullHp [order [j].index];
								ChangeAccordingHp (order [j].index, monsters [order [j].index].hp, type);
							} 
							else {
								//計算傷害
								damage = CalDamage (monsters [i].atk, characters [order[j].index].def, 100, order[j].attriJob, order [j].minus, 0, isAll);
								ShowLog (string.Format ("傷害值 : {0}", damage), 1);
								ShowLog (string.Format("玩家編號{0}滿血值 : {1}",order [j].index ,charaFullHp [order [j].index]),1);
								ShowLog (string.Format ("敵人編號{0}攻擊玩家編號{1}受傷前 : {2}", i, order [j].index, characters [order [j].index].hp), 1);


								characters [order [j].index].hp -= damage;
								if (characters [order [j].index].hp <= 0) {
									characters [order [j].index].hp = 0;
									if (characters [order [j].index].job == 2) {
										charaProtect--;
									}
								}
								ShowLog (string.Format ("敵人編號{0}攻擊玩家編號{1}受傷後 : {2}", i, order [j].index, characters [order [j].index].hp), 1);

								hpRatio = (float)characters [order [j].index].hp / (float)charaFullHp [order [j].index];

								UseSkill (i ,monsters [i]._NorSkill, InitiatorType.Enemy);

								ChangeAccordingHp (order [j].index, characters [order [j].index].hp, type);
							}

							if (damages.ContainsKey (i)) {
								damages [i].Add (GetDamageData (order [j].index, hpRatio));
							} 
							else{
								List<DamageData> data = new List<DamageData>();
								data.Add (GetDamageData (order [j].index, hpRatio));
								damages.Add(i,data);
							}

							if (!isAll) {
								break;
							}
						}
					}
				}
			}
		}

		CallbackProtect ();
	}

	private DamageData GetDamageData(int targer, float hpRatio){
		DamageData data = new DamageData ();
		data.targetIdx = targer;
		data.hpRatio = hpRatio;

		return data;
	}

	public int CalDamage(int atk, int def, int ratio, float ratioAJ, int minus,int actLevel, bool isAll){
		int actRatio;
		if (actLevel != 0) {
			actRatio = 50 * (int)Mathf.Pow (2, actLevel - 1);
		} else {
			actRatio = 0;
		}

		float randomRatio = isAll != true ? UnityEngine.Random.Range (75, 101) : UnityEngine.Random.Range (40, 75);
		ShowLog (string.Format("加成倍率 : {0}", (randomRatio / 100) * (ratio + actRatio) / 100 * ratioAJ * (int)Mathf.Pow (1.5f, resetRatio)),1);
		int damage = Mathf.CeilToInt ((atk * (randomRatio / 100) * (ratio + actRatio) / 100 * ratioAJ * (int)Mathf.Pow (1.5f, resetRatio) - def) * (100 - minus) / 100);
		//((Atk * randamRatio * (ratio + actRatio) * ratioAJ * resetCount) - def) * minus

		return damage <= 0 ? 1 : damage;
	}

	IEnumerator ShowFight(InitiatorType iType, bool Callback){
		int count = 0;
		while (count < damages.Count) {

			foreach (DamageData data in damages[damages.ElementAt(count).Key]) {
				if (onShowFight != null) {
					onShowFight.Invoke (damages.ElementAt (count).Key, data, iType);
				}
			}
			count++;

			yield return new WaitForSeconds(0.5f);
		}


		if (Callback) {
			onShowFight = null;
			if (onComplete != null) {
				UseActSkill ();
				onComplete.Invoke ();
			}
		}
		else {
			EnemyFight ();
		}
	} 

	public void FightStart(bool lockEnemy, List<int> canAttack, int[] ratios, int[] actLevel){
		jobRatio = ratios;
		jobActLevel = actLevel;
		bool enemyFight = cdTime.Any (t => t == 0);

		if (canAttack.Count > 0) {
			FightStart (canAttack.ToArray (), InitiatorType.Player, actLevel);
			OnFight (InitiatorType.Player);
			StartCoroutine (ShowFight (InitiatorType.Player, !enemyFight));
		}
	}

	public void EnemyFight(){
		FightStart (cdTime, InitiatorType.Enemy);
		OnFight (InitiatorType.Enemy);
		StartCoroutine (ShowFight (InitiatorType.Enemy, true));
	}

	private void FightStart(int[] attackIdx, InitiatorType iType, int[] actLevel = null){
		fightPairs = new Dictionary<int, AccordingData[]> ();
		if (iType == InitiatorType.Player) {
			foreach (int idx in attackIdx) {
				fightPairs.Add (idx, matchAtkTarget (idx, iType));
			}
		} 
		else {
			for (int i = 0;i< attackIdx.Length;i++) {
				if (attackIdx [i] == 0) {
					fightPairs.Add (i, matchAtkTarget (i, iType));					
				}
			}
		}
	}

	private AccordingData[] matchAtkTarget(int idx, InitiatorType iType){

		AccordingData[] compareOrder = CompareData (idx, iType);

		int orderCount = iType == InitiatorType.Player ? monsters.Length : characters.Length;

		AccordingData[] atkOrder = new AccordingData[orderCount];
		AccordingData[] lockOrder = new AccordingData[lockOrderIdx.Count];



		if (iType == InitiatorType.Player) {
			if (!isLock) {
				atkOrder = compareOrder;
			} 
			else {
				for (int i = 0; i < lockOrderIdx.Count; i++) {
					atkOrder [i] = GetAccordingData (idx, lockOrderIdx.ElementAt (i), iType);
				}

				for (int i = lockOrderIdx.Count; i < orderCount; i++) {	
					foreach (AccordingData data in compareOrder) {
						foreach (AccordingData orderData in lockOrder) {
							if (data.index != orderData.index) {
								atkOrder [i] = data;
							}
						}
					}
				}
			}

			return atkOrder;
		} 
		else {
			return compareOrder;
		}


	}

	private void CallbackProtect(){
		onProtect.Invoke (charaProtect > 0);
	}


	private AccordingData[] CompareData(int orgIdx, InitiatorType iType){
		AccordingData[] according = iType == InitiatorType.Player ? charaAccording[orgIdx] : monsterAccording [orgIdx];

		return AccordingCompare (according, true);
	}

	private AccordingData[] AccordingCompare(AccordingData[] according, bool isPlayer = false){
		if (isPlayer) {
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.attriJob.CompareTo (y.attriJob)) * -1;
			});
		} 
		else {
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				if (x.attriJob.CompareTo (y.attriJob) == 0) {
					if (x.mAtkAtk[3].CompareTo (y.mAtkAtk[3]) == 0) {
						if (x.hp.CompareTo (y.hp) == 0) {
							if (x.minus.CompareTo (y.minus) == 0) {
								return x.crt.CompareTo (y.crt);
							} else {
								return x.minus.CompareTo (y.minus);
							}
						} else {
							return x.hp.CompareTo (y.hp);
						}
					} else {
						return(x.mAtkAtk[3].CompareTo (y.mAtkAtk[3])) * -1;
					}
				} else {
					return(x.attriJob.CompareTo (y.attriJob)) * -1;
				}
			});
		}

		return according;
	}
		
	public LinkedList<int> LockOrder (int idx){
		if (lockOrderIdx.Contains (idx)) {
			foreach (var v in lockOrderIdx) {
				if (v == idx) {
					lockOrderIdx.Remove (v);
					break;
				}
			}

		} 
		else {	
			lockOrderIdx.AddLast (idx);
		}
		isLock = lockOrderIdx.Count > 0;

		return lockOrderIdx;
	}

	public LinkedList<int> UnLockOrder(){
		lockOrderIdx = new LinkedList<int> ();
		isLock = false;
		return lockOrderIdx;
	}

	public void SetCDTime(int[] cd, bool isInit = true){
		cdTime = cd;
		if (isInit) {
			for (int i = 0; i < charaAccording.Count; i++) {
				for (int j = 0; j < charaAccording.ElementAt (i).Value.Length; j++) {
					if (cdTime [j] == 0) {
						charaAccording.ElementAt (i).Value [j].minus = 0;
					} else {
						charaAccording.ElementAt (i).Value [j].minus = 50;
					}
				}
			}
		}
	}

	public void SetResetRatio(int count){
		resetRatio = count;
	}

	struct AccordingData{
		public int index;
		public float attriJob;
		public int[] mAtkAtk;
		public int hp;
		public int minus;
		public int crt;
	}

	private void ChangeAccordingHp(int idx, int hp, InitiatorType iType){
		if (iType == InitiatorType.Player) {
			for (int i = 0; i < charaAccording.Count; i++) {
				for (int j = 0; j < charaAccording.ElementAt (i).Value.Length; j++) {
					if (charaAccording.ElementAt (i).Value [j].index == idx) {
						charaAccording.ElementAt (i).Value [j].hp = hp;
					}
				}
			}
		} 
		else {
			for (int i = 0; i < monsterAccording.Count; i++) {
				for (int j = 0; j < monsterAccording.ElementAt (i).Value.Length; j++) {
					if (monsterAccording.ElementAt (i).Value [j].index == idx) {
						monsterAccording.ElementAt (i).Value [j].hp = hp;
					}
				}
			}
		}
	}

	private void GetAccordingDataDic(){
		charaAccording = new Dictionary<int, AccordingData[]> ();
		monsterAccording = new Dictionary<int, AccordingData[]> ();
		for (int i = 0; i < characters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<monsters.Length; j++){
				data [j] = GetAccordingDataDic (i, j, InitiatorType.Player);
			}
			charaAccording.Add (i, data);
		}

		for (int i = 0; i < monsters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<characters.Length; j++){
				data [j] = GetAccordingDataDic (i, j, InitiatorType.Enemy);
			}
			monsterAccording.Add (i, data);
		}
	}

	private AccordingData GetAccordingDataDic(int orgIdx, int idx, InitiatorType iType){
		AccordingData data = new AccordingData ();
		data.index = idx;
		if (iType == InitiatorType.Player) {
			data.attriJob = GetCalcRatio (characters [orgIdx].job, monsters [idx].job, characters [orgIdx].attributes, monsters [idx].attributes);
			data.mAtkAtk = new int[3] { monsters [idx].mAtk, monsters [idx].atk, monsters [idx].mAtk + monsters [idx].atk };
			data.hp = monsters [idx].hp;
			data.minus = cdTime [idx] == 0 ? 0 : 50;
			data.crt = monsters [idx].crt;
		} else {
			data.attriJob = GetCalcRatio (monsters [orgIdx].job, characters [idx].job, monsters [orgIdx].attributes, characters [idx].attributes);
			data.mAtkAtk = new int[3] { characters [idx].mAtk, characters [idx].atk, characters [idx].mAtk + characters [idx].atk };
			data.hp = characters [idx].hp;
			data.minus = protectJob [characters [idx].job];
			data.crt = characters [idx].crt;
		}

		return data;
	}

	private AccordingData GetAccordingData(int orgIdx, int idx, InitiatorType iType){
		if (iType == InitiatorType.Player) {
			return charaAccording [orgIdx] [idx];
		}
		else {
			return monsterAccording [orgIdx] [idx];
		}
	}

	public void FightEnd(){
		UseActSkill ();
		for (int i = 0; i < skillCdTime.Length; i++) {
			if (skillCdTime [i] > 0) {
				skillCdTime [i]--;
				if (skillCdTime [i] == 0) {
					if (onSkillCDEnd != null) {
						onSkillCDEnd.Invoke (i);
					}
				}
			}
		}
	}

	public void UseActSkill(){
		for (int i = 0;i<characters.Length;i++) {
			if (jobRatio [characters[i].job] > 0 && characters[i].hp > 0) {
				UseSkill (i ,characters[i]._ActSkill);
			}
		}
	}

	private float GetCalcRatio(int aj, int bj, int aa, int ba){
		return ParameterConvert.AttriRatioCal (aa, ba)*ParameterConvert.JobRatioCal (aj, bj);
	}


	private void ShowLog(string content,int type = 0){
		/*if (type == 0) {
			Debug.Log (content);
		} else if (type == 1) {
			Debug.LogWarning (content);
		} else {
			Debug.LogError (content);
		}*/
	}

	public void UseSkill(int idx, InitiatorType iType){
		UseSkill (idx, characters [idx]._NorSkill);
	}

	public void UseSkill(int idx, SkillLargeData skillData,InitiatorType iType = InitiatorType.Player){
		foreach (RuleLargeData ruleData in skillData.ruleData) {
			if (ruleData.isBuff) {
				OnSkillBuff (idx, ruleData, iType);
			}
		}
	}

	public void SelectSkillTarget (int charaIdx){
		
	}

	public void OnSkillBuff(int idx,RuleLargeData ruleData, InitiatorType iType){
		if (ruleData.RuleType [0] != 0) {
			CheckSkillRule (ruleData.RuleType, iType);
		}
	}

	public void CheckSkillRule(int[] ruleType, InitiatorType iType){		
	}

	/// <summary>
	/// Rule type.
	/// None 無
	/// HpLess 血量(少於)
	/// HpBest 血量(多於含)
	/// Nerf 異常狀態
	/// norDmg 自然傷害值(自)
	/// norDmgP 自然傷害值(物自)
	/// norDmgM 自然傷害值(魔自)
	/// SpDmg 自然傷害值(他)
	/// DeathCount 隊友死亡數
	/// Over 溢補值
	/// SelfDeath 自己死亡
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
	/// None(無),Self(自身),DirMate(指定隊友),OnlyMate(僅隊友),Team(全隊),Enemy(全敵人),DirEnemy(指定敵人),Trigger(觸發者)
	/// </summary>
	private enum targetType {
		None = 0,
		Self = 1,
		DirMate = 2,
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
		Call = 14,
		Buff = 25,
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

	public void ShowHasAnim(){
		foreach (var data in characters) {
			Debug.Log (data._NorSkill.hasAnim);
		}
	}
}

public enum InitiatorType{
	Player,
	Enemy
}

public struct DamageData{
	public int targetIdx;
	public float hpRatio;
}