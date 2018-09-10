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

	LinkedList<int> lockOrderIdx;

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

	public void FightStart(bool lockEnemy){
		
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
}
