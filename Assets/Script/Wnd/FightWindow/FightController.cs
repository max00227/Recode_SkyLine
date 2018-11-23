﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using model.data;

public class FightController : MonoBehaviour {
	[SerializeField]
	SkillController skillController;

	[HideInInspector]
	public SoulLargeData[] characters;
	[HideInInspector]
	public SoulLargeData[] monsters;

	private int[] monsterCdTimes = new int[5];

	private int[] jobRatio;

	LinkedList<int> lockOrderIdx;

	private Dictionary<int,AccordingData[]> fightPairs;

	public delegate void FightComplete ();
	public FightComplete onComplete;

	public delegate void SelectComplete();
	public SelectComplete onSelectComplete;

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
	private List<int>[] charaNerfStatus;

	private List<int>[] monsterBuffStatus;
	private List<int>[] monsterNerfStatus;

	private List<int> excludeChara = new List<int> ();
	private List<int> excludeMonster = new List<int> ();

	float resetRatio;

	bool isLock = false;


	public delegate void OnShowFight(List<DamageData> damageData);
	public OnShowFight onShowFight;

	public delegate void OnProtect(bool hasProtect);
	public OnProtect onProtect;

	public delegate void OnCallBackIdx(int idx);
	public OnCallBackIdx onSkillCDEnd;
	public OnCallBackIdx unLockOrder;

	public delegate void OnDead(int idx, TargetType tType);
	public OnDead onDead;

	public delegate void OnLockOrder(LinkedList<int> order);
	public OnLockOrder onLockOrder;

	public Dictionary<int, Dictionary<int, List<DamageData>>> damageShowSort;


	public delegate void OnCloseButton(List<int> idx, TargetType tType);
	public OnCloseButton onCloseButton;
	public Dictionary<int, List<RuleLargeData>> charaTriggers;
	public Dictionary<int, List<RuleLargeData>> monsterTriggers;



	public FightStatus fightStatus;


	void Update(){
		if (Input.GetKeyDown (KeyCode.P)) {
			for (int i = 0; i < 5; i++) {
				Debug.Log (monsterAccording [0] [i].minus);
			}
		}
	}


	public void SetData(){
		resetRatio = 1;
		SetCharaData ();
		SetMonsterData ();

		skillController.SetData (characters, monsters);

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

		monsters = new SoulLargeData[enemyData.TeamData[0].Team.Count];
		monsterFullHp = new int[enemyData.TeamData[0].Team.Count];
		monsterBuffStatus = new List<int>[MyUserData.GetTeamData(0).Team.Count];

		for (int i = 0;i<enemyData.TeamData[0].Team.Count;i++) {
			monsters[i] = MasterDataManager.GetSoulData (enemyData.TeamData[0].Team[i].id);
			monsters [i].Merge (ParameterConvert.GetMonsterAbility (monsters [i], enemyData.TeamData[0].Team[i].lv));
			monsters [i].Merge (monsters [i].actSkill, monsters [i].norSkill);
			monsterFullHp [i] = monsters [i].abilitys["Hp"];

			monsterBuffStatus [i] = new List<int> ();
			if (monsters [i].job == 2) {
				monsterProtect++;
			}
		}
	}

	private void SetCharaData(){
		characters = new SoulLargeData[MyUserData.GetTeamData(0).Team.Count];
		charaFullHp = new int[MyUserData.GetTeamData(0).Team.Count];
		skillCdTime = new int[MyUserData.GetTeamData(0).Team.Count];
		skillInitCD = new int[MyUserData.GetTeamData(0).Team.Count];
		charaBuffStatus = new List<int>[MyUserData.GetTeamData(0).Team.Count];
		protectJob = new int[5]{ 0, 0, 0, 0, 0 };




		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters[i] = MasterDataManager.GetSoulData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].Merge (ParameterConvert.GetCharaAbility (characters [i], MyUserData.GetTeamData (0).Team [i].lv));
			characters [i].Merge (characters [i].actSkill, characters [i].norSkill);
			charaFullHp [i] = characters [i].abilitys["Hp"];

			charaBuffStatus [i] = new List<int> ();

			skillCdTime [i] = (int)characters[i]._norSkill.cdTime;
			skillInitCD [i] = (int)characters[i]._norSkill.cdTime;
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

