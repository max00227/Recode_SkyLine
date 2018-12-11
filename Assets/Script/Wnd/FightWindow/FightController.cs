using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using model.data;

public class FightController : MonoBehaviour {
	[SerializeField]
	SkillController skillController;

	[SerializeField]
	FightUIController fightUIController;

	/*[HideInInspector]
	private SoulLargeData[] characters;
	[HideInInspector]
	private SoulLargeData[] enemys;*/

	private int[] enemyCdTimes = new int[5];

	LinkedList<int> lockOrderIdx = new LinkedList<int>();

	private Dictionary<int,AccordingData[]> fightPairs;

	private int[] cdTime;

	private int[] skillCdTime;
	private int[] skillInitCD;

	private int[] protectChara;

	private int enemyProtect;

	private int charaProtect;


	[HideInInspector]
	public ChessData[] characters;
	[HideInInspector]
	public ChessData[] enemys;



	float resetRatio;

	bool isLock = false;

	public Dictionary<int, Dictionary<int, List<DamageData>>> damageShowSort;

	public Dictionary<int, List<RuleLargeData>> charaTriggers;
	public Dictionary<int, List<RuleLargeData>> enemyTriggers;



	public FightStatus fightStatus;


	void Update(){
		if (Input.GetKeyDown (KeyCode.P)) {
			for (int i = 0; i < 5; i++) {
				Debug.Log (enemys [0].according [i].minus);
			}
		}
	}


	public void SetData(){
		resetRatio = 1;

		skillController.SetData (SetCharaData (), SetEnemyData ());

		for (int i = 0; i < enemyCdTimes.Length; i++) {
			enemyCdTimes [i] = 5;
		}

		SetAccordingDataDic ();

		CallbackProtect ();

		UnLockOrder ();
	}

	private SoulLargeData[] SetEnemyData(){
		
		enemyProtect = 0;

		string enemyDataPath = "/ClientData/EnemyData.txt";

		System.IO.StreamReader sr = new System.IO.StreamReader (Application.dataPath + enemyDataPath);
		string json = sr.ReadToEnd();

		EnemyLargeData enemyData = JsonConversionExtensions.ConvertJson<EnemyLargeData>(json);

		SoulLargeData[] soulData = new SoulLargeData[enemyData.TeamData[0].Team.Count];
		enemys = new ChessData[enemyData.TeamData [0].Team.Count];

		for (int i = 0;i<enemyData.TeamData[0].Team.Count;i++) {
			enemys[i].soulData = MasterDataManager.GetSoulData (enemyData.TeamData[0].Team[i].id);
			enemys[i].soulData.Merge (ParameterConvert.GetEnemyAbility (enemys[i].soulData, enemyData.TeamData[0].Team[i].lv));
			enemys[i].soulData.Merge (enemys[i].soulData.actSkill, enemys[i].soulData.norSkill);
			enemys[i].fullHp = enemys[i].soulData.abilitys["Hp"];
			enemys [i].status = new List<int> ();
			enemys [i].abiChange = new Dictionary<string, int> ();
			soulData [i] = enemys [i].soulData;


			if (enemys[i].soulData.job == 2) {
				enemyProtect++;
			}
		}
		return soulData;
	}

	private SoulLargeData[] SetCharaData(){
		SoulLargeData[] soulData = new SoulLargeData[MyUserData.GetTeamData(0).Team.Count];
		skillCdTime = new int[MyUserData.GetTeamData(0).Team.Count];
		skillInitCD = new int[MyUserData.GetTeamData(0).Team.Count];
		characters = new ChessData[MyUserData.GetTeamData (0).Team.Count];
		protectChara = new int[MyUserData.GetTeamData(0).Team.Count];


		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters [i].soulData = MasterDataManager.GetSoulData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].soulData.Merge (ParameterConvert.GetCharaAbility (characters [i].soulData, MyUserData.GetTeamData (0).Team [i].lv));
			characters [i].soulData.Merge (characters [i].soulData.actSkill, characters [i].soulData.norSkill);
			characters [i].fullHp = characters [i].soulData.abilitys["Hp"];
			characters[i].initCD = (int)characters [i].soulData._norSkill.cdTime;
			characters [i].status = new List<int> ();
			characters [i].abiChange = new Dictionary<string, int> ();
			soulData [i] = characters [i].soulData;


