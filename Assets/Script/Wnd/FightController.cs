using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using model.data;

public class FightController : MonoBehaviour {
	enum AtkType{
		pve,
		evp
	}

	AtkType atkType;

	public CharaLargeData[] characters;

	public MonsterLargeData[] monsters;

	private int[] monsterCdTimes = new int[5];

	LinkedList<int> lockOrderIdx;

	private Dictionary<int,int[]> fightPairs;

	public delegate void FightComplete ();

	public FightComplete onComplete;

	public void SetData(){
		characters = new CharaLargeData[5];
		monsters = new MonsterLargeData[5];

		string enemyDataPath = "/ClientData/EnemyData.txt";

		System.IO.StreamReader sr = new System.IO.StreamReader (Application.dataPath + enemyDataPath);
		string json = sr.ReadToEnd();

		EnemyLargeData enemyData = JsonConversionExtensions.ConvertJson<EnemyLargeData>(json);

		for (int i = 0;i<enemyData.TeamData[0].Team.Count;i++) {
			monsters[i] = MasterDataManager.GetMonsterData (enemyData.TeamData[0].Team[i].id);
			monsters [i].Merge (ParameterConvert.GetMonsterAbility (monsters [i], enemyData.TeamData[0].Team[i].lv));
		}


		for (int i = 0;i<MyUserData.GetTeamData(0).Team.Count;i++) {
			characters[i] = MasterDataManager.GetCharaData (MyUserData.GetTeamData(0).Team[i].id);
			characters [i].Merge (ParameterConvert.GetCharaAbility (characters [i], MyUserData.GetTeamData (0).Team [i].lv));
		}

		for (int i = 0; i < monsterCdTimes.Length; i++) {
			monsterCdTimes [i] = 5;
		}

		UnLockOrder ();
	}

	public void FightStart(bool lockEnemy, List<int> canAttack, int[] cdTime){
		if (canAttack.Count > 0) {
			FightStart (canAttack.ToArray (), AtkType.pve);
		}
		if (cdTime.Any (t => t == 0)) {
			FightStart (cdTime, AtkType.evp);
		}
		onComplete.Invoke ();
	}

