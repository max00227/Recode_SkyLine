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

	private int[] enemyCdTimes = new int[5];

	LinkedList<int> lockOrderIdx = new LinkedList<int>();

	private Dictionary<int,AccordingData[]> fightPairs;

	private int[] cdTime;
    
	private int enemyProtect;

	private int charaProtect;

    private int uniteFullHp = 0;

    private int uniteHp = 0;

    private int uniteDef;
    private int uniteMDef;

    private int mainJob;

    private int mainAttri;

    private List<List<int>> playerConditions;

    private int[] playersActLevel;

    [HideInInspector]
	public ChessData[] players;
	[HideInInspector]
	public ChessData[] enemys;

    public List<int> canAttack;

	#region 主資料
	private ChessData mainOrgChess;
	private ChessData[] mainOrgsChess;

	private ChessData mainTargetChess;
	private ChessData[] mainTargetsChess;

	private string[] mainTarget;
	#endregion

	#region 副資料
	private ChessData deputyOrgChess;
	private ChessData[] deputyOrgsChess;

	private ChessData deputyTargetChess;
	private ChessData[] deputyTargetsChess;

	private string[] deputyTarget;
	#endregion

	float resetRatio;

	bool isLock = false;

	public Dictionary<int, Dictionary<int, List<DamageData>>> damageShowSort;

	public Dictionary<int, List<RuleLargeData>> charaTriggers;
	public Dictionary<int, List<RuleLargeData>> enemyTriggers;

	List<int> actIdx = new List<int> ();

	public FightStatus fightStatus;

    bool isSetJob = false;

    TeamLargeData teamData;

	void Update(){
	}


	public void SetData(){
		resetRatio = 1;

		skillController.SetData (SetCharaData (), SetEnemyData ());

		for (int i = 0; i < enemyCdTimes.Length; i++) {
			enemyCdTimes [i] = 5;
		}

        playersActLevel = new int[players.Length];

		SetAccordingDataDic ();

		CallbackProtect ();

        fightUIController.ChangeUniteHpBar((float)uniteHp / (float)uniteFullHp, true);

        InitAllConditions();

		UnLockOrder ();
	}

	private SoulLargeData[] SetEnemyData(){
		
		enemyProtect = 0;

		string enemyDataPath = "/ClientData/EnemyData.txt";

		System.IO.StreamReader sr = new System.IO.StreamReader (Application.dataPath + enemyDataPath);
		string json = sr.ReadToEnd();

		EnemyLargeData enemyData = JsonConversionExtensions.ConvertJson<EnemyLargeData>(json);

		SoulLargeData[] soulData = new SoulLargeData[enemyData.teams[0].member.Count];
		enemys = new ChessData[enemyData.teams[0].member.Count];

        int[] enemyAct = new int[5] { 100, 1, 0, 100, 1 };
		for (int i = 0;i<enemyData.teams[0].member.Count;i++) {
			enemys[i].soulData = MasterDataManager.GetSoulData (enemyData.teams[0].member[i].id);
			enemys[i].soulData.Merge (ParameterConvert.GetEnemyAbility (enemys[i].soulData, enemyData.teams[0].member[i].lv));
			enemys[i].soulData.Merge (enemys[i].soulData.skill);
			enemys[i].fullHp = enemys[i].soulData.abilitys["Hp"];
			enemys [i].status = new Dictionary<StatusLargeData, int> ();
			enemys [i].recStatus = new Dictionary<StatusLargeData, int> ();
			enemys [i].statusTime = new Dictionary<StatusLargeData, int> ();
			enemys [i].abiChange = new Dictionary<int, Dictionary<string, int>> ();
			enemys [i].hasStatus = new bool[Enum.GetNames (typeof(Status)).Length];
            enemys[i].act = enemyAct; 
			enemys [i].initAttri = enemys [i].soulData.attributes;
			soulData [i] = enemys [i].soulData;


			if (enemys[i].soulData.job == 2) {
				enemyProtect++;
			}
		}
		return soulData;
	}

	private SoulLargeData[] SetCharaData(){
        teamData = MyUserData.GetTeamData(0);
        SoulLargeData[] soulData = new SoulLargeData[teamData.member.Count];
		players = new ChessData[teamData.member.Count];

        for (int i = 0; i < teamData.member.Count; i++)
        {
            players[i].soulData = MasterDataManager.GetSoulData(teamData.member[i].id);
            players[i].soulData.Merge(ParameterConvert.GetCharaAbility(players[i].soulData, teamData.member[i].lv));
            players[i].soulData.Merge(players[i].soulData.skill);
            players[i].status = new Dictionary<StatusLargeData, int>();
            players[i].recStatus = new Dictionary<StatusLargeData, int>();
            players[i].statusTime = new Dictionary<StatusLargeData, int>();
            players[i].abiChange = new Dictionary<int, Dictionary<string, int>>();
            players[i].hasStatus = new bool[Enum.GetNames(typeof(Status)).Length];
            players[i].initAttri = players[i].soulData.attributes;
            players[i].condition = SetCondition(i, 0);
            soulData[i] = players[i].soulData;

            uniteHp += players[i].soulData.abilitys["Hp"];
            uniteFullHp += players[i].soulData.abilitys["Hp"];
        }

		return soulData;
	}

    public void InitAllConditions() {
        playerConditions = new List<List<int>>();
        for(int i = 0; i < players.Length; i++) { 
            fightUIController.SetButtonCondition(i,players[i].condition, true);
        }
    }

    public void ConditionDown(int[] down, bool chgJob = false)
    {
        Dictionary<int, int> overDowns = new Dictionary<int, int>();
        for (int i = 0; i < players.Length; i++)
        {
            int overDown = 0;
            if (playersActLevel[i] <= 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    players[i].condition[j] -= down[j];
                    {
                        if (players[i].condition[j] < 0)
                        {
                            overDown -= players[i].condition[j];
                            players[i].condition[j] = 0;
                        }
                    }
                }

                if (players[i].condition.Sum(x => Convert.ToInt32(x)) == 0)
                {
                    playersActLevel[i]++;
                    if (playersActLevel[i] <= 3)
                    {
                        players[i].condition = playersActLevel[i] < 3 ? SetCondition(i, playersActLevel[i]) : new int[3];
                        players[i].act = (int[])players[i].soulData.act[playersActLevel[i] - 1].Clone();

                        fightUIController.SetButtonCondition(i, players[i].condition, true, playersActLevel[i]);
                    }
                    else
                    {
                        fightUIController.SetButtonCondition(i, players[i].condition, false);
                    }
                    if (overDown > 0)
                    {
                        overDowns.Add(i, overDown);
                    }
                }
                else
                {
                    fightUIController.SetButtonCondition(i, players[i].condition, false);
                }
            }
        }

        if (!isSetJob && chgJob)
        {
            if (overDowns.Count > 1)
            {
                Dictionary<int, int> sort = overDowns.OrderBy(Data => Data.Value).ToDictionary(keyvalue => keyvalue.Key, keyvalue => keyvalue.Value);
                SetJob(sort.ElementAt(sort.Count - 1).Key);
            }
            else if (overDowns.Count == 1)
            {
                SetJob(overDowns.ElementAt(0).Key);
            }
        }
    }

    public void SetJob(int charaIdx)
    {
        isSetJob = true;
        mainJob = players[charaIdx].soulData.job;
        mainAttri = players[charaIdx].soulData.attributes;
        int abiChanged = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].soulData.job == mainJob)
            {
                abiChanged = 100;//GetAbiChange(i, 1, "Def");
                uniteDef += players[i].soulData.abilitys["Def"] * abiChanged / 100;
                abiChanged = 100;//GetAbiChange(i, 1, "mDef");
                uniteDef += players[i].soulData.abilitys["mDef"] * abiChanged / 100;
            }
        }
    }

    /// <summary>
    /// 產生傷害資料，並進行傷害公式
    /// </summary>
	private void OnFight(){
		damageShowSort = new Dictionary<int, Dictionary<int, List<DamageData>>> ();
		int count = mainTarget[0] == "E" ? enemys.Length : players.Length;
		for (int i = 0; i < count; i++) {
			mainOrgChess = mainTarget[0] == "E" ? enemys[i] : players[i];
			if (mainOrgChess.soulData.abilitys["Hp"] > 0) {
				//判斷是否全體攻擊
				bool isAll = false;
				if (mainOrgChess.soulData.job >= 3) {
					if (mainTarget[0] == "P") {
						if (playersActLevel[i] >= 2) {
							isAll = true;
						}
					}
					else {
						isAll = true;
					}
				}
                
				if (CheckStatus ((int)Nerf.Confusion, mainOrgChess, mainTarget[0])) {
					RandomFight (i, isAll);
				}
				else {
					NormalFight (i, isAll);
				}
			}
		}

		CallbackProtect ();
	}

	private void NormalFight(int selfIdx, bool isAll){
        //自動攻擊之敵人跳過盾職
		bool ignore = false;
        bool atkSuccess = false;
		if (mainTarget[1] == "P"){
            if (cdTime[selfIdx] == 0)
            {
                List<DamageData> allDamage = new List<DamageData>();
                float jobRatio = ParameterConvert.JobRatioCal(mainOrgChess.soulData.job, mainJob);
                float attriRatio = ParameterConvert.JobRatioCal(mainOrgChess.soulData.attributes, mainAttri);
                
                if (mainOrgChess.soulData.job <= 3)
                {
                    allDamage.Add(GetDamage(selfIdx, 0, attriRatio * jobRatio, DamageType.Physical, isAll));
                }

                if (mainOrgChess.soulData.job >= 3)
                {
                    allDamage.Add(GetDamage(selfIdx, 0, attriRatio * jobRatio, DamageType.Magic, isAll));
                }

                OnDamage(allDamage);
            }
		}
        else
        {
            if (fightPairs.ContainsKey(selfIdx))
            {
                AccordingData[] order;
                fightPairs.TryGetValue(selfIdx, out order);
                for (int i = 0; i < order.Length; i++)
                {
                    mainTargetChess = enemys[order[i].index];

                    if (mainTargetChess.soulData.abilitys["Hp"] > 0)
                    {
                        OnDamage(FightDamageData(selfIdx, order[i], isAll));
                        atkSuccess = true;
                    }


                    if ((!isAll && atkSuccess) || i == order.Length - 1)
                    {
                        break;
                    }
                }
            }
        }
	}

	private void RandomFight(int selfIdx, bool isAll){
		if (!isAll) {
			int randomIdx = UnityEngine.Random.Range (0, players.Length + enemys.Length);
			while (!CheckRandomDirect (randomIdx,selfIdx)) {
				randomIdx = UnityEngine.Random.Range (0, players.Length + enemys.Length);
			}
			mainTarget[1] = randomIdx >= players.Length ? "E" : "P";

			RandomFight (selfIdx, isAll, randomIdx);
		} 
		else {
			for (int i = 0; i < players.Length + enemys.Length; i++) {
				mainTarget[1] = i >= players.Length ? "E" : "P";
				if (CheckRandomDirect (i, selfIdx)) {
					RandomFight (selfIdx, isAll, i);
				}
			}
		}
	}

	private void RandomFight(int selfIdx, bool isAll, int randomIdx){
		/*int idx = randomIdx >= players.Length ? randomIdx - (players.Length) : randomIdx;
		mainTargetChess = randomIdx >= players.Length ? enemys [idx] : players [idx];
		if (mainTarget[1] == mainTarget[0]) {
            //打自己人時需另計傷害加成值
			int actLevel = mainTarget[1] == "P" ? 0 : fightUIController.GetActLevel(mainOrgChess.soulData.job) - 1;
			float attriRatio = ParameterConvert.AttriRatioCal (mainOrgChess.soulData.act [actLevel], mainTargetChess.soulData.attributes);
			float jobRatio = ParameterConvert.AttriRatioCal (mainOrgChess.soulData.job, mainTargetChess.soulData.job);
			OnDamage (FightDamageData (selfIdx, attriRatio, jobRatio, mainTargetChess.according [idx], isAll));
		} 
		else {
			OnDamage (FightDamageData (selfIdx, mainOrgChess.according [idx], isAll));
		}*/
	}

	private List<DamageData> FightDamageData(int selfIdx, AccordingData orderData, bool isAll){
		List<DamageData> allDamage = new List<DamageData> ();
		if (mainOrgChess.soulData.job <= 3) {
			allDamage.Add (GetDamage (selfIdx, orderData.index, orderData.attriRatio * orderData.jobRatio, DamageType.Physical, isAll));
		}

		if (mainOrgChess.soulData.job >= 3) {
			allDamage.Add (GetDamage (selfIdx, orderData.index, orderData.attriRatio * orderData.jobRatio, DamageType.Magic, isAll));
		}

		return allDamage;
	}

	private List<DamageData> FightDamageData(int selfIdx, float attriRatio, float jobRatio, AccordingData orderData, bool isAll){
		List<DamageData> allDamage = new List<DamageData> ();
		if (mainOrgChess.soulData.job <= 3) {
			allDamage.Add (GetDamage (selfIdx, orderData.index, attriRatio * jobRatio, DamageType.Physical, isAll));
		}

		if (mainOrgChess.soulData.job >= 3) {
			allDamage.Add (GetDamage (selfIdx, orderData.index, attriRatio * jobRatio, DamageType.Magic, isAll));
		}

		return allDamage;
	}

	private bool CheckRandomDirect(int randomIdx, int selfIdx){
		if (randomIdx >= players.Length + enemys.Length) {
			return false;
		}
		if (randomIdx >= players.Length) {
			if (mainTarget[0] == "E") {
				if ((randomIdx - (players.Length - 1)) == selfIdx) {
					return false;
				}
			}
		}
		else {
			if (mainTarget[0] == "P") {
				if (randomIdx == selfIdx) {
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// 建立傷害資料
	/// <returns>The damage.</returns>
	/// <param name="orgIdx">攻擊者索引值，建立傷害資料用</param>
	/// <param name="targetIdx">被攻擊者索引值，建立傷害資料用</param>
	/// <param name="attriJob">攻剋倍率</param>
	/// <param name="minus">檢傷值</param>
	/// <param name="dType">傷害類型</param>
	/// <param name="isAll">是否為全體攻擊，會影響浮動值</param>
    /// </
	private DamageData GetDamage (int orgIdx, int targetIdx,float attriJob, DamageType dType, bool isAll){
		DamageData damageData;
		int actLevel = mainTarget[0] == "E" ? 0 : playersActLevel[orgIdx] - 1;
        int ratio = mainTarget[0] == "E" ? 0 : 0;

		string titleKey = dType == DamageType.Physical ? "" : "m";

        int orgChanged = 100;

        int targetChanged = 100;

        //Status Affect
        int orgStatusRatio = 100;
        int targetStatusRatio = 100;

        /*if (mainTarget[0] == "E")
        {
            orgChanged = GetAbiChange(orgIdx, 0, titleKey + "Atk");
            orgStatusRatio = 100 + GetAbilityStatus(titleKey + "Atk", true);
        }
        if (mainTarget[1] == "E") {
            targetChanged = GetAbiChange(orgIdx, 0, titleKey + "Atk");
            targetStatusRatio = 100 + GetAbilityStatus(titleKey + "Atk", true);
        }*/

        int targetDef = mainTarget[1] == "P" ? uniteDef : mainTargetChess.soulData.abilitys[titleKey + "Def"];

        int crt = (mainOrgChess.soulData.job != 2 && mainOrgChess.soulData.job != 4) ? mainOrgChess.soulData.abilitys["Spc"] : 0;

        damageData = CalDamage (
			mainOrgChess.soulData.abilitys [titleKey + "Atk"] * orgChanged / 100 * orgStatusRatio / 100,
            targetDef * targetChanged / 100 * targetStatusRatio / 100, 
			mainOrgChess.act, 
			attriJob,
            crt,
            isAll
		);
		damageData.damageType = dType;

		damageData.tType = (string[])mainTarget.Clone();
		damageData.attributes = mainOrgChess.soulData.act [actLevel][2];
		damageData.orgIdx = orgIdx;
		damageData.targetIdx = targetIdx;
        damageData.atkJob = mainOrgChess.soulData.job;

		return damageData;
	}

	/// <summary>
	/// 進行攻擊
	/// </summary>
	/// <param name="allDamage">全部的傷害資料</param>
	private void OnDamage (List<DamageData> allDamage){
		if (damageShowSort.ContainsKey (allDamage [0].orgIdx)) {
			if (damageShowSort [allDamage [0].orgIdx].ContainsKey (allDamage [0].targetIdx)) {
				//targetIdx重複時會加10做區分，之後會自己做判斷
				damageShowSort [allDamage [0].orgIdx].Add (allDamage [0].targetIdx + 10, OnDamageList (allDamage));
			} 
			else {
				damageShowSort [allDamage [0].orgIdx].Add (allDamage [0].targetIdx, OnDamageList (allDamage));
			}
		} 
		else {
			damageShowSort.Add (allDamage[0].orgIdx, new Dictionary<int, List<DamageData>> ());
			damageShowSort [allDamage[0].orgIdx].Add (allDamage[0].targetIdx, OnDamageList (allDamage));
		}
	}

	private List<DamageData> OnDamageList(List<DamageData> allDamage){
		List<DamageData> damageList = new List<DamageData> ();

		//屬性攻擊追加負面狀態
		if (allDamage [0].attributes != 0 && allDamage [0].attributes != mainTargetChess.initAttri) {
			if (UnityEngine.Random.Range (0, 101) <= 10) {
				if (GetStatus (allDamage [0].attributes, mainTargetChess) != null) {
					if (mainTargetChess.status [GetStatus (allDamage [0].attributes, mainTargetChess)] < 5) {
						mainTargetChess.status [GetStatus (allDamage [0].attributes, mainTargetChess)]++;
					}
				} 
				else {
					mainTargetChess.status.Add (MasterDataManager.GetStatusData(allDamage [0].attributes), 0);
				}
			}
		}

		foreach (DamageData damageData in allDamage) {
			damageList.Add (OnDamageData (damageData));
		}

		return damageList;
	}

	/// <summary>
	/// 計算被攻擊者受傷後資料.
	/// </summary>
	/// <param name="targetData">被攻擊者資料.</param>
	/// <param name="damageData">傷害資料.</param>
	private DamageData OnDamageData(DamageData damageData){
		DamageData data = damageData;
		bool isDead = false;


        if (damageData.tType[1] == "E")
        {
            data.hpRatio = new float[damageData.damage.Length];
            for (int i = 0; i < damageData.damage.Length; i++)
            {
                mainTargetChess.soulData.abilitys["Hp"] -= data.damage[i];
                if (mainTargetChess.soulData.abilitys["Hp"] <= 0)
                {
                    if (mainTargetChess.hasStatus[(int)Status.Suffer] == false)
                    {
                        mainTargetChess.soulData.abilitys["Hp"] = 0;
                        isDead = true;
                    }
                    else
                    {
                        mainTargetChess.soulData.abilitys["Hp"] = 1;
                    }
                }

                if (isDead)
                {
                    OnDeath(damageData.targetIdx);
                }
                data.hpRatio[i] = (float)mainTargetChess.soulData.abilitys["Hp"] / (float)enemys[damageData.targetIdx].fullHp;
            }
        }
        else {
            data.hpRatio = new float[1];
            uniteHp -= data.damage[0];
            data.hpRatio[0] = (float)uniteHp / (float)uniteFullHp;
        }


		return data;
	}


	private void OnDeath(int idx){
		mainTargetsChess = mainTarget[1] == "P" ? players : enemys;
		mainOrgsChess = mainTarget[0] == "E" ? players : enemys;
		int deleteKey = mainTarget[1] == "P" ? players[idx].soulData.skill : enemys[idx].soulData.skill;

        //我方陣營附加狀態
		foreach (var data in mainTargetsChess){
            if (data.abiChange.ContainsKey(deleteKey))
            {
                data.abiChange.Remove(deleteKey);
            }
        }

        //敵對陣營附加狀態
		foreach (var data in mainOrgsChess){
            if (data.abiChange.ContainsKey(deleteKey))
            {
                data.abiChange.Remove(deleteKey);
            }
        }

		mainTargetChess.abiChange = new Dictionary<int, Dictionary<string, int>> ();
		mainTargetChess.status = new Dictionary<StatusLargeData, int> ();
		mainTargetChess.recStatus = new Dictionary<StatusLargeData, int> ();
		mainTargetChess.statusTime = new Dictionary<StatusLargeData, int> ();
		mainTargetChess.hasStatus = new bool[Enum.GetNames (typeof(Status)).Length];

		fightUIController.OnDead(idx,mainTarget);
	}

	/// <summary>
	/// 計算傷害值
	public DamageData CalDamage(int atk, int def, int[] act, float ratioAJ, int crt, bool isAll){
		DamageData damageData = new DamageData ();
        damageData.damage = new int[act[1]];
        damageData.isCrt = new bool[act[1]];

        for (int i = 0; i < damageData.damage.Length; i++)
        {
            damageData.isCrt[i] = UnityEngine.Random.Range(0, 100) < crt;

            float crtRatio = Mathf.Pow(2f, Convert.ToInt32(damageData.isCrt[i]));

            int hitRate = damageData.isCrt[i] == true ? 99 : UnityEngine.Random.Range(0, 100);
            //爆擊時必命中
            bool isMiss = damageData.isCrt[i] == true ? false : !(hitRate < act[3]);
            float radio = isAll == true ? (mainTarget[1] == "E" ? 60 : 100f + (float)fightUIController.GetCharaGround() * 10) : 100;

            if (!isMiss)
            {
                radio -= ((99 - hitRate) / 10);
            }

            int finalDef = mainOrgChess.hasStatus[(int)Status.UnDef] == true ? 0 : def;

            int damage = 0;
            if(isMiss == false)
            {
                if (act[0] == 999)
                {
                    damage = 999999999;
                }
                else {
                    damage = Mathf.CeilToInt((atk * (act[0] / 100) * radio / 100 * ratioAJ * resetRatio * crtRatio - finalDef));
                    //((Atk * randamRatio * ratio * ratioAJ * resetCount) * isCrt - finalDef) * finalMinus
                }
            }

            damageData.damage[i] = isMiss == true ? 0 : damage <= 0 ? 1 : damage;//狀態傷害加成
        }
		return damageData;
	}

	private void CheckSoulStatus(){
		CheckSoulStatus (players, "P");
		CheckSoulStatus (enemys, "E");
	}

	private void CheckSoulStatus(ChessData[] chessData,string targetString){
		for (int i = 0; i < chessData.Length; i++) {
			Dictionary<StatusLargeData, int> orgData = new Dictionary<StatusLargeData, int> (chessData [i].status);
			foreach (KeyValuePair<StatusLargeData, int> kv in orgData) {
				if (chessData [i].recStatus.ContainsKey (kv.Key)) {
					if (chessData [i].recStatus [kv.Key] != kv.Value) {
						chessData [i].statusTime [kv.Key] = MasterDataManager.GetStatusData (kv.Key.id).rmParam;
						fightUIController.OnStatus (i, MasterDataManager.GetStatusData (kv.Key.id), kv.Value, targetString);
					} 
					else {
						chessData [i].statusTime [kv.Key]--;
						fightUIController.OnStatusDown (i, kv.Key, chessData [i].statusTime [kv.Key], targetString);
						if (chessData [i].statusTime [kv.Key] == 0) {
							chessData [i].statusTime.Remove (kv.Key);
							chessData [i].status.Remove (kv.Key);
						}
					}
				} 
				else {
					chessData [i].statusTime.Add (kv.Key, MasterDataManager.GetStatusData (kv.Key.id).rmParam);
					fightUIController.OnStatus (i, MasterDataManager.GetStatusData (kv.Key.id), kv.Value, targetString);
				}
			}


			chessData [i].recStatus = new Dictionary<StatusLargeData, int> (chessData [i].status);
		}
	}

	private StatusLargeData GetStatus(int statusId, ChessData chessData){
		foreach (KeyValuePair<StatusLargeData, int> kv in chessData.recStatus) {
				if (kv.Key.id == statusId) {
					return kv.Key;
				}
		}

		return null;
	}

	private bool CheckStatus(int statusType, ChessData chessData, string targetString){
		foreach (KeyValuePair<StatusLargeData, int> kv in chessData.recStatus) {
			if (targetString == "P") {
				Debug.Log (kv.Key.charaStatus);
				if (kv.Key.charaStatus == statusType) {
					return true;
				}
			} 
			else {
				if (kv.Key.enemyStatus == statusType) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// 有參數之狀態確認
	private void ParameterStatus(int param, int statusType, ChessData chessData, string targetString, int? idx){
		foreach (KeyValuePair<StatusLargeData, int> kv in chessData.recStatus) {
			if (targetString == "P") {
				if (kv.Key.charaStatus == statusType) {
					OnStatusParam (kv.Key, kv.Value, param, chessData, idx);
				}
			} 
			else {
				if (kv.Key.enemyStatus == statusType) {
					OnStatusParam (kv.Key, kv.Value, param, chessData, idx);
				}
			}
		}
	}

	private int GetAbilityStatus(string abiKey, bool isReverse = false){
		int param = 0;
		if (!isReverse) {
			foreach (KeyValuePair<StatusLargeData, int> kv in mainTargetChess.recStatus) {
				if (kv.Key.statusParam.ContainsKey (abiKey)) {
					param += kv.Key.statusParam [abiKey] [kv.Value];
				}
			}
		} 
		else {
			foreach (KeyValuePair<StatusLargeData, int> kv in mainOrgChess.recStatus) {
				if (kv.Key.statusParam.ContainsKey (abiKey)) {
					param += kv.Key.statusParam [abiKey] [kv.Value];
				}
			}
		}

		return param;
	}

	private void OnStatusParam(StatusLargeData statusData, int level, int param, ChessData chessData, int? idx){
		foreach (KeyValuePair<string,int[]> kv in statusData.statusParam) {
			switch (kv.Key) {
			case "Hp":
				chessData.soulData.abilitys ["Hp"] += chessData.fullHp * kv.Value [level] * param / 100;

				fightUIController.ChangeHpBar ((int)idx, "P", (float)mainTargetChess.soulData.abilitys ["Hp"] / (float)mainTargetChess.fullHp, false);
				break;
			}
		}
	}

	/// <summary>
	/// 表現傷害動畫
	/// <returns>The fight.</returns>
	/// <param name="tType">被攻擊者陣營</param>
	/// <param name="Callback">是否戰鬥結束的Callback</param>
	IEnumerator ShowFight(bool Callback){
		int count = 0;
		while (count < damageShowSort.Count) {
			foreach (KeyValuePair<int, Dictionary<int, List<DamageData>>> data in damageShowSort) {
				ShowFight (data.Key, data.Value);
				count++;
				yield return new WaitForSeconds(0.5f);
			}
		}

		if (Callback) {
			CheckSoulStatus ();
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
	void ShowFight(int orgIdx, Dictionary<int, List<DamageData>> damageData){
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
	public void FightStart(bool lockEnemy){
		bool enemyFight = DataUtil.CheckArray<int> (cdTime, 0);

		for(int i = 0;i<playersActLevel.Length;i++){
			if (playersActLevel[i]!=0 && !canAttack.Contains(i)) {
                canAttack.Add(i);
			}
		}

		if (canAttack.Count > 0) {
			mainTarget = TargetType.P_E.ToString().Split('_');
			FightPairs (canAttack.ToArray ());
			OnFight ();
			StartCoroutine (ShowFight (!enemyFight));
		}
        else{
            EnemyFight();
        }
	}

	/// <summary>
	/// 敵人攻擊
	public void EnemyFight(){
		mainTarget = TargetType.E_P.ToString().Split('_');
        //FightPairs(cdTime);
		OnFight ();
		StartCoroutine (ShowFight (true));
	}

	/// <summary>
	/// 攻擊目標配對
	/// <param name="attackIdx">可攻擊清單</param>
	/// <param name="tType">目標陣營.</param>
	/// <param name="actLevel">攻擊者攻擊階級<param>
	private void FightPairs(int[] attackIdx){
		fightPairs = new Dictionary<int, AccordingData[]> ();
		if (mainTarget[0] == "P") {
			foreach (int idx in attackIdx) {
				fightPairs.Add (idx, matchTarget (idx));
			}
		} 
	}

	/// <summary>
	/// 配對攻擊目標並排列順序
	/// <returns>The target.</returns>
	/// <param name="idx">攻擊者索引值</param>
	/// <param name="tType">被攻擊者陣營</param>
	private AccordingData[] matchTarget(int idx){

		AccordingData[] compareOrder = CompareData (idx, mainTarget[1]);

		int orderCount = mainTarget[1] == "P" ? players.Length : enemys.Length;

		AccordingData[] atkOrder = new AccordingData[orderCount];
		AccordingData[] lockOrder = new AccordingData[lockOrderIdx.Count];



		if (mainTarget[1] == "P") {
			return compareOrder;
		} 
		else {
			if (!isLock) {
				atkOrder = compareOrder;
			} 
			else {
				for (int i = 0; i < lockOrderIdx.Count; i++) {
					atkOrder [i] = GetAccordingData (idx, lockOrderIdx.ElementAt (i));
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
	private AccordingData[] CompareData(int orgIdx, string targetString){
		//因應玩家角色攻擊屬性會變換更改According資料
		if (targetString == "E") {
			for (int i = 0; i < players [orgIdx].according.Length; i++) {
                players[orgIdx].according[i].attriRatio = 
                    ParameterConvert.AttriRatioCal(players[orgIdx].soulData.act[playersActLevel[orgIdx] - 1][2], enemys[i].soulData.attributes);
			}
		}

		AccordingData[] according = (AccordingData[])players[orgIdx].according.Clone();

		return AccordingCompare (according);
	}

	/// <summary>
	/// 進行目標排序，目標為敵人時因為玩家可做簡單排序，因此只對克制加成做排序，若目標為玩家會以克制加成>>當下血量>>爆擊值>>減傷>>攻擊傷害順序做排列
	/// <returns>The compare.</returns>
	/// <param name="according">根據值</param>
	/// <param name="isPlayer">被攻擊者陣營</param>
	private AccordingData[] AccordingCompare(AccordingData[] according){
		Array.Sort (according, delegate(AccordingData x, AccordingData y) {
			return((x.attriRatio * x.jobRatio).CompareTo (y.attriRatio * y.jobRatio)) * -1;
		});


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
			for (int i = 0; i < enemys.Length; i++) {
                enemys[i].minus = cdTime[i] == 0? 100 - (0 + enemyProtect * 10):100 - (50 * (10 + enemyProtect) / 10);
			}
		}
	}

	/// <summary>
	/// 加快該局後期節奏，會因重製版面次數影響倍率
	/// <param name="count">Count.</param>
	public void SetResetRatio(int count){
		resetRatio = Mathf.Pow (1.5f, count);
        ResetGround();
	}

	private enum AccChangeType{
		AttriRatio,
		JobRatio,
	}

	/// <summary>
	/// 變更根據值
	/// <param name="idx">Index.</param>
	/// <param name="hp">Hp.</param>
	/// <param name="tType">T type.</param>
	private void ChangeAccordingData(int idx, int parameter, string targetString, AccChangeType acType){
		ChessData[] targetsChess = targetString == "P" ? enemys : players;
		for (int i = 0; i < targetsChess.Length; i++) {
			for (int j = 0; j < targetsChess [i].according.Length; j++) {
				if (idx == j) {
					ChangeAccordingData (targetsChess [i].according [j], parameter, acType);
				} 
			}
		}
	}

	private void ChangeAccordingData(AccordingData data, float param, AccChangeType acType){
        switch (acType)
        {
            case AccChangeType.AttriRatio:
                data.attriRatio = param;
                break;
            case AccChangeType.JobRatio:
                data.jobRatio = param;
                break;
        }
	}

	/// <summary>
	/// 建立各角色對相對陣營的攻擊順序根據值清單
	private void SetAccordingDataDic(){
		ChessData[] charaData = new ChessData[0];
		charaData = (ChessData[])players.Clone();
		ChessData[] enemyData = new ChessData[0];
        enemyData = (ChessData[])enemys.Clone();

        for (int i = 0; i < charaData.Length; i++) {
			AccordingData[] data = new AccordingData[enemys.Length];
			for(int j = 0;j<enemyData.Length; j++){
				data[j] = GetAccording(charaData[i].soulData, enemyData[j].soulData, j, "P");
			}
			players [i].according = data;
		}
	}

	/// <summary>
	/// 建立攻擊順序根據值
	/// <returns>The according.</returns>
	/// <param name="orgData">攻擊者資料</param>
	/// <param name="targetData">被攻擊者資料</param>
	/// <param name="targetIdx">被攻擊者索引值</param>
	/// <param name="tType">目標陣營，減傷值會因陣營有所不同</param>
	private AccordingData GetAccording(SoulLargeData orgData, SoulLargeData targetData, int targetIdx,string targetString){
		AccordingData data = new AccordingData ();
		data.index = targetIdx;
		data.jobRatio = ParameterConvert.JobRatioCal (orgData.job, targetData.job);

		return data;
	}

	/// <summary>
	/// 取得根據值資料
	/// <returns>The according data.</returns>
	/// <param name="orgIdx">攻擊者索引</param>
	/// <param name="idx">被攻擊者索引</param>
	/// <param name="tType">被攻擊者索引值</param>
	private AccordingData GetAccordingData(int orgIdx, int targetIdx){
		if (mainTarget[1] == "E") {
			return players[orgIdx].according[targetIdx];
		}
		else {
			return enemys[orgIdx].according[targetIdx];
		}
	}

	public void FightEnd(){
		/*for (int i = 0; i < skillCdTime.Length; i++) {
			if (skillCdTime [i] > 0) {
				skillCdTime [i]--;
				if (skillCdTime [i] == 0) {
					fightUIController.OnSkillCDEnd (i);
				}
			}
		}*/
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

	public void SelectSkillTarget(string targetString, int idx){
		//skillController.SelectSkillTarget (targetString, idx);
	}

	public void OnSelectSkillTarget(List<int> idxList, string targetString){
		fightUIController.OnSelectionDir (ExcludeTarget (idxList, targetString), targetString);
	}

	public List<int> ExcludeTarget(List<int> idxList, string targetString){
		int count = targetString == "P" ? players.Length : enemys.Length;


		for(int i = 0;i<count;i++){
			if (targetString == "P") {
				if (players [i].hasStatus[(int)Status.UnDirect] == true) {
					idxList.Remove (i);
				}
			} else {
				if (enemys [i].hasStatus[(int)Status.UnDirect] == true) {
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
    public bool OnRuleMeets(int idx, int[] ruleId, string targetString)
    {
        if (targetString == "P")
        {
            return playersActLevel[idx] >= teamData.member[idx].skillSet - 1;
            //return OnEnemyRule(idx, ruleId);
        }
        else
        {
            return OnEnemyRule(idx, ruleId);
        }
    }

    public bool OnCharacterRule(int idx ,int[] ruleId){
		if (playersActLevel[idx] > 0 && players [idx].soulData.abilitys["Hp"]>0) {
            switch (ruleId[0])
            {
                case 0:
                    return true;
                case 1:
                    return (players[idx].soulData.abilitys["Hp"] / players[idx].fullHp * 100) < ruleId[1];
                case 2:
                    return (players[idx].soulData.abilitys["Hp"] / players[idx].fullHp * 100) >= ruleId[1];
                case 11:
                    return true;
                case 12:
                    return true;
            }
        }
		return false;
	}

	public bool OnEnemyRule(int idx ,int[] ruleId){
		if (enemys [idx].soulData.abilitys ["Hp"] > 0) {
			switch (ruleId[0]) {
			case 1:
				return (enemys [idx].soulData.abilitys ["Hp"] / enemys [idx].fullHp * 100) < ruleId[1];
			case 2:
				return (enemys [idx].soulData.abilitys ["Hp"] / enemys [idx].fullHp * 100) >= ruleId[1];
			}
		}
		return false;
	}

	//取決練線數之狀態
	public void OnHitCountStatus(List<RaycastData> dataList){
		int hitCount = 0;
		foreach (RaycastData data in dataList) {
			hitCount += data.hits.Count;
		}
		foreach (RaycastData data in dataList) {
			for (int i = 0; i < players.Length; i++) {
				if (players [i].soulData.job == data.CharaJob) {
					ChessData targetChess = players [i];

					ParameterStatus (hitCount, (int)Nerf.Hit, targetChess, "P", i);
				}
			}
		}
	}


		
	public void RoundEnd(){
		//skillController.OnRoundSkill ();
        isSetJob = false;
	}

    public void ResetGround() {
        for (int i = 0; i < playersActLevel.Length; i++) {
            if (playersActLevel[i] == 2)
            {
                playersActLevel[i] = 1;
            }
            else {
                playersActLevel[i] = 0;
            }
        }
        for (int i = 0; i < players.Length; i++)
        {
            players[i].condition = SetCondition(i, playersActLevel[i]);
            if (playersActLevel[i] > 0)
            {
                if (playersActLevel[i] < 3)
                {
                    players[i].act = players[i].soulData.act[playersActLevel[i] - 1];
                }
            }
            else {
                players[i].act = null;
            }
            fightUIController.SetButtonCondition(i, players[i].condition, true, playersActLevel[i]);
        }
        canAttack = new List<int>();
    }

    public void OnTriggerSkill(List<DamageData> allDamage){
		int minusCount = allDamage [0].targetIdx >= 10 ? 10 : 0; 
		ChessData orgChess = allDamage [0].tType [0] == "E" ? enemys [allDamage [0].targetIdx] : players [allDamage [0].orgIdx];
		ChessData targetChess = allDamage [0].tType [1] == "E" ? enemys [allDamage [0].targetIdx] : players [allDamage [0].orgIdx];

		//skillController.OnTriggerSkill (orgChess, targetChess, allDamage);
	}

	public ChessData GetChessData(string targetString,int index){
		if (targetString == "P") {
			return players [index];
		} 
		else {
			return enemys [index];
		}
	}

    public void OnBrust() { 
        for(int i = 0; i < players.Length; i++)
        {
            if (playersActLevel[i] == 3) {
                players[i].act = players[i].soulData.act[2];
            }
        }
    }

    public void OnHealing(int healRatio) {
        int healParam = 0;
        for (int i = 0;i<players.Length;i++) {
            if ((players[i].soulData.job == 2 || players[i].soulData.job == 4) && playersActLevel[i]>0) {
                healParam += players[i].soulData.abilitys["Spc"] * healRatio;
            }
        }

        uniteHp += healParam;
        fightUIController.SetUniteHp((float)uniteHp / (float)uniteFullHp, true);
    }


	#region Skill
	/// <summary>
	/// 發動效果，技能分為一般跟狀態類效果
	/// <param name="orgIdx">發動者索引</param>
	/// <param name="idxList">對象清單</param>
	/// <param name="data">技能效果資料</param>
	/// <param name="targetType">目標類型 玩家：敵人</param>
	/// <param name="paramater">效果參數</param>
	public void OnSkillEffect(int orgIdx, List<int> idxList, RuleLargeData data, string[] skillTarget ,int skillId, bool isExpend = false){
		if (isExpend) {
			fightUIController.AddEnerge (data.energe * -1);
		}
		deputyTarget = skillTarget;
		deputyOrgChess = skillTarget [0] == "P" ? players [orgIdx] : enemys [orgIdx];

		foreach (int idx in idxList) {
			deputyTargetChess = skillTarget [1] == "E" ? players [idx] : enemys [idx];

			if (data.effectType == 1) {
				OnStatus (orgIdx, idx, data, skillTarget[0], skillId);
			} 
			else {
				OnNormal (orgIdx, idx, data, skillTarget[1]);
			}
		}
	}


	/// <summary>
	/// 一般類型既能函式
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.Recovery = 1,Act = 2,Cover = 3,RmAlarm = 4,RmNerf = 5,Dmg = 6,Exchange = 7,Call = 8</param>
	/// <param name="paramater">效果參數.</param>
	private void OnNormal(int orgIdx, int idx, RuleLargeData data, string targetString){

		switch (data.effect[0]) {
		case (int)Normal.Recovery:
			OnRecovery (orgIdx, idx, data, targetString);
			break;
		case (int)Normal.Act:
			actIdx.Add (idx);
			break;
		case (int)Normal.Cover:
			fightUIController.OnCover ();
			break;
		case (int)Normal.RmAlarm:
			OnRmAlarm (orgIdx);
			break;
		case (int)Normal.RmNerf:
			OnRmNerf (idx, targetString);
			break;
		case (int)Normal.Energe:
			fightUIController.AddEnerge (data.effect [1]);
			break;
		case (int)Normal.LockEng:
			fightUIController.LockEnemy (data.effect [1]);
			break;
		}
	}

	/// <summary>
	/// 狀態附加類型技能函式
	/// <param name="idxList">目標情單.</param>
	/// <param name="data">技能效果資料.UnDef = 1,UnNerf = 2,AddNerf = 3,Suffer = 4,Maximum = 5,Ability = 6,UnDirect = 7</param>
	/// <param name="paramater">效果參數.</param>
	private void OnStatus(int orgIdx, int idx, RuleLargeData data, string targetString, int skillId){
		deputyOrgChess.hasStatus [data.effect [0]] = true;

		OnAbilityChanged (idx, data, targetString, skillId);

		switch (data.effect [0]) {
		case (int)Status.AddNerf:
			OnAddNerf (idx,skillId);
			break;
		}
	}

	private void OnRecovery(int orgIdx, int idx, RuleLargeData data, string targetString ){
		int over = 0;

		if (deputyTargetChess.soulData.abilitys ["Hp"] > 0) {

			//狀態減免治療效果
			int minus = 100 + GetAbilityStatus ("Rcy");

			minus = minus < 0 ? 0 : minus;
			deputyTargetChess.soulData.abilitys ["Hp"] += data.effect [1] * minus / 100;
			if (deputyTargetChess.soulData.abilitys ["Hp"] > deputyTargetChess.fullHp) {
				over = deputyTargetChess.soulData.abilitys ["Hp"] - deputyTargetChess.fullHp;
				deputyTargetChess.soulData.abilitys ["Hp"] = deputyTargetChess.fullHp;
			}
			fightUIController.OnRecovery (idx, targetString, (float)deputyTargetChess.soulData.abilitys ["Hp"] / (float)deputyTargetChess.fullHp);

			if (over > 0) {
				//skillController.OverRecovery (idx, orgIdx, over, deputyTarget);
			}
		}
	}

	private void OnRmAlarm(int idx){
		if (cdTime [idx] == 1) {
			cdTime [idx]++;
			fightUIController.OnRmAlarm (cdTime [idx], idx);
		}
	}
		
	private void OnRmNerf(int idx, string targetString){
		Dictionary<StatusLargeData,int> orgData = deputyTargetChess.status;
		foreach (KeyValuePair<StatusLargeData,int> kv in orgData) {
			if (kv.Key.canRemove) {
				fightUIController.RmStatus (idx, kv.Key, targetString);
				deputyTargetChess.status.Remove (kv.Key);
			}
		}
	}

	private void OnRevive(int orgIdx, int idx, RuleLargeData data, string targetString){
		if (deputyTargetChess.soulData.abilitys ["Hp"] <= 0) {

			deputyTargetChess.soulData.abilitys ["Hp"] += deputyTargetChess.fullHp * data.effect [1] / 100;
			fightUIController.OnRecovery (idx, targetString, (float)deputyTargetChess.soulData.abilitys ["Hp"] / (float)deputyTargetChess.fullHp);
		}
	}

	private void OnAddNerf(int id, int skillId){
		/*if (!orgChess.hasStatus [(int)Status.UnNerf]) {
			orgChess.status.Add (skillId);
		}*/
	}

	#endregion

	private void OnAbilityChanged(int idx, RuleLargeData data, string targetString, int skillId){
		deputyOrgsChess = targetString == "P" ? enemys : players;
		deputyTargetsChess = targetString == "P" ? players : enemys;

		if (!deputyTargetChess.abiChange.ContainsKey (skillId)) {
			deputyTargetChess.abiChange.Add (skillId, data.abilitys);
		}
	}

	private int GetAbiChange (int idx,int targetIdx, string abiKey, bool isMain = true){
		int changeParam = 100;
		ChessData chess;
		if (isMain) {
			chess = mainTarget [targetIdx] == "P" ? players [idx] : enemys [idx];
		} else {
			chess = deputyTarget [targetIdx] == "P" ? players [idx] : enemys [idx];
		}

		foreach (var value in chess.abiChange.Values) {
			if (value.ContainsKey (abiKey)) {
				changeParam += value [abiKey];
			}
		}

		return changeParam;
	}

    public int[] SetCondition(int playerIdx,int level) {
        if(level == teamData.member[playerIdx].skillSet - 1)
        {
            int[] condition = new int[3];
            for(int i = 0; i < condition.Length; i++)
            {
                condition[i] = players[playerIdx].soulData.actCondition[level][i] + players[playerIdx].soulData._skill.condition[level][i];
            }

            return condition;
        }
        else {
            return (int[])players[playerIdx].soulData.actCondition[level].Clone();
        }
    }


    public void ShowData(){
        int healParam = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].soulData.job == 2 || players[i].soulData.job == 4)
            {
                healParam += players[i].soulData.abilitys["Spc"];
            }
        }
        Debug.Log(uniteHp);
        Debug.Log(healParam);
    }

    public void TestFunction(){
		if (GetStatus (9, players [4]) != null) {
			if (players [4].status [GetStatus (9, players [4])] < 4) {
				players [4].status [GetStatus (9, players [4])]++;
			}
		}
		else {
			players [4].status.Add (MasterDataManager.GetStatusData(9), 0);
		}


		CheckSoulStatus ();
	}

	public void ShowSoulDataC(){
		Debug.Log (UnityEngine.Random.Range (0, 5));
	}
}


public enum TargetType{
	P_E,
	E_P,
	P_P,
    E_E,
    All
}

public struct DamageData{
	public int orgIdx;
	public int targetIdx;
	public int[] damage;
	public float[] hpRatio;
	public DamageType damageType;
	public int attributes;
    public int atkJob;
	public bool[] isCrt;
	public string[] tType;
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
	None,
	Physical,
	Magic
}

/// <param name="data">技能效果資料.UnDef = 1,UnNerf = 2,AddNerf = 3,Suffer = 4,Maximum = 5,Ability = 6,UnDirect = 7</param>
public struct ChessData{
	public SoulLargeData soulData;
	public AccordingData[] according;
	public int fullHp;
	/// <summary>
	/// key = StatusData, value = Status Level
	public Dictionary<StatusLargeData, int> status;
	public Dictionary<StatusLargeData, int> recStatus;
	/// <summary>
	/// key = StatusData, value = Status Time
	public Dictionary<StatusLargeData, int> statusTime;
	/// <summary>
	/// First Key = SkillId, Key = abbilityKey Value = ChangeParam in First Value 
	public Dictionary<int, Dictionary<string,int>> abiChange;
	public int initCD;
	public int initAttri;
	public bool[] hasStatus;
    public int minus;
    public int[] condition;
    public int[] act;
}

public struct AccordingData{
	public int index;
	public float attriRatio;
	public float jobRatio;
}