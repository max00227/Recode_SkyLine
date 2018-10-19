using System.Collections;
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

	int resetRatio;

	bool isLock = false;


	public delegate void OnShowFight(int orgIdx , int targetIdx ,DamageData damageData, TargetType tType);
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

	public Dictionary<int, List<DamageData>> damages;

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
			monsters [i].MergeSkill (null, monsters [i].NorSkill);
			monsterFullHp [i] = monsters [i].hp;

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
			characters [i].MergeSkill (characters [i].ActSkill, characters [i].NorSkill); 

			charaFullHp [i] = characters [i].hp;

			charaBuffStatus [i] = new List<int> ();

			skillCdTime [i] = (int)characters [i]._NorSkill.CD;
			skillInitCD [i] = (int)characters [i]._NorSkill.CD;
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

	private void OnFight(TargetType type){
		damageShowSort = new Dictionary<int, Dictionary<int, List<DamageData>>> ();
		int count = type == TargetType.Player ? monsters.Length : characters.Length;
		for (int i = 0; i < count; i++) {
			SoulLargeData orgData = type == TargetType.Player ? monsters[i] : characters[i];
			if (orgData.hp > 0) {
				if (fightPairs.ContainsKey (i)) {
					AccordingData[] order;
					fightPairs.TryGetValue (i, out order);

					//判斷是否全體攻擊
					bool isAll = false;
					if (orgData.job >= 3) {
						if (type == TargetType.Enemy) {
							if (jobActLevel [i] >= 2) {
								isAll = true;
							}
						}
						else {
							isAll = true;
						}
					}

					for (int j = 0; j < order.Length; j++) {
						SoulLargeData targetData = type == TargetType.Player ? characters [j] : monsters [j];
						if (targetData.hp > 0) {
							int damage;
							if (orgData.job <= 3) {
								damage = GetDamage (orgData, targetData, order [j].index, order [j].attriJob, order [j].minus, type, DamageType.Physical, isAll);
								OnDamage (orgData, targetData, i, order [j].index, damage, type, DamageType.Physical);
							}

							if (orgData.job >= 3) {
								damage = GetDamage (orgData, targetData, order [j].index, order [j].attriJob, order [j].minus, type, DamageType.Magic, isAll);
								OnDamage (orgData, targetData, i, order [j].index, damage, type, DamageType.Physical);
							}
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

	private int GetDamage (SoulLargeData orgData, SoulLargeData targetData, int targetIdx,float attriJob, int minus, TargetType tType, DamageType dType, bool isAll){
		int damage;
		if (dType == DamageType.Physical) {
			damage = CalDamage (orgData.atk, targetData.def, 100, attriJob, minus, 0, isAll);
		} 
		else {
			damage = CalDamage (orgData.mAtk, targetData.mDef, 100, attriJob, minus, 0, isAll);
		}
			
		if (tType == TargetType.Player) {
			return damage;
		} 
		else {
			return damage * (10 - monsterProtect) / 10;
		}
	}

	private void OnDamage (SoulLargeData orgData, SoulLargeData targetData, int orgIdx, int targetIdx, int damage, TargetType tType, DamageType dType){
		if (damageShowSort.ContainsKey (orgIdx)) {
			if (damageShowSort [orgIdx].ContainsKey (targetIdx)) {

				damageShowSort [orgIdx] [targetIdx].Add (GetDamageData (orgData, targetData, targetIdx, damage, tType, dType));
			} else {
				List<DamageData> data = new List<DamageData> ();
				data.Add (GetDamageData (orgData, targetData, targetIdx, damage, tType, dType));
				damageShowSort [orgIdx].Add (targetIdx, data);
			}
		} 
		else {
			damageShowSort.Add (orgIdx, new Dictionary<int, List<DamageData>> ());

			List<DamageData> data = new List<DamageData> ();
			data.Add (GetDamageData (orgData, targetData, targetIdx, damage, tType, dType));
			damageShowSort [orgIdx].Add (targetIdx, data);
		}
	}

	private DamageData GetDamageData(SoulLargeData orgData, SoulLargeData targetData, int targetIdx, int damage, TargetType tType, DamageType dType){
		bool isDead = false;
		targetData.hp -= damage;

		if (targetData.hp <= 0) {
			targetData.hp = 0;
			isDead = true;
			if (targetData.job == 2) {
				if (tType == TargetType.Player) {
					charaProtect--;
				}
				else{
					monsterProtect--;
				}
			}
		}


		if (isDead) {
			OnDeath (targetIdx, tType);
		}

		if (tType == TargetType.Player) {
			ChangeAccordingHp (targetIdx, targetData.hp, tType);
			return GetDamageData (damage, (float)targetData.hp / (float)charaFullHp [targetIdx], dType);
		} 
		else {
			ChangeAccordingHp (targetIdx, targetData.hp, tType);
			return GetDamageData (damage, (float)targetData.hp / (float)monsterFullHp [targetIdx], dType);
		}

	}

	private DamageData GetDamageData(int damage, float hpRatio, DamageType dType){
		DamageData data = new DamageData ();
		data.damage = damage;
		data.hpRatio = hpRatio;
		data.damageType = dType;

		return data;
	}

	private void OnDeath(int idx, TargetType tType){
		onDead.Invoke (idx, tType);
	}

	public int CalDamage(int atk, int def, int ratio, float ratioAJ, int minus,int actLevel, bool isAll){
		int actRatio;
		if (actLevel != 0) {
			actRatio = 50 * (int)Mathf.Pow (2, actLevel - 1);
		} else {
			actRatio = 0;
		}

		float randomRatio = isAll != true ? UnityEngine.Random.Range (75, 101) : UnityEngine.Random.Range (40, 75);

		int damage = Mathf.CeilToInt ((atk * (randomRatio / 100) * (ratio + actRatio) / 100 * ratioAJ * (int)Mathf.Pow (1.5f, resetRatio) - def) * (100 - minus) / 100);
		//((Atk * randamRatio * (ratio + actRatio) * ratioAJ * resetCount) - def) * minus

		return damage <= 0 ? 1 : damage;
	}

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

	void ShowFight(int orgIdx, Dictionary<int, List<DamageData>> damageData, TargetType tType){
		foreach (KeyValuePair<int, List<DamageData>> data in damageData) {
			StartCoroutine(ShowFight (orgIdx, data.Key, data.Value, tType));
		}
	}



	IEnumerator ShowFight(int orgIdx, int targetIdx, List<DamageData> damageData, TargetType tType){
		int count = 0;
		foreach (DamageData data in damageData) {
			if (onShowFight != null) {
				onShowFight.Invoke (orgIdx, targetIdx, data, tType);
			}
			count++;
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void FightStart(bool lockEnemy, List<int> canAttack, int[] ratios, int[] actLevel){
		jobRatio = ratios;
		jobActLevel = actLevel;
		bool enemyFight = DataUtil.CheckArray<int> (cdTime, 0);

		if (canAttack.Count > 0) {
			FightStart (canAttack.ToArray (), TargetType.Enemy, actLevel);
			OnFight (TargetType.Enemy);
			StartCoroutine (ShowFight (TargetType.Enemy, !enemyFight));
		}
	}

	public void EnemyFight(){
		FightStart (cdTime, TargetType.Player);
		OnFight (TargetType.Player);
		StartCoroutine (ShowFight (TargetType.Player, true));
	}

	private void FightStart(int[] attackIdx, TargetType tType, int[] actLevel = null){
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


	private AccordingData[] CompareData(int orgIdx, TargetType tType){
		AccordingData[] according = tType == TargetType.Player ? monsterAccording[orgIdx] : charaAccording [orgIdx];

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

	private void GetAccordingDataDic(){
		charaAccording = new Dictionary<int, AccordingData[]> ();
		monsterAccording = new Dictionary<int, AccordingData[]> ();
		for (int i = 0; i < characters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<monsters.Length; j++){
				data [j] = GetAccordingDataDic (i, j, TargetType.Player);
			}
			charaAccording.Add (i, data);
		}

		for (int i = 0; i < monsters.Length; i++) {
			AccordingData[] data = new AccordingData[5];
			for(int j = 0;j<characters.Length; j++){
				data [j] = GetAccordingDataDic (i, j, TargetType.Enemy);
			}
			monsterAccording.Add (i, data);
		}
	}

	private AccordingData GetAccordingDataDic(int orgIdx, int targetIdx, TargetType tType){
		if (tType == TargetType.Player) {
			return GetAccording (characters [orgIdx], monsters [targetIdx], targetIdx, tType);
		} else {
			return GetAccording (monsters [orgIdx], characters [targetIdx], targetIdx, tType);
		}
	}

	private AccordingData GetAccording(SoulLargeData orgData, SoulLargeData targetData, int targetIdx,TargetType tType){
		AccordingData data = new AccordingData ();
		data.index = targetIdx;
		data.attriJob = GetCalcRatio (orgData.job, targetData.job, orgData.attributes, targetData.attributes);
		data.mAtkAtk = new int[3] { targetData.mAtk, targetData.atk, orgData.mAtk + orgData.atk };
		data.hp = targetData.hp;
		data.crt = targetData.crt;
		if (tType == TargetType.Player) {
			data.minus = cdTime [targetIdx] == 0 ? 0 : 50;
		} 
		else {
			data.minus = protectJob [targetData.job];
		}

		return data;
	}

	private AccordingData GetAccordingData(int orgIdx, int idx, TargetType tType){
		if (tType == TargetType.Player) {
			return charaAccording [orgIdx] [idx];
		}
		else {
			return monsterAccording [orgIdx] [idx];
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
				characters [idx].hp += recovery;
				if (characters [idx].hp > charaFullHp [idx]) {
					skillController.OverRecovery (idx, orgIdx, characters [idx].hp - charaFullHp [idx], tType);
					characters [idx].hp = charaFullHp [idx];
				}
			} else {
				monsters [idx].hp += recovery;
				if (monsters [idx].hp > monsterFullHp [idx]) {
					skillController.OverRecovery (idx, orgIdx, monsters [idx].hp - monsterFullHp [idx], tType);
					monsters [idx].hp = monsterFullHp [idx];
				}
			}
		}
	}

	public bool OnRuleMeets(int idx ,int ruleId, int param, TargetType tType){
		if (tType == TargetType.Player) {
			OnCharacterRule (idx, ruleId, param);
		} 
		else {
			OnMonsterRule (idx, ruleId, param);
		}
		return false;
	}

	public bool OnCharacterRule(int idx ,int ruleId, int param){
		if (jobActLevel [characters [idx].job] > 0) {
			switch (ruleId) {
			case 1:
				return (characters [idx].hp / charaFullHp [idx] * 100) < param;
			case 2:
				return (characters [idx].hp / charaFullHp [idx] * 100) >= param;
			}
		}
		return false;
	}

	public bool OnMonsterRule(int idx ,int ruleId, int param){
		switch (ruleId) {
		case 1:
			return (monsters [idx].hp / monsterFullHp [idx] * 100) < param;
		case 2:
			return (monsters [idx].hp / monsterFullHp [idx] * 100) >= param;
		}
		return false;
	}

	public void AddExclude(int idx, TargetType tType){
		//if(tType)

	}

	public void ShowSoulData(){
		for (int i = 0; i < 5; i++) {
			Debug.Log (characters [i].hp + " : " + charaFullHp [i]);
			Debug.Log (monsters [i].hp + " : " + monsterFullHp [i]);
		}
	}
}

public enum TargetType{
	Player,
	Enemy,
	Both
}

public struct DamageData{
	public int damage;
	public float hpRatio;
	public DamageType damageType;
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