	private void FightStart(int[] attackIdx, AtkType aType){
		fightPairs = new Dictionary<int, int[]> ();
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

	private int[] matchAtkTarget(int jobIdx, AtkType aType){
		int[] atkOrder = new int[5];
		List<int> unLockIdx = new List<int> ();
		for (int i = 0; i < 5; i++) {
			if (!lockOrderIdx.Contains (i)) {
				unLockIdx.Add (i);
			}	
		}

		int orderIdx = 0;
		foreach (int i in CompareData (jobIdx, unLockIdx, aType).Reverse()) {
			atkOrder [orderIdx] = i;
			orderIdx++;	
		}

		return atkOrder;
	}

	private int[] CompareData(int orgIdx, List<int> unLockIdx, AtkType aType){
		List<AccordingData> according = new List<AccordingData> ();
		if (aType == AtkType.pve) {
			foreach (int idx in unLockIdx) {
				AccordingData data = new AccordingData ();
				data.index = idx;
				data.attriJob = GetCalcRatio (characters [orgIdx].job, monsters [idx].job, characters [orgIdx].attributes, monsters [idx].attributes);
				data.mAtkAtk = monsters [idx].mAtk + monsters [idx].atk;
				data.hp = monsters [idx].hp;
				data.def = monsters [idx].def;
				data.crt = monsters [idx].crt;
				according.Add (data);
			}

			return AccordingCompare (according.ToArray(), true);
		} 
		else {
			foreach (int idx in unLockIdx) {
				AccordingData data = new AccordingData ();
				data.index = idx;
				data.attriJob = GetCalcRatio (monsters [orgIdx].job, characters [idx].job, monsters [orgIdx].attributes, characters [idx].attributes);
				data.mAtkAtk = characters [idx].mAtk + characters [idx].atk;
				data.hp = characters [idx].hp;
				data.def = characters [idx].def;
				data.crt = characters [idx].crt;
				according.Add (data);
			}

			return AccordingCompare (according.ToArray());
		}
	}

	private float GetCalcRatio(int aj, int bj, int aa, int ba){
		return ParameterConvert.AttriRatioCal (aa, ba)*ParameterConvert.JobRatioCal (aj, bj);
	}

	private int[] AccordingCompare(AccordingData[] according, bool isPlayer = false){
		according = AccordingCompare (according, AccordingType.attriJob, isPlayer);
		int[] order = new int[5];
		for (int i = 0; i < according.Length; i++) {
			order [i] = according [i].index;
		}

		return order;
	}

	private AccordingData[] AccordingCompare(AccordingData[] according, AccordingType type, bool isPlayer = false){
		switch(type){
		case AccordingType.attriJob:
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.attriJob.CompareTo (y.attriJob));
			});
			if (isPlayer) {
				return according;
			}
			break;
		case AccordingType.mAtkAtk:
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.mAtkAtk.CompareTo (y.mAtkAtk));
			});
			break;
		case AccordingType.hp:
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.hp.CompareTo (y.hp));
			});
			break;
		case AccordingType.def:
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.def.CompareTo (y.def));
			});
			break;
		case AccordingType.crt:
			Array.Sort (according, delegate(AccordingData x, AccordingData y) {
				return(x.crt.CompareTo (y.crt));
			});
			return according;
		}

		Dictionary<int, int> splitDic = new Dictionary<int, int> ();
		for (int i = 0; i < according.Length; i++) {
			for (int j = 0; j < according.Length; j++) {
				int count = 0;
				switch(type){
				case AccordingType.attriJob:
					if (according [i].attriJob == according [j].attriJob && i != j) {
						count++;
					}
					break;
				case AccordingType.mAtkAtk:
					if (according [i].mAtkAtk == according [j].mAtkAtk && i != j) {
						count++;
					}
					break;
				case AccordingType.hp:
					if (according [i].hp == according [j].hp && i != j) {
						count++;
					}
					break;
				case AccordingType.def:
					if (according [i].def == according [j].def && i != j) {
						count++;
					}
					break;
				case AccordingType.crt:
					if (according [i].crt == according [j].crt && i != j) {
						count++;
					}
					break;
				}

				if (count > 0) {
					if (splitDic.Count > 0) {
						bool newDic = false;
						foreach (KeyValuePair<int, int> kv in splitDic) {
							if (splitDic.ContainsKey (i) && i > kv.Key + kv.Value) {
								newDic = true;	
							}
						}
						if (newDic) {
							splitDic.Add (i, count);
						}
					} else {
						splitDic.Add (i, count);
					}
				}
			}
		}


		foreach (KeyValuePair<int, int> kv in splitDic) {
			
			AccordingData[] accs = new AccordingData[kv.Value];
			Array.Copy (according, kv.Key, accs, 0, kv.Value);
			switch(type){
			case AccordingType.attriJob:
				accs = AccordingCompare (accs, AccordingType.mAtkAtk);
				break;
			case AccordingType.mAtkAtk:
				accs = AccordingCompare (accs, AccordingType.hp);
				break;
			case AccordingType.hp:
				accs = AccordingCompare (accs, AccordingType.def);
				break;
			case AccordingType.def:
				accs = AccordingCompare (accs, AccordingType.crt);
				break;
			}
			for (int i = kv.Key; i < kv.Value; i++) {
				for (int j = 0; j < accs.Length; j++) {
					according [i] = accs [j];
				}
			}
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

	struct AccordingData{
		public int index;
		public float attriJob;
		public int mAtkAtk;
		public int hp;
		public int def;
		public int crt;
	}

	enum AccordingType{
		attriJob,
		mAtkAtk,
		hp,
		def,
		crt
	}
}