			skillCdTime [i] = (int)characters [i].soulData._norSkill.cdTime;
		}
		return soulData;
	}

	public void SetProtect(int[] jobProtects){
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < characters.Length; j++) {
				if (characters [j].soulData.job == i) {
					protectChara [j] = jobProtects [i];
				}
			}
		}

		for (int i = 0; i < protectChara.Length; i++) {
			ChangeAccordingData (i, protectChara [i], TargetType.Player, AccChangeType.Minus);
		}
	}

	/// <summary>
	/// 計算傷害值
	/// <param name="type">目標類型</param>
	private void OnFight(TargetType tType){
		damageShowSort = new Dictionary<int, Dictionary<int, List<DamageData>>> ();
		int count = tType == TargetType.Player ? enemys.Length : characters.Length;
		for (int i = 0; i < count; i++) {
			SoulLargeData orgData = tType == TargetType.Player ? enemys[i].soulData : characters[i].soulData;
			if (orgData.abilitys["Hp"] > 0) {
				if (fightPairs.ContainsKey (i)) {
					AccordingData[] order;
					fightPairs.TryGetValue (i, out order);

					//判斷是否全體攻擊
					bool isAll = false;
					if (orgData.job >= 3) {
						if (tType == TargetType.Enemy) {
							if (fightUIController.GetActLevel (orgData.job) >= 2) {
								isAll = true;
							}
						}
						else {
							isAll = true;
						}
					}
						
					for (int j = 0; j < order.Length; j++) {
						SoulLargeData targetData = tType == TargetType.Player ? characters [order[j].index].soulData : enemys [order[j].index].soulData;
						if (targetData.abilitys["Hp"] > 0) {
							List<DamageData> allDamage = new List<DamageData> ();
							if (orgData.job <= 3) {
								allDamage.Add (GetDamage (orgData, targetData, i, order [j].index, order [j].attriRatio * order [j].jobRatio, order [j].minus, tType, DamageType.Physical, isAll));
							}

							if (orgData.job >= 3) {
								allDamage.Add (GetDamage (orgData, targetData, i, order [j].index, order [j].attriRatio * order [j].jobRatio, order [j].minus, tType, DamageType.Magic, isAll));
							}

							OnDamage (targetData, allDamage);
						} 


						if (!isAll) {
							break;
						}
					}
				}
			}
		}

		CallbackProtect ();
	}

	/// <summary>
	/// 建立傷害資料
	/// <returns>The damage.</returns>
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="orgIdx">攻擊者索引值，建立傷害資料用</param>
	/// <param name="targetIdx">被攻擊者索引值，建立傷害資料用</param>
	/// <param name="attriJob">攻剋倍率</param>
	/// <param name="minus">檢傷值</param>
	/// <param name="tType">目標陣營.</param>
	/// <param name="dType">傷害類型</param>
	/// <param name="isAll">是否為全體攻擊，會影響浮動值</param>
	private DamageData GetDamage (SoulLargeData orgData, SoulLargeData targetData, int orgIdx, int targetIdx,float attriJob, int minus, TargetType tType, DamageType dType, bool isAll){
		DamageData damageData;
		int actLevel = tType == TargetType.Player ? 0 : fightUIController.GetActLevel (orgData.job);
		int ratio = tType == TargetType.Player ? 0 : fightUIController.GetCharaRatio (orgData.job);
		int orgChanged = 100;
		int targetChanged= 100;
		string titleKey;
		if (dType == DamageType.Physical) {
			titleKey = "";
		} else {
			titleKey = "m";
		}

		if (tType == TargetType.Player) {
			orgChanged = enemys[orgIdx].abiChange.ContainsKey (titleKey + "Atk") == true ? enemys[orgIdx].abiChange [titleKey + "Atk"] : orgChanged;
			orgChanged = characters[targetIdx].abiChange.ContainsKey (titleKey + "Atk") == true ? characters[targetIdx].abiChange [titleKey + "Atk"] : targetChanged;
		} else {
			orgChanged = characters[orgIdx].abiChange.ContainsKey (titleKey + "Atk") == true ? characters[orgIdx].abiChange [titleKey + "Atk"] : orgChanged;
			targetChanged = enemys[targetIdx].abiChange.ContainsKey (titleKey + "Def") == true ? enemys[targetIdx].abiChange [titleKey + "Def"] : targetChanged;
		}
			
		if (dType == DamageType.Physical) {
			damageData = CalDamage (orgData.abilitys ["Atk"] * orgChanged / 100, targetData.abilitys ["Def"] * targetChanged / 100, ratio, attriJob, minus, actLevel, orgData.abilitys ["Cri"], isAll);
			damageData.damageType = DamageType.Physical;
		} 
		else {
			damageData = CalDamage (orgData.abilitys ["mAtk"] * orgChanged / 100, targetData.abilitys ["mDef"] * targetChanged / 100, ratio, attriJob, minus, actLevel, orgData.abilitys ["Cri"], isAll);
			damageData.damageType = DamageType.Magic;
		}

		damageData.tType = tType;
		damageData.attributes = tType == TargetType.Player ? orgData.act [0] : orgData.act [actLevel - 1];
		damageData.orgIdx = orgIdx;
		damageData.targetIdx = targetIdx;

		return damageData;
	}

	private void OnDamage (SoulLargeData targetData, List<DamageData> allDamage){
		if (damageShowSort.ContainsKey (allDamage[0].orgIdx)) {
			damageShowSort [allDamage[0].orgIdx].Add (allDamage[0].targetIdx, OnDamageList (targetData, allDamage));
		} 
		else {
			damageShowSort.Add (allDamage[0].orgIdx, new Dictionary<int, List<DamageData>> ());
			damageShowSort [allDamage[0].orgIdx].Add (allDamage[0].targetIdx, OnDamageList (targetData, allDamage));
		}
	}

	private List<DamageData> OnDamageList(SoulLargeData targetData, List<DamageData> allDamage){
		List<DamageData> damageList = new List<DamageData> ();
		foreach (DamageData damageData in allDamage) {
			damageList.Add (OnDamageData (targetData, damageData));
		}

		return damageList;
	}


	private DamageData OnDamageData(SoulLargeData targetData, DamageData damageData){
		DamageData data = damageData;
		bool isDead = false;
		targetData.abilitys["Hp"] -= data.damage;
		if (targetData.abilitys["Hp"] <= 0) {
			targetData.abilitys["Hp"] = 0;
			isDead = true;
			if (targetData.job == 2) {
				if (damageData.tType == TargetType.Player) {
					charaProtect--;
				}
				else{
					enemyProtect--;
				}
			}
		}

		if (isDead) {
			OnDeath (damageData.targetIdx, damageData.tType);
		}

		if (damageData.tType == TargetType.Player) {
			ChangeAccordingData (damageData.targetIdx, targetData.abilitys["Hp"], damageData.tType, AccChangeType.Hp);
			data.hpRatio = (float)targetData.abilitys["Hp"] / (float)characters [damageData.targetIdx].fullHp;
			return data;
		} 
		else {
			ChangeAccordingData (damageData.targetIdx, targetData.abilitys["Hp"], damageData.tType, AccChangeType.Hp);
			data.hpRatio = (float)targetData.abilitys["Hp"] / (float)enemys [damageData.targetIdx].fullHp;
			return data;
		}

	}

	private void OnDeath(int idx, TargetType tType){
		fightUIController.OnDead(idx,tType);
	}

	/// <summary>
	/// 計算傷害值
	public DamageData CalDamage(int atk, int def, int ratio, float ratioAJ, int minus,int actLevel, int crt, bool isAll){
		Debug.LogWarning (atk);
		DamageData damageData = new DamageData ();
		int actRatio;
		if (actLevel != 0) {
			actRatio = 50 * (int)Mathf.Pow (2, actLevel - 1);
		} else {
			actRatio = 0;
		}
		bool isCrt = UnityEngine.Random.Range (0, 101) <= crt;

		damageData.isCrt = isCrt;
		float crtRatio = Mathf.Pow (1.5f, Convert.ToInt32 (isCrt));

		float randomRatio = isAll != true ? UnityEngine.Random.Range (75, 101) : UnityEngine.Random.Range (40, 75);

		int damage = Mathf.CeilToInt ((atk * (randomRatio / 100) * (100 + ratio + actRatio) / 100 * ratioAJ * resetRatio * crtRatio - def) * (100 - minus) / 100);
		//((Atk * randamRatio * (100 + ratio + actRatio) * ratioAJ * resetCount) * isCrt - def) * minus
		damageData.damage = damage <= 0 ? 1 : damage;

		return damageData;
	}

	/// <summary>
	/// 表現傷害動畫
	/// <returns>The fight.</returns>
	/// <param name="tType">被攻擊者陣營</param>
	/// <param name="Callback">是否戰鬥結束的Callback</param>
	IEnumerator ShowFight(TargetType tType, bool Callback){
		int count = 0;
		while (count < damageShowSort.Count) {
			foreach (KeyValuePair<int, Dictionary<int, List<DamageData>>> data in damageShowSort) {
				ShowFight (data.Key, data.Value, tType);
				count++;
				yield return new WaitForSeconds(0.5f);
			}
		}

		if (Callback) {
			fightUIController.FightEnd ();

		}
		else {
			EnemyFight ();
		}
	} 

	/// <summary>
	/// 回調UI執行傷害動畫
	/// <param name="orgIdx">Org index.</param>
	/// <param name="damageData">Damage data.</param>
	/// <param name="tType">T type.</param>
	void ShowFight(int orgIdx, Dictionary<int, List<DamageData>> damageData, TargetType tType){
		foreach (KeyValuePair<int, List<DamageData>> data in damageData) {
			fightUIController.OnShowFight(data.Value);
		}
	}
		
	/// <summary>
	/// 開始進行戰鬥，先決定可攻擊者的攻擊目標順序，然後計算傷害值，最後執行戰鬥動畫
	/// <param name="lockEnemy">玩家是否有鎖定敵人</param>
	/// <param name="canAttack">玩家角色是否有可能攻擊者</param>
	/// <param name="ratios">職業攻擊力加成</param>
	/// <param name="actLevel">攻擊者攻擊階級</param>
	public void FightStart(bool lockEnemy, List<int> canAttack){
		bool enemyFight = DataUtil.CheckArray<int> (cdTime, 0);

		if (canAttack.Count > 0) {
			FightPairs (canAttack.ToArray (), TargetType.Enemy);
			OnFight (TargetType.Enemy);
			StartCoroutine (ShowFight (TargetType.Enemy, !enemyFight));
		} 
	}

	/// <summary>
	/// 敵人攻擊
	public void EnemyFight(){
		FightPairs (cdTime, TargetType.Player);
		OnFight (TargetType.Player);
		StartCoroutine (ShowFight (TargetType.Player, true));
	}

	/// <summary>
	/// 攻擊目標配對
	/// <param name="attackIdx">可攻擊清單</param>
	/// <param name="tType">目標陣營.</param>
	/// <param name="actLevel">攻擊者攻擊階級<param>
	private void FightPairs(int[] attackIdx, TargetType tType){
		fightPairs = new Dictionary<int, AccordingData[]> ();
		if (tType == TargetType.Enemy) {
			foreach (int idx in attackIdx) {
				fightPairs.Add (idx, matchTarget (idx, tType));
			}
		} 
		else {
			for (int i = 0;i< attackIdx.Length;i++) {
				if (attackIdx [i] == 0) {
					fightPairs.Add (i, matchTarget (i, tType));					
				}
			}
		}
	}

	/// <summary>
	/// 配對攻擊目標並排列順序
	/// <returns>The target.</returns>
	/// <param name="idx">攻擊者索引值</param>
	/// <param name="tType">被攻擊者陣營</param>
	private AccordingData[] matchTarget(int idx, TargetType tType){

		AccordingData[] compareOrder = CompareData (idx, tType);

		int orderCount = tType == TargetType.Player ? characters.Length : enemys.Length;

		AccordingData[] atkOrder = new AccordingData[orderCount];
		AccordingData[] lockOrder = new AccordingData[lockOrderIdx.Count];



		if (tType == TargetType.Player) {
			return compareOrder;
		} 
		else {
			if (!isLock) {
				atkOrder = compareOrder;
			} 
			else {
				for (int i = 0; i < lockOrderIdx.Count; i++) {
					atkOrder [i] = GetAccordingData (idx, lockOrderIdx.ElementAt (i), tType);
				}

				int orderIdx = lockOrderIdx.Count;
				foreach (AccordingData data in compareOrder) {
					bool isHad = false;
					for (int i = 0; i < lockOrderIdx.Count; i++) {
						if (data.index == atkOrder [i].index) {
							isHad = true;
						}
					}

					if (!isHad) {
						atkOrder [orderIdx] = data;
						orderIdx++;
					}
				}
			}

			return atkOrder;
		}
	}

	private void CallbackProtect(){
		fightUIController.GetHasProtect(charaProtect > 0);
	}

	/// <summary>
	/// 建立比對值進行排序
	/// <returns>The data.</returns>
	/// <param name="orgIdx">攻擊者索引值</param>
	/// <param name="tType">被攻擊者陣營</param>
	private AccordingData[] CompareData(int orgIdx, TargetType tType){
		//因應玩家角色攻擊屬性會變換更改According資料
		if (tType == TargetType.Enemy) {
			for (int i = 0; i < characters [orgIdx].according.Length; i++) {
				characters [orgIdx].according[i] = ChangeAccordingData (
					characters [orgIdx].according[i], 
					ParameterConvert.AttriRatioCal (characters [orgIdx].soulData.act [fightUIController.GetActLevel(characters [orgIdx].soulData.job)-1], enemys [i].soulData.attributes)
					, AccChangeType.AttriRatio
				);
			}
		}
        AccordingData[] according = new AccordingData[0];

		according = tType == TargetType.Player ? (AccordingData[])enemys[orgIdx].according.Clone() : (AccordingData[])characters[orgIdx].according.Clone();

		return AccordingCompare (according, tType);
	}

	/// <summary>
	/// 進行目標排序，目標為敵人時因為玩家可做簡單排序，因此只對克制加成做排序，若目標為玩家會以克制加成>>當下血量>>爆擊值>>減傷>>攻擊傷害順序做排列
	/// <returns>The compare.</returns>
	/// <param name="according">根據值</param>
	/// <param name="isPlayer">被攻擊者陣營</param>
	private AccordingData[] AccordingCompare(AccordingData[] according, TargetType tType){
		if (tType == TargetType.Enemy) {
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return((x.attriRatio * x.jobRatio).CompareTo (y.attriRatio * y.jobRatio)) * -1;
			});
		} 
		else {
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				if ((x.attriRatio * x.jobRatio).CompareTo (y.attriRatio * y.jobRatio) == 0) {
					if (x.mAtkAtk.CompareTo (y.mAtkAtk) == 0) {
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
						return(x.mAtkAtk.CompareTo (y.mAtkAtk)) * -1;
					}
				} else {
					return((x.attriRatio * x.jobRatio).CompareTo (y.attriRatio * y.jobRatio)) * -1;
				}
			});
		}

		return according;
	}
		
	/// <summary>
	/// 鎖定排序
	/// <param name="idx">選取敵人索引值</param>
	/// <param name="isDead">是否死亡</param>
	public void LockOrder (int idx, bool isDead = false){
		if (lockOrderIdx.Contains (idx)) {
			foreach (var v in lockOrderIdx) {
				if (v == idx) {
					lockOrderIdx.Remove (v);
					fightUIController.SetUnLockUI (idx);
					break;
				}
			}

		} 
		else {
			if (isDead) {
				return;
			}
			if (lockOrderIdx.Count < 3) {
				lockOrderIdx.AddLast (idx);
			} 
			else {
				UnLockOrder ();
				return;
			}
		}
		isLock = lockOrderIdx.Count > 0;

		fightUIController.SetLockUI (lockOrderIdx);
	}

	public void UnLockOrder(){
		lockOrderIdx = new LinkedList<int> ();
		isLock = false;
		fightUIController.SetLockUI (lockOrderIdx);
	}

	public void SetCDTime(int[] cd, bool isInit = true){
		cdTime = cd;
		if (isInit) {
			for (int i = 0; i < characters.Length; i++) {
				for (int j = 0; j < characters[i].according.Length; j++) {
					if (cdTime [j] == 0) {
						ChangeAccordingData (j, 0 + enemyProtect * 10, TargetType.Enemy, AccChangeType.Minus);
					} 
					else {
						ChangeAccordingData (j, 50 * (10 + enemyProtect) / 10, TargetType.Enemy, AccChangeType.Minus);
					}
				}
			}
		}
	}

	/// <summary>
	/// 加快該局後期節奏，會因重製版面次數影響倍率
	/// <param name="count">Count.</param>
	public void SetResetRatio(int count){
		resetRatio = Mathf.Pow (1.5f, count);
	}

	private enum AccChangeType{
		AttriRatio,
		JobRatio,
		MAtkAtk,
		Hp,
		Minus,
		Crt
	}

	/// <summary>
	/// 變更根據值
	/// <param name="idx">Index.</param>
	/// <param name="hp">Hp.</param>
	/// <param name="tType">T type.</param>
	private void ChangeAccordingData(int idx, int parameter, TargetType tType, AccChangeType acType){
		if (tType == TargetType.Enemy) {
			for (int i = 0; i < characters.Length; i++) {
				for (int j = 0; j < characters[i].according.Length; j++) {
					if (idx != null) {
						characters[i].according [j] = ChangeAccordingData (characters[i].according [j], parameter, acType);
					} 
				}
			}
		} 
		else {
			for (int i = 0; i < enemys.Length; i++) {
				for (int j = 0; j < enemys[i].according.Length; j++) {
					if (idx != null) {
						if (acType != AccChangeType.AttriRatio) {
							enemys [i].according [j] = ChangeAccordingData (enemys [i].according [j], parameter, acType);
						}
					}
					else {
						enemys [i].according [j] = ChangeAccordingData (enemys [i].according [j], parameter, acType);
					}
				}
			}
		}
	}

	private AccordingData ChangeAccordingData(AccordingData data, float param, AccChangeType acType){
		AccordingData accData = data;
		switch (acType) {
		case AccChangeType.AttriRatio:
			accData.attriRatio = param;
			break;
		case AccChangeType.JobRatio:
			accData.jobRatio = param;
			break;
		case AccChangeType.MAtkAtk:
			accData.mAtkAtk = Convert.ToInt32 (param);
			break;
		case AccChangeType.Hp:
			accData.hp = Convert.ToInt32 (param);;
			break;
		case AccChangeType.Minus:
			accData.minus = Convert.ToInt32 (param);;
			break;
		case AccChangeType.Crt:
			accData.crt = Convert.ToInt32 (param);;
			break;
		}
		return accData;
	}

	/// <summary>
	/// 建立各角色對相對陣營的攻擊順序根據值清單
	private void SetAccordingDataDic(){
		
		ChessData[] charaData = new ChessData[0];
		charaData = (ChessData[])characters.Clone();
		ChessData[] enemyData = new ChessData[0];
		enemyData = (ChessData[])enemys.Clone();
		for (int i = 0; i < charaData.Length; i++) {
			AccordingData[] data = new AccordingData[enemys.Length];
			for(int j = 0;j<enemyData.Length; j++){
				data[j] = GetAccording(charaData[i].soulData, enemyData[j].soulData, j, TargetType.Player);
			}
			characters [i].according = data;
		}

		for (int i = 0; i < enemyData.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<charaData.Length; j++){
				data[j] = GetAccording(enemyData[i].soulData, charaData[j].soulData, j, TargetType.Enemy);
			}
			enemys [i].according = data;
		}
	}

	/// <summary>
	/// 建立攻擊順序根據值
	/// <returns>The according.</returns>
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="targetIdx">被攻擊者索引值</param>
	/// <param name="tType">目標陣營，減傷值會因陣營有所不同</param>
	private AccordingData GetAccording(SoulLargeData orgData, SoulLargeData targetData, int targetIdx,TargetType tType){
        
		AccordingData data = new AccordingData ();
		data.index = targetIdx;
		data.mAtkAtk = targetData.abilitys["mAtk"] + targetData.abilitys["Atk"];
		data.hp = targetData.abilitys["Hp"];
		data.crt = targetData.abilitys["Cri"];
		data.jobRatio = ParameterConvert.JobRatioCal (orgData.job, targetData.job);
		if (tType == TargetType.Player) {
			data.attriRatio = 1;
			data.minus = cdTime [targetIdx] == 0 ? 0 + enemyProtect * 10 : 50 * (10 + enemyProtect) / 10;
		} 
		else {
			data.attriRatio = ParameterConvert.AttriRatioCal (orgData.act [0], targetData.attributes);
			data.minus = protectChara [targetIdx];
		}

		return data;
	}

	/// <summary>
	/// 取得根據值資料
	/// <returns>The according data.</returns>
	/// <param name="orgIdx">攻擊者索引</param>
	/// <param name="idx">被攻擊者索引</param>
	/// <param name="tType">被攻擊者索引值</param>
	private AccordingData GetAccordingData(int orgIdx, int targetIdx, TargetType tType){
		if (tType == TargetType.Enemy) {
			return characters[orgIdx].according[targetIdx];
		}
		else {
			return enemys[orgIdx].according[targetIdx];
		}
	}

	public void FightEnd(){
		skillController.OnRoundSkill ();
		for (int i = 0; i < skillCdTime.Length; i++) {
			if (skillCdTime [i] > 0) {
				skillCdTime [i]--;
				if (skillCdTime [i] == 0) {
					fightUIController.OnSkillCDEnd (i);
				}
			}
		}
	}

	private float GetCalcRatio(int aj, int bj, int aa, int ba){
		return ParameterConvert.AttriRatioCal (aa, ba)*ParameterConvert.JobRatioCal (aj, bj);
	}


	private void ShowLog(string content,int type = 0){
		if (type == 0) {
			Debug.Log (content);
		} else if (type == 1) {
			Debug.LogWarning (content);
		} else {
			Debug.LogError (content);
		}
	}

	public void SelectSkillTarget(TargetType tType, int idx){
		skillController.SelectSkillTarget (tType, idx);
	}

	public void OnSelectSkillTarget(List<int> idxList, TargetType tType){
		fightUIController.OnSelectionDir (ExcludeTarget (idxList, tType), tType);
	}

	public List<int> ExcludeTarget(List<int> idxList, TargetType tType){
		int count = tType == TargetType.Player ? characters.Length : enemys.Length;


		for(int i = 0;i<count;i++){
			if (tType == TargetType.Player) {
				if (characters [i].exclube == true) {
					idxList.Remove (i);
				}
			} else {
				if (enemys [i].exclube == true) {
					idxList.Remove (i);
				}
			}
		}

		return idxList;
	}


	/// <summary>
	/// 技能條件是否符合
	/// <param name="idx">Index.</param>
	/// <param name="ruleId">Rule identifier.</param>
	/// <param name="param">Parameter.</param>
	/// <param name="tType">T type.</param>
	public bool OnRuleMeets(int idx ,int ruleId, int param, TargetType tType){
		if (tType == TargetType.Player) {
			return OnCharacterRule (idx, ruleId, param);
		} 
		else {
			return OnenemyRule (idx, ruleId, param);
		}
	}

	public bool OnCharacterRule(int idx ,int ruleId, int param){
		if (fightUIController.GetActLevel(characters [idx].soulData.job) > 0) {
			switch (ruleId) {
			case 0:
				return true;
			case 1:
				return (characters [idx].soulData.abilitys["Hp"] / characters [idx].fullHp * 100) < param;
			case 2:
				return (characters [idx].soulData.abilitys["Hp"] / characters [idx].fullHp * 100) >= param;
			}
		}
		return false;
	}

	public bool OnenemyRule(int idx ,int ruleId, int param){
		switch (ruleId) {
		case 1:
			return (enemys [idx].soulData.abilitys["Hp"] / enemys [idx].fullHp * 100) < param;
		case 2:
			return (enemys [idx].soulData.abilitys["Hp"] / enemys [idx].fullHp * 100) >= param;
		}
		return false;
	}

	public void AddExclude(int idx, TargetType tType){
		
	}

	public void RoundEnd(){
		skillController.OnRoundSkill ();
		for (int i = 0; i < skillCdTime.Length; i++) {
			if (skillCdTime[i] > 0) {
				skillCdTime [i]--;
			}
		}
	}

	public void OnTriggerSkill(List<DamageData> allDamage){
		ChessData orgData;
		ChessData targetData;
		if (allDamage [0].tType == TargetType.Enemy) {
			orgData = characters [allDamage [0].orgIdx];
			targetData = enemys [allDamage [0].targetIdx];

		} 
		else {
			orgData = enemys [allDamage [0].orgIdx];
			targetData = characters [allDamage [0].targetIdx];
		}
		skillController.OnTriggerSkill (orgData, targetData, allDamage);
	}

	public ChessData GetChessData(TargetType tType,int index){
		if (tType == TargetType.Player) {
			return characters [index];
		} 
		else {
			return enemys [index];
		}
	}

	public int GetRadio(TargetType tType,int index){
		if (tType == TargetType.Player) {
			return fightUIController.GetCharaRatio (characters [index].soulData.job);
		} 
		else {
			return 10000;
		}
	}

	public int GetJob(TargetType tType,int index){
		if (tType == TargetType.Player) {
			return characters [index].soulData.job;
		} 
		else {
			return enemys [index].soulData.job;
		}
	}


	#region Skill
	/// <summary>
	/// 發動效果，技能分為一般跟狀態類效果
	/// <param name="orgIdx">發動者索引</param>
	/// <param name="idxList">對象清單</param>
	/// <param name="data">技能效果資料</param>
	/// <param name="targetType">目標類型 玩家：敵人</param>
	/// <param name="paramater">效果參數</param>
	public void OnSkillEffect(int orgIdx, List<int> idxList, RuleLargeData data, TargetType targetType ,int skillId){
		if (data.effectType == 1) {
			foreach (int idx in idxList) {
				if (characters[idx].status == null || !characters[idx].status.Contains (skillId)) {
					if (characters[idx].status == null) {
						characters[idx].status = new List<int> ();
					}
					characters[idx].status.Add (skillId);
					OnStatus (orgIdx, idx, data, targetType);
				}
			}
		} 
		else {
			foreach (int idx in idxList) {
				OnNormal (orgIdx, idx, data, targetType);
			}
		}
	}


	/// <summary>
	/// 一般類型既能函式
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.Recovery = 1,Act = 2,Cover = 3,RmAlarm = 4,RmNerf = 5,Dmg = 6,Exchange = 7,Call = 8</param>
	/// <param name="paramater">效果參數.</param>
	private void OnNormal(int orgIdx, int idx, RuleLargeData data, TargetType tType){
		switch (data.effect[0]) {
		case (int)Normal.Recovery:
			OnRecovery (orgIdx, idx, data, tType);
			break;
		}
	}

	/// <summary>
	/// 狀態附加類型技能函式
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.UnDef = 1,UnNerf = 2,AddNerf = 3,Suffer = 4,Maximum = 5,Ability = 6,UnDirect = 7</param>
	/// <param name="paramater">效果參數.</param>
	private void OnStatus(int orgIdx, int idx, RuleLargeData data, TargetType tType){
		switch (data.effect [0]) {
		case (int)Status.Ability:
			OnAbilityChanged (idx, data.abilitys, tType);
			break;
		}
	}

	private void OnRecovery(int orgIdx, int idx, RuleLargeData data, TargetType tType){
		int over = 0;
		if (tType == TargetType.Player) {
			characters [idx].soulData.abilitys ["Hp"] += data.effect [1];
			if (characters [idx].soulData.abilitys ["Hp"] > characters [idx].fullHp) {
				over = characters [idx].soulData.abilitys ["Hp"] - characters [idx].fullHp;
				characters [idx].soulData.abilitys ["Hp"] = characters [idx].fullHp;
			}
			fightUIController.OnRecovery (idx, tType, (float)characters [idx].soulData.abilitys ["Hp"] / (float)characters [idx].fullHp);

			if (over > 0) {
				skillController.OverRecovery (idx, orgIdx, over, tType);
			}
			ChangeAccordingData (idx, characters [idx].soulData.abilitys ["Hp"], tType, AccChangeType.Hp);
		} 
		else {
			enemys [idx].soulData.abilitys ["Hp"] += data.effect [1];
			if (enemys [idx].soulData.abilitys ["Hp"] > enemys [idx].fullHp) {
				over = enemys [idx].soulData.abilitys ["Hp"] - enemys [idx].fullHp;
				enemys [idx].soulData.abilitys ["Hp"] = enemys [idx].fullHp;
			}

			fightUIController.OnRecovery (idx, tType, (float)enemys [idx].soulData.abilitys ["Hp"] / (float)enemys [idx].fullHp);

			if (over > 0) {
				skillController.OverRecovery (idx, orgIdx, over, tType);
			}
			ChangeAccordingData (idx, enemys [idx].soulData.abilitys ["Hp"], tType, AccChangeType.Hp);
		}
	}


	#endregion

	private void OnAbilityChanged(int idx, Dictionary<string, int> ability, TargetType tType){
		if (tType == TargetType.Player) {
			characters[idx].abiChange = OnAbilityChanged (characters[idx].abiChange, ability);
			for(int i=0;i<enemys.Length;i++){
				if (characters [idx].abiChange.ContainsKey ("Atk") || characters [idx].abiChange.ContainsKey ("mAtk")) {
					int atk = characters [idx].soulData.abilitys ["Atk"];
					int mAtk = characters [idx].soulData.abilitys ["mAtk"];
					if (characters [idx].abiChange.ContainsKey ("Atk")) {
						atk = atk * characters [idx].abiChange ["Atk"]/100;
					}
					if (characters [idx].abiChange.ContainsKey ("mAtk")) {
						mAtk = mAtk * characters [idx].abiChange ["mAtk"]/100;
					}
					ChangeAccordingData (i, atk + mAtk, TargetType.Player, AccChangeType.MAtkAtk);
				}

			}
		} else {
			enemys[idx].abiChange = OnAbilityChanged (enemys[idx].abiChange, ability);
			for(int i=0;i<characters.Length;i++){
				if (enemys [idx].abiChange.ContainsKey ("Atk") || enemys [idx].abiChange.ContainsKey ("mAtk")) {
					int atk = enemys [idx].soulData.abilitys ["Atk"];
					int mAtk = enemys [idx].soulData.abilitys ["mAtk"];
					if (enemys [idx].abiChange.ContainsKey ("Atk")) {
						atk = atk * enemys [idx].abiChange ["Atk"]/100;
					}
					if (enemys [idx].abiChange.ContainsKey ("mAtk")) {
						mAtk = mAtk * enemys [idx].abiChange ["mAtk"]/100;
					}
					ChangeAccordingData (i, atk + mAtk, TargetType.Player, AccChangeType.MAtkAtk);
				}

			}
		}
	}

	private Dictionary<string, int> OnAbilityChanged(Dictionary<string, int> abiChanged, Dictionary<string, int> ability)
	{
		foreach (KeyValuePair<string, int> kv in ability) { 
			if (abiChanged.ContainsKey (kv.Key)) {
				abiChanged [kv.Key] += kv.Value;
			} else {
				abiChanged.Add (kv.Key, 100 + kv.Value);
			}
		}

		return abiChanged;
	}


	public void ShowSoulData(){
		/*if (character.abiChange.ContainsKey (3)) {
			foreach (KeyValuePair<string, int> kv in character.abiChange[3]) {
				Debug.LogWarning (kv.Key + " : " + kv.Value);
			}
		} else {
			Debug.LogWarning ("Null");
		}*/
		/*foreach (KeyValuePair<string, int> kv in characters [3].soulData.abilitys) {
			Debug.LogWarning (kv.Key + " : " + kv.Value);
		}*/
		/*foreach (AccordingData acD in enemys[3].according) {
			Debug.LogWarning (acD.index);
		}*/
		Debug.LogWarning (characters [3].soulData.abilitys ["Atk"] + " : " + characters [3].soulData.abilitys ["mAtk"] + " : " + enemys [3].according [3].index + " : " + enemys [3].according [3].mAtkAtk);
	}

	public void ShowSkillData(){
		skillController.ShowRuleData ();
	}


}

public enum TargetType{
	Player,
	Enemy,
	Both
}

public struct DamageData{
	public int orgIdx;
	public int targetIdx;
	public int damage;
	public float hpRatio;
	public DamageType damageType;
	public int attributes;
	public bool isCrt;
	public TargetType tType;
}

public enum FightStatus{
	RoundPrepare,
	RoundStart,
	FightStart,
	FightEnd,
	SkillStart,
	SkillEnd,
	SelSkillTarget,
}

public enum DamageType{
	Physical,
	Magic
}

public struct ChessData{
	public SoulLargeData soulData;
	public AccordingData[] according;
	public int fullHp;
	public List<int> status;
	public Dictionary<string,int> abiChange;
	public int initCD;
	public bool exclube;
}

public struct AccordingData{
	public int index;
	public float attriRatio;
	public float jobRatio;
	public int mAtkAtk;
	public int hp;
	public int minus;
	public int crt;
}