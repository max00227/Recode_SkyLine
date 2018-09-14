using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using model.data;

public class FightController : MonoBehaviour {
	
		
	public CharaLargeData[] characters;

	public MonsterLargeData[] monsters;

	private int[] monsterCdTimes = new int[5];

	private int[] jobRatio;

	LinkedList<int> lockOrderIdx;

	private Dictionary<int,AccordingData[]> fightPairs;

	public delegate void FightComplete ();

	public FightComplete onComplete;

	private int[] cdTime;

	private int[] protectJob;

	private int[] charaActLevel;

	private int[] charaFullHp;
	private int[] monsterFullHp;


	public delegate void OnShowFight(int orgIdx ,DamageData damageData, AtkType aType);

	public OnShowFight onShowFight;

	public Dictionary<int, List<DamageData>> damages;

	public void SetData(){
		characters = new CharaLargeData[5];
		monsters = new MonsterLargeData[5];
		charaFullHp  = monsterFullHp = new int[5];

		string enemyDataPath = "/ClientData/EnemyData.txt";

		System.IO.StreamReader sr = new System.IO.StreamReader (Application.dataPath + enemyDataPath);
		string json = sr.ReadToEnd();

		EnemyLargeData enemyData = JsonConversionExtensions.ConvertJson<EnemyLargeData>(json);

		for (int i = 0;i<enemyData.TeamData[0].Team.Count;i++) {
			monsters[i] = MasterDataManager.GetMonsterData (enemyData.TeamData[0].Team[i].id);
			monsters [i].Merge (ParameterConvert.GetMonsterAbility (monsters [i], enemyData.TeamData[0].Team[i].lv));
			monsterFullHp [i] = monsters [i].hp;
		}


		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters[i] = MasterDataManager.GetCharaData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].Merge (ParameterConvert.GetCharaAbility (characters [i], MyUserData.GetTeamData (0).Team [i].lv));
			charaFullHp [i] = characters [i].hp;
		}

		for (int i = 0; i < monsterCdTimes.Length; i++) {
			monsterCdTimes [i] = 5;
		}

		UnLockOrder ();
	}

	public void SetProtect(int[] protects){
		protectJob = protects;
	}

	private void OnFight(AtkType type){
		damages = new Dictionary<int, List<DamageData>> ();
		int count = type == AtkType.pve ? characters.Length : monsters.Length;
		for (int i = 0; i < count; i++) {
			int orgHp = type == AtkType.pve ? characters[i].hp : monsters[i].hp;
			if (orgHp > 0) {
				if (fightPairs.ContainsKey (i)) {
					AccordingData[] order;
					fightPairs.TryGetValue (i, out order);
					int orgJob = type == AtkType.pve ? characters[i].job : monsters[i].job;
					Dictionary<int,int> finalDamage = new Dictionary<int, int> ();

					//判斷是否全體攻擊
					bool isAll = false;
					if (orgJob > 4) {
						if (type == AtkType.evp) {
							isAll = true;
						}
						else {
							if (charaActLevel [i] > 2) {
								isAll = true;
							}
						}
					}
					for (int j = 0;j<order.Length;j++) {
						if (order[i].hp > 0) {
							int damage;
							float hpRatio;
							if (type == AtkType.pve) {
								damage = CalDamage (characters [i].atk, monsters [order[j].index].def, jobRatio [orgJob], order[j].attriJob, order[j].minus, isAll);
								monsters [order[j].index].hp -= damage;
								hpRatio = monsters [order [j].index].hp / monsterFullHp [order [j].index];
							} 
							else {
								damage = CalDamage (monsters [i].atk, characters [order[j].index].def, 1, order[j].attriJob, order[j].minus, isAll);
								characters [order[i].index].hp -= damage;
								hpRatio = characters [order [j].index].hp / charaFullHp [order [j].index];
							}
							order [j].hp -= damage;


							if (damages.ContainsKey (i)) {
								damages [i].Add (GetDamageData (order [i].index, order [j].hp, hpRatio));
							} 
							else{
								List<DamageData> data = new List<DamageData>();
								data.Add (GetDamageData (order [i].index, order [j].hp, hpRatio));
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
	}

	private DamageData GetDamageData(int targer, int fHp, float hpRatio){
		DamageData data = new DamageData ();
		data.targetIdx = targer;
		data.finalHp = fHp;
		data.hpRatio = hpRatio;

		return data;
	}

	public int CalDamage(int atk, int def, int ratio, float ratioAJ, int minus, bool isAll){
		int randomRatio = isAll != true ? UnityEngine.Random.Range (75, 101) : UnityEngine.Random.Range (40, 75);

		int damage = Mathf.CeilToInt((atk * (randomRatio / 100) * ratio * ratioAJ - def) * minus / 100);

		return damage;
	}

	IEnumerator ShowFight(AtkType atype, bool Callback){
		int count = 0;
		while (count < damages.Count) {

			foreach (DamageData data in damages[damages.ElementAt(count).Key]) {
				onShowFight.Invoke (damages.ElementAt (count).Key, data, atype);
			}
			count++;

			yield return new WaitForSeconds(0.5f);
		}
		onShowFight = null;

		if (Callback) {
			onComplete.Invoke ();
		}
	} 

	public void FightStart(bool lockEnemy, List<int> canAttack, int[] ratios, int[] actLevel){
		jobRatio = ratios;
		charaActLevel = actLevel;
		bool enemyFight = cdTime.Any (t => t == 0);

		if (canAttack.Count > 0) {
			FightStart (canAttack.ToArray (), AtkType.pve, actLevel);
			OnFight (AtkType.pve);
			StartCoroutine (ShowFight (AtkType.pve, !enemyFight));
		}
		if (enemyFight) {
			FightStart (cdTime, AtkType.evp);
			OnFight (AtkType.evp);
			StartCoroutine (ShowFight (AtkType.evp, enemyFight));
		}
	}

	private void FightStart(int[] attackIdx, AtkType aType, int[] actLevel = null){
		fightPairs = new Dictionary<int, AccordingData[]> ();
		if (aType == AtkType.pve) {
			foreach (int idx in attackIdx) {
				fightPairs.Add (idx, matchAtkTarget (characters [idx].job, aType));
			}
		} 
		else {
			for (int i = 0;i< attackIdx.Length;i++) {
				if (attackIdx [i] == 0) {
					fightPairs.Add (i, matchAtkTarget (monsters [i].job, aType));					
				}
			}
		}
	}

	private AccordingData[] matchAtkTarget(int jobIdx, AtkType aType){
		AccordingData[] atkOrder = new AccordingData[5];
		List<int> unLockIdx = new List<int> ();
		for (int i = 0; i < 5; i++) {
			if (!lockOrderIdx.Contains (i)) {
				unLockIdx.Add (i);
			}	
		}
		AccordingData[] compareOrder = CompareData (jobIdx, unLockIdx, aType);

		if (unLockIdx.Count == 5) {
			atkOrder = compareOrder;
		}
		else{
			for (int i = 0; i < atkOrder.Length; i++) {
				atkOrder [i] = GetAccordingData (jobIdx, lockOrderIdx.ElementAt (i), aType);
				if (i >= lockOrderIdx.Count) {
					atkOrder [i] = compareOrder [i - lockOrderIdx.Count];
				}
			}
		}

		return atkOrder;
	}

	private AccordingData GetAccordingData(int orgIdx, int idx, AtkType aType){
		AccordingData data = new AccordingData ();
		data.index = idx;
		if (aType == AtkType.pve) {
			data.attriJob = GetCalcRatio (characters [orgIdx].job, monsters [idx].job, characters [orgIdx].attributes, monsters [idx].attributes);
			data.mAtkAtk = new int[3] { monsters [idx].mAtk, monsters [idx].atk, monsters [idx].mAtk + monsters [idx].atk };
			data.hp = monsters [idx].hp;
			data.minus = cdTime [idx] == 0 ? 0 : 50;
			data.crt = monsters [idx].crt;
		} else {
			data.attriJob = GetCalcRatio (monsters [orgIdx].job, characters [idx].job, monsters [orgIdx].attributes, characters [idx].attributes);
			data.mAtkAtk = new int[3] { characters [idx].mAtk, characters [idx].atk, characters [idx].mAtk + characters [idx].atk };
			data.hp = characters [idx].hp;
			data.minus = protectJob [characters [idx].job] / 2;
			data.crt = characters [idx].crt;
		}

		return data;
	}

	private AccordingData[] CompareData(int orgIdx, List<int> unLockIdx, AtkType aType){
		List<AccordingData> according = new List<AccordingData> ();
		foreach (int idx in unLockIdx) {
			AccordingData data = GetAccordingData (orgIdx, idx, aType);
			according.Add (data);
		}

		return AccordingCompare (according.ToArray(), true);
	}

	private float GetCalcRatio(int aj, int bj, int aa, int ba){
		return ParameterConvert.AttriRatioCal (aa, ba)*ParameterConvert.JobRatioCal (aj, bj);
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
			return lockOrderIdx;
		} 
		else {	
			lockOrderIdx.AddLast (idx);
			return lockOrderIdx;
		}
	}

	public LinkedList<int> UnLockOrder(){
		lockOrderIdx = new LinkedList<int> ();
		return lockOrderIdx;
	}

	public void SetCDTime(int[] cd){
		foreach(int i in cd){
			Debug.Log (i);
		}
		cdTime = cd;
	}

	struct AccordingData{
		public int index;
		public float attriJob;
		public int[] mAtkAtk;
		public int hp;
		public int minus;
		public int crt;
	}
}

public enum AtkType{
	pve,
	evp
}

public struct DamageData{
	public int targetIdx;
	public int finalHp;
	public float hpRatio;
}