	/// <summary>
	/// 計算傷害值
	/// </summary>
	/// <param name="type">目標類型</param>
	private void OnFight(TargetType tType){
		damageShowSort = new Dictionary<int, Dictionary<int, List<DamageData>>> ();
		int count = tType == TargetType.Player ? monsters.Length : characters.Length;
		for (int i = 0; i < count; i++) {
			SoulLargeData orgData = tType == TargetType.Player ? monsters[i] : characters[i];
			if (orgData.abilitys["Hp"] > 0) {
				if (fightPairs.ContainsKey (i)) {
					AccordingData[] order;
					fightPairs.TryGetValue (i, out order);

					//判斷是否全體攻擊
					bool isAll = false;
					if (orgData.job >= 3) {
						if (tType == TargetType.Enemy) {
							if (jobActLevel [i] >= 2) {
								isAll = true;
							}
						}
						else {
							isAll = true;
						}
					}

					for (int j = 0; j < order.Length; j++) {
						SoulLargeData targetData = tType == TargetType.Player ? characters [order[j].index] : monsters [order[j].index];
						if (targetData.abilitys["Hp"] > 0) {
							List<DamageData> allDamage = new List<DamageData> ();
							if (orgData.job <= 3) {
								allDamage.Add (GetDamage (orgData, targetData, i, order [j].index, order [j].attriJob, order [j].minus, tType, DamageType.Physical, isAll));
							}

							if (orgData.job >= 3) {
								allDamage.Add (GetDamage (orgData, targetData, i, order [j].index, order [j].attriJob, order [j].minus, tType, DamageType.Magic, isAll));
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
	/// </summary>
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
		int actRatio = tType == TargetType.Player ? 0 : jobActLevel [orgData.job];
		int ratio = tType == TargetType.Player ? 0 : jobRatio [orgData.job];
		if (dType == DamageType.Physical) {
			damageData = CalDamage (orgData.abilitys["Atk"], targetData.abilitys["Def"], ratio, attriJob, minus, actRatio, orgData.abilitys["Cri"], isAll);
			damageData.damageType = DamageType.Physical;
		} 
		else {
			damageData = CalDamage (orgData.abilitys["mAtk"], targetData.abilitys["mDef"], ratio, attriJob, minus, actRatio, orgData.abilitys["Cri"], isAll);
			damageData.damageType = DamageType.Magic;
		}

		damageData.tType = tType;
		damageData.attributes = orgData.act [jobActLevel [orgData.job]];
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
					monsterProtect--;
				}
			}
		}

		if (isDead) {
			OnDeath (damageData.targetIdx, damageData.tType);
		}

		if (damageData.tType == TargetType.Player) {
			ChangeAccordingHp (damageData.targetIdx, targetData.abilitys["Hp"], damageData.tType);
			data.hpRatio = (float)targetData.abilitys["Hp"] / (float)charaFullHp [damageData.targetIdx];
			return data;
		} 
		else {
			ChangeAccordingHp (damageData.targetIdx, targetData.abilitys["Hp"], damageData.tType);
			data.hpRatio = (float)targetData.abilitys["Hp"] / (float)monsterFullHp [damageData.targetIdx];
			return data;
		}

	}

	private void OnDeath(int idx, TargetType tType){
		onDead.Invoke (idx, tType);
	}

	/// <summary>
	/// 計算傷害值
	/// </summary>
	public DamageData CalDamage(int atk, int def, int ratio, float ratioAJ, int minus,int actLevel, int crt, bool isAll){
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
	/// </summary>
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
			onShowFight = null;
			if (onComplete != null) {
				onComplete.Invoke ();
			}
		}
		else {
			EnemyFight ();
		}
	} 

	/// <summary>
	/// 回調UI執行傷害動畫
	/// </summary>
	/// <param name="orgIdx">Org index.</param>
	/// <param name="damageData">Damage data.</param>
	/// <param name="tType">T type.</param>
	void ShowFight(int orgIdx, Dictionary<int, List<DamageData>> damageData, TargetType tType){
		foreach (KeyValuePair<int, List<DamageData>> data in damageData) {
			onShowFight.Invoke (data.Value);
		}
	}
		
	/// <summary>
	/// 開始進行戰鬥，先決定可攻擊者的攻擊目標順序，然後計算傷害值，最後執行戰鬥動畫
	/// </summary>
	/// <param name="lockEnemy">玩家是否有鎖定敵人</param>
	/// <param name="canAttack">玩家角色是否有可能攻擊者</param>
	/// <param name="ratios">職業攻擊力加成</param>
	/// <param name="actLevel">攻擊者攻擊階級</param>
	public void FightStart(bool lockEnemy, List<int> canAttack, int[] ratios, int[] actLevel){
		jobRatio = ratios;
		jobActLevel = actLevel;
		bool enemyFight = DataUtil.CheckArray<int> (cdTime, 0);

		if (canAttack.Count > 0) {
			FightPairs (canAttack.ToArray (), TargetType.Enemy, actLevel);
			OnFight (TargetType.Enemy);
			StartCoroutine (ShowFight (TargetType.Enemy, !enemyFight));
		}
	}

	/// <summary>
	/// 敵人攻擊
	/// </summary>
	public void EnemyFight(){
		FightPairs (cdTime, TargetType.Player);
		OnFight (TargetType.Player);
		StartCoroutine (ShowFight (TargetType.Player, true));
	}

	/// <summary>
	/// 攻擊目標配對
	/// </summary>
	/// <param name="attackIdx">可攻擊清單</param>
	/// <param name="tType">目標陣營.</param>
	/// <param name="actLevel">攻擊者攻擊階級<param>
	private void FightPairs(int[] attackIdx, TargetType tType, int[] actLevel = null){
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
	/// </summary>
	/// <returns>The target.</returns>
	/// <param name="idx">攻擊者索引值</param>
	/// <param name="tType">被攻擊者陣營</param>
	private AccordingData[] matchTarget(int idx, TargetType tType){

		AccordingData[] compareOrder = CompareData (idx, tType);

		int orderCount = tType == TargetType.Player ? characters.Length : monsters.Length;

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
	}

	private void CallbackProtect(){
		onProtect.Invoke (charaProtect > 0);
	}

	/// <summary>
	/// 建立比對值進行排序
	/// </summary>
	/// <returns>The data.</returns>
	/// <param name="orgIdx">攻擊者索引值</param>
	/// <param name="tType">被攻擊者陣營</param>
	private AccordingData[] CompareData(int orgIdx, TargetType tType){
        AccordingData[] according = new AccordingData[0];

        according = tType == TargetType.Player ? (AccordingData[])monsterAccording[orgIdx].Clone() : (AccordingData[])charaAccording [orgIdx].Clone();

		return AccordingCompare (according, tType);
	}

	/// <summary>
	/// 進行目標排序，目標為敵人時因為玩家可做簡單排序，因此只對克制加成做排序，若目標為玩家會以克制加成>>當下血量>>爆擊值>>減傷>>攻擊傷害順序做排列
	/// </summary>
	/// <returns>The compare.</returns>
	/// <param name="according">根據值</param>
	/// <param name="isPlayer">被攻擊者陣營</param>
	private AccordingData[] AccordingCompare(AccordingData[] according, TargetType tType){
		if (tType == TargetType.Enemy) {
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
		
	/// <summary>
	/// 鎖定排序
	/// </summary>
	/// <param name="idx">選取敵人索引值</param>
	/// <param name="isDead">是否死亡</param>
	public void LockOrder (int idx, bool isDead = false){
		if (lockOrderIdx.Contains (idx)) {
			foreach (var v in lockOrderIdx) {
				if (v == idx) {
					lockOrderIdx.Remove (v);
					unLockOrder.Invoke (idx);
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

		onLockOrder.Invoke (lockOrderIdx);
	}

	public void UnLockOrder(){
		lockOrderIdx = new LinkedList<int> ();
		isLock = false;
		onLockOrder.Invoke (lockOrderIdx);
	}

	public void SetCDTime(int[] cd, bool isInit = true){
		cdTime = cd;
		if (isInit) {
			for (int i = 0; i < charaAccording.Count; i++) {
				for (int j = 0; j < charaAccording.ElementAt (i).Value.Length; j++) {
					if (cdTime [j] == 0) {
						charaAccording.ElementAt (i).Value [j].minus = 0 + monsterProtect * 10;
					} else {
						charaAccording.ElementAt (i).Value [j].minus = 50 * (10 - monsterProtect) / 10;
					}
				}
			}
		}
	}

	/// <summary>
	/// 加快該局後期節奏，會因重製版面次數影響倍率
	/// </summary>
	/// <param name="count">Count.</param>
	public void SetResetRatio(int count){
		resetRatio = Mathf.Pow (1.5f, count);
	}

	struct AccordingData{
		public int index;
		public float attriJob;
		public int[] mAtkAtk;
		public int hp;
		public int minus;
		public int crt;
	}

	/// <summary>
	/// 變更根據值
	/// </summary>
	/// <param name="idx">Index.</param>
	/// <param name="hp">Hp.</param>
	/// <param name="tType">T type.</param>
	private void ChangeAccordingHp(int idx, int hp, TargetType tType){
		if (tType == TargetType.Enemy) {
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

	/// <summary>
	/// 建立各角色對相對陣營的攻擊順序根據值清單
	/// </summary>
	private void GetAccordingDataDic(){
		charaAccording = new Dictionary<int, AccordingData[]> ();
		monsterAccording = new Dictionary<int, AccordingData[]> ();
        SoulLargeData[] charaData = new SoulLargeData[0];
        charaData = (SoulLargeData[])characters.Clone();
        SoulLargeData[] monsterData = new SoulLargeData[0];
        monsterData = (SoulLargeData[])monsters.Clone();
        for (int i = 0; i < characters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<monsters.Length; j++){
                data[j] = GetAccording(charaData[i], monsterData[j], j, TargetType.Player);
			}
			charaAccording.Add (i, data);
		}

		for (int i = 0; i < monsters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<characters.Length; j++){
                data[j] = GetAccording(monsterData[i], charaData[j], j, TargetType.Enemy);
			}
			monsterAccording.Add (i, data);
		}
	}

	/// <summary>
	/// 建立攻擊順序根據值
	/// </summary>
	/// <returns>The according.</returns>
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="targetIdx">被攻擊者索引值</param>
	/// <param name="tType">目標陣營，減傷值會因陣營有所不同</param>
	private AccordingData GetAccording(SoulLargeData orgData, SoulLargeData targetData, int targetIdx,TargetType tType){
        
		AccordingData data = new AccordingData ();
		data.index = targetIdx;
		data.attriJob = GetCalcRatio (orgData.job, targetData.job, orgData.attributes, targetData.attributes);
		data.mAtkAtk = new int[3] { targetData.abilitys["mAtk"], targetData.abilitys["Atk"], orgData.abilitys["mAtk"] + orgData.abilitys["Atk"] };
		data.hp = targetData.abilitys["Hp"];
		data.crt = targetData.abilitys["Cri"];
		if (tType == TargetType.Player) {
			data.minus = cdTime [targetIdx] == 0 + monsterProtect * 10 ? 0 : 50 * (10 - monsterProtect) / 10;
		} 
		else {
			data.minus = protectJob [targetData.job];
		}

		return data;
	}

	/// <summary>
	/// 取得根據值資料
	/// </summary>
	/// <returns>The according data.</returns>
	/// <param name="orgIdx">攻擊者索引</param>
	/// <param name="idx">被攻擊者索引</param>
	/// <param name="tType">被攻擊者索引值</param>
	private AccordingData GetAccordingData(int orgIdx, int targetIdx, TargetType tType){
		if (tType == TargetType.Enemy) {
			return charaAccording [orgIdx] [targetIdx];
		}
		else {
			return monsterAccording [orgIdx] [targetIdx];
		}
	}

	public void FightEnd(){
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
		if (onCloseButton != null) {
			onCloseButton.Invoke (ExcludeTarget (idxList, tType), tType);
		}
	}

	public List<int> ExcludeTarget(List<int> idxList, TargetType tType){
		List<int> checkList = idxList;

		for(int i = 0;i<idxList.Count;i++){
			if (tType == TargetType.Player) {
				if (excludeChara.Contains (checkList [i])) {
					checkList.Remove (checkList [i]);
				}
			} else {
				if (excludeMonster.Contains (idxList [i])) {
					checkList.Remove (checkList [i]);
				}
			}
		}

		return checkList;
	}

	public void OnRecovery(int orgIdx, List<int> recoveryList, int recovery, TargetType tType){
		foreach (int idx in recoveryList) {
			if (tType == TargetType.Player) {
				characters [idx].abilitys["Hp"] += recovery;
				if (characters [idx].abilitys["Hp"] > charaFullHp [idx]) {
					skillController.OverRecovery (idx, orgIdx, characters [idx].abilitys["Hp"] - charaFullHp [idx], tType);
					characters [idx].abilitys["Hp"] = charaFullHp [idx];
				}
				ChangeAccordingHp (idx, characters [idx].abilitys["Hp"], tType);
			} else {
				monsters [idx].abilitys["Hp"] += recovery;
				if (monsters [idx].abilitys["Hp"] > monsterFullHp [idx]) {
					skillController.OverRecovery (idx, orgIdx, monsters [idx].abilitys["Hp"] - monsterFullHp [idx], tType);
					monsters [idx].abilitys["Hp"] = monsterFullHp [idx];
				}
				ChangeAccordingHp (idx, monsters [idx].abilitys["Hp"], tType);
			}
		}
	}

	/// <summary>
	/// 技能條件是否符合
	/// </summary>
	/// <param name="idx">Index.</param>
	/// <param name="ruleId">Rule identifier.</param>
	/// <param name="param">Parameter.</param>
	/// <param name="tType">T type.</param>
	public bool OnRuleMeets(int idx ,int ruleId, int param, TargetType tType){
		if (tType == TargetType.Player) {
			return OnCharacterRule (idx, ruleId, param);
		} 
		else {
			return OnMonsterRule (idx, ruleId, param);
		}
	}

	public bool OnCharacterRule(int idx ,int ruleId, int param){
		if (jobActLevel [characters [idx].job] > 0) {
			switch (ruleId) {
			case 1:
				return (characters [idx].abilitys["Hp"] / charaFullHp [idx] * 100) < param;
			case 2:
				return (characters [idx].abilitys["Hp"] / charaFullHp [idx] * 100) >= param;
			}
		}
		return false;
	}

	public bool OnMonsterRule(int idx ,int ruleId, int param){
		switch (ruleId) {
		case 1:
			return (monsters [idx].abilitys["Hp"] / monsterFullHp [idx] * 100) < param;
		case 2:
			return (monsters [idx].abilitys["Hp"] / monsterFullHp [idx] * 100) >= param;
		}
		return false;
	}

	public void AddExclude(int idx, TargetType tType){
		//if(tType)

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
		SoulLargeData orgData;
		SoulLargeData targetData;
		if (allDamage [0].tType == TargetType.Enemy) {
			orgData = characters [allDamage [0].orgIdx];
			targetData = monsters [allDamage [0].targetIdx];

		} 
		else {
			orgData = monsters [allDamage [0].orgIdx];
			targetData = characters [allDamage [0].targetIdx];
		}
		skillController.OnTriggerSkill (orgData, targetData, allDamage);
	}

	public SoulLargeData GetSoulData(TargetType tType,int index){
		if (tType == TargetType.Player) {
			return characters [index];
		} 
		else {
			return monsters [index];
		}
	}

	/// <summary>
	/// 發動效果，技能分為一般跟狀態類效果
	/// </summary>
	/// <param name="idxList">對象清單</param>
	/// <param name="data">技能效果資料</param>
	/// <param name="targetType">目標類型 玩家：敵人</param>
	/// <param name="paramater">效果參數</param>
	public void OnSkillEffect(List<int> idxList, RuleLargeData data, TargetType targetType){
		if (data.effectType == 1) {
			OnStatus (idxList, data);
		} 
		else {
			OnNormal (idxList, data);
		}
	}


	/// <summary>
	/// 一般類型既能函式
	/// </summary>
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.Recovery = 1,Act = 2,Cover = 3,RmAlarm = 4,RmNerf = 5,Dmg = 6,Exchange = 7,Call = 8</param>
	/// <param name="paramater">效果參數.</param>
	private void OnNormal(List<int> targetList, RuleLargeData data){
		switch (data.effectType) {
		case (int)Normal.Recovery:
			break;
		}
	}

	/// <summary>
	/// 狀態附加類型技能函式
	/// </summary>
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.UnDef = 1,UnNerf = 2,AddNerf = 3,Suffer = 4,Maximum = 5,Ability = 6,UnDirect = 7</param>
	/// <param name="paramater">效果參數.</param>
	private void OnStatus(List<int> targetList, RuleLargeData data){

	}





	public void ShowSoulData(){
		for (int i = 0; i < 5; i++) {
			Debug.LogWarning (charaFullHp [i]);
			Debug.LogError (monsterFullHp [i]);
		}
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

