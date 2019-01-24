using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
using System.Linq;
using UnityEngine.Profiling;

public class FightUIController : MonoBehaviour {
	[SerializeField]
	bool isPortrait = false;

	[SerializeField]
	GroundRaycastController groundPool;

	[SerializeField]
	Transform CharaGroup;

	[SerializeField]
	Transform rayGroup;

	[SerializeField]
	Transform imagePool;

	[SerializeField]
	FightController fightController;

	[SerializeField]
	SkillController skillController;

    [SerializeField]
    StartShowController startShowController;

	GroundController[] allGcs;

	GroundController startGc, endGc;

	[SerializeField]
	Button checkButton;

    [SerializeField]
    NumberSetting energyNum;

	bool isResetGround = false;

	private int[] monsterCdTimes = new int[5];

	int CreateGround, ResetCount;

	List<CharaImageData> _charaGroup = new List<CharaImageData> ();

	Stack<Image> _imagePool = new Stack<Image> ();

	[SerializeField]
	Image spriteImage;

    public EnergyStone[] energyStone;

	int? charaIdx;

    [SerializeField]
    FilledBarController uniteHpBar;

	Image startCharaImage, endCharaImage;

	LinkedList<GroundController> charaGc;

	int resetGroundCount;

	public Sprite[] CharaSprite;

	int[] recJobRatios = new int[5];

	int[] preJobRatios;

	int[] jobRatios;

	Dictionary<int, Dictionary<StatusLargeData,int>> playerStatus;
	Dictionary<int, Dictionary<StatusLargeData,int>> enemyStatus;

	private bool spaceCorrect;

	//bool hasDamage;

	List<RaycastData> recAllRatioData, allRatioData, testAllRatioData;


	List<GroundController[]> raycasted;

	int[] ratioCount = new int[5];

	List<ExtraRatioData> ExtraRatios;

	List<GroundSEController> completeSe;

	int[] recActLevel = new int[5];

	public FightItemButton[] playerButton;
	Vector3[] playerButtonPos;
	public FightItemButton[] enemyButton;
	Vector3[] enemyButtonPos;

	private int energe;

	private int lockEnerge;

	private int spaceCount = 0;

	int unCompleteCount;

	int unShowed;

	int lockCount;

	List<int> canAttack;

	List<ExtraRatioData> extraedGc;

	int spCount;

	bool canCover;

	bool startCover, endCover;

	List<RaycastData> newRaycastData;

	#region GroundShow 格子轉換效果
	[SerializeField]
	GroundSEController showGrounds;

	[SerializeField]
	Transform showGroup;

    int centerIdx;

	Queue<GroundSEController> SEPool = new Queue<GroundSEController>();
	Queue<GroundSEController> SEingPool = new Queue<GroundSEController>();
	#endregion

	#region CharaButton 角色選去按鈕用
	bool onPress = false;
	private float lastIsDownTime;
	private float delay = 1f;
	bool charaDetail = false;
	#endregion

	#region InstantItemCount 遊玩中產生物件宣告
	int showItemCount = 16;
	int imageItem = 32;
	#endregion

	int[] protectJob = new int[5];
	bool hasProtect;

    bool fightInit = false;

	void SetData() {
		monsterCdTimes = new int[5]{7,5,1,10,6};
		fightController.SetCDTime (monsterCdTimes, false);
		fightController.SetData ();
		playerStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();
		enemyStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();

        startShowController.callback = StartShowEnd;
        fightInit = true;

        centerIdx = 30;
        groundPool.SetCenter(groundPool.transform.GetChild(centerIdx).GetComponent<GroundController>());

        foreach (FightItemButton btn in playerButton) {
			btn.SetHpBar (1, false);
		}

		foreach (FightItemButton btn in enemyButton) {
			btn.SetHpBar (1, false);
		}

		playerButtonPos = new Vector3[playerButton.Length];
		for (int i = 0; i<playerButton.Length;i++) {
			playerButtonPos [i] = playerButton [i].transform.localPosition + playerButton [i].transform.parent.localPosition;
		}

		enemyButtonPos = new Vector3[enemyButton.Length];
		for (int i = 0; i<enemyButton.Length;i++) {
			enemyButtonPos [i] = enemyButton [i].transform.localPosition + enemyButton [i].transform.parent.localPosition;
		}

		lockOrder = new LinkedList<int> ();

		groundPool.SetController ();
	}

	// Use this for initialization
	void Start () {
        WndStart();
    }

    private void WndStart() {
        lockOrder = new LinkedList<int>();

        for (int i = 0; i < imageItem; i++)
        {
            Image _image = Instantiate(spriteImage) as Image;
            _image.GetComponent<RectTransform>().SetParent(imagePool);
            _image.transform.localPosition = Vector3.zero;
            _image.gameObject.SetActive(false);
            _imagePool.Push(_image);
        }

        for (int i = 0; i < showItemCount; i++)
        {
            GroundSEController showItem = Instantiate(showGrounds) as GroundSEController;
            showItem.GetComponent<RectTransform>().SetParent(showGroup);
            showItem.transform.localPosition = Vector3.zero;
            showItem.transform.localScale = Vector3.one;
            showItem.gameObject.SetActive(false);
            SEPool.Enqueue(showItem);
        }

        allGcs = groundPool.GetComponentsInChildren<GroundController>();

        resetGroundCount = 0;

        foreach (GroundController gc in allGcs)
        {
            if ((int)gc._groundType != 99)
            {
                gc.plusRatio = OnPlusRatio;
            }
        }

        CreateGround = 3;

        lockCount = 0;


        SetData();

        energe = 3;
        SetEnergy(true);

        SetCenter();
        startShowController.ShowStart(groundPool.transform.GetChild(centerIdx).position);
    }

    private void StartShowEnd() {
        foreach(GroundController gc in allGcs) {
            gc.CloseLight();
        }
        ResetGround();
    }

    private void SetCenter() {
        foreach (GroundController gc in allGcs)
        {
            gc.transform.tag = "raycastG";
        }
        groundPool.transform.GetChild(centerIdx).tag = "Center";
        groundPool.transform.GetChild(centerIdx).GetComponent<GroundController>().SetTag();
    }

    #region ShowRecycle
    private void RecycleReverseItem(GroundSEController gse) {
		unShowed--;
		SEPool.Enqueue (gse);
		completeSe.Add (gse);
		gse.onRecycle = null;
		if (unShowed == 0) {
			foreach (GroundSEController se in completeSe) {
				se.gameObject.SetActive (false);
				se.AllReset ();
				se.CloseSE ();
			}
			foreach (FightItemButton btn in playerButton) {
				btn.NumberShowRun ();
			}
		}
	}

	private void RecycleShowUp(){
		unCompleteCount--;
		if (unCompleteCount == 0) {
			recAllRatioData = allRatioData;
			if (ExtraRatios.Count > 0) {
				StartCoroutine (OnShowExtra ());
			} else {
				OnFight ();
			}
		}
	}

	private void RecycleExtraItem(GroundSEController gse) {
		unShowed--;
		gse.gameObject.SetActive (false);
		SEPool.Enqueue (gse);
		gse.onRecycle = null;

		if (unShowed == 0) {
			gse.onExtraUp = null;
			OnFight ();
		}
	}


	private void OnPlusRatio(ExtraRatioData plusDamage) {
		ExtraRatios.Add (plusDamage);
	}

	private IEnumerator OnShowExtra(){
		foreach (var data in ExtraRatios) {
			List<GroundController> org = new List<GroundController> ();
			org.Add (data.gc);
			recJobRatios [data.extraJob] += data.upRatio;
			for (int i = 0; i < recAllRatioData.Count; i++) {
				if (recAllRatioData [i].start.name == data.linkData.ElementAt (0).Key.name && recAllRatioData [i].end.name == data.linkData.ElementAt (0).Value.name) {
					RaycastData raycastdata = new RaycastData();
					raycastdata = recAllRatioData [i];
					raycastdata.ratio += data.upRatio;
					recAllRatioData [i] = raycastdata;
				}
			}
			for (int i = 0; i < fightController.players.Length; i++) {
				if (fightController.GetJob ("P", i) == data.extraJob) {
					GroundSEController gse = SEPool.Dequeue ();
					gse.SetExtraSE (org, playerButtonPos [i], i, data.upRatio);
					gse.onRecycle = RecycleExtraItem;
					gse.onExtraUp = ExtraRatioUp;
					SEingPool.Enqueue (gse);
					AddCanAttack (i);
				}
			}
		}

		unShowed = SEingPool.Count;
		while (SEingPool.Count > 0) {
			GroundSEController gse = SEingPool.Dequeue ();
			gse.gameObject.SetActive (true);
			gse.Run ();
			yield return new WaitForSeconds (0.2f);
		}
	}

	private void ExtraRatioUp(GroundSEController gse, int idx, int upRatio){
		gse.gameObject.SetActive (false);
		playerButton [idx].SetExtra (upRatio);
	}

	private void OnFight(){
		CheckActLevel ();
		fightController.SetProtect (protectJob);
		fightController.FightStart (lockCount != 0, canAttack);
	}

	public void OnShowFight(List<DamageData> allDamage){
		StartCoroutine (OnShowAllFight (allDamage));
	}

	private IEnumerator OnShowAllFight(List<DamageData> allDamage){
		for (int i = 0; i < allDamage.Count; i++) {
			FightItemButton target = null;
			Vector3 orgPos = allDamage[i].tType[0] == "P" ? playerButtonPos [allDamage [i].orgIdx] : enemyButtonPos [allDamage [i].orgIdx];

			//當TargetIdx重複時會加10，此時需要減去10
			int minusCount = allDamage [i].targetIdx >= 10 ? 10 : 0;

			target = allDamage[i].tType[1] == "P" ? playerButton [allDamage [i].targetIdx - minusCount] : enemyButton [allDamage [i].targetIdx - minusCount];
			Vector3 targetPos = allDamage[i].tType[1] == "P" ? uniteHpBar.transform.localPosition : enemyButtonPos [allDamage [i].targetIdx - minusCount];

			GroundSEController gse = SEPool.Dequeue ();
			gse.SetAttackShow (orgPos, targetPos, target, allDamage [i]);
			gse.onRecycleDamage = ShowFightEnd;
			gse.gameObject.SetActive (true);
			gse.Run ();

			if (i == 0) {
				if (allDamage.Count == 1) {
					yield return new WaitForSeconds (0.5f);//該名角色最後攻擊所需時間
				}
				yield return new WaitForSeconds (0.2f);//該名角色的物理及魔法攻擊的顯示間隔
			} 
			else {
				yield return new WaitForSeconds (0.5f);//該名角色最後攻擊所需時間
			}
		}

		fightController.OnTriggerSkill (allDamage);
	}


	private void ShowFightEnd(GroundSEController gse, DamageData damageData, FightItemButton target, Vector3 tPos){
		gse.gameObject.SetActive (false);
        if (damageData.tType[1] == "P")
        {
            uniteHpBar.SetBar(damageData.hpRatio, true, false);
        }
        else
        {
            target.SetHpBar(damageData.hpRatio);
        }
		SEPool.Enqueue (gse);
		gse.onRecycle = null;
		gse.SetDamageShow (damageData, tPos);
		gse.onRecycle = ShowDamageEnd;
		gse.gameObject.SetActive (true);
		gse.Run ();
	}


	private void ShowDamageEnd(GroundSEController gse){
		gse.gameObject.SetActive (false);
		SEPool.Enqueue (gse);
		gse.CloseSE ();
		gse.onRecycle = null;
	}
	#endregion

	private void AddCanAttack(int idx){
		if (!canAttack.Contains (idx)) {
			canAttack.Add (idx);
		}
	}

	private void OnProtection(int targetJob){
		if (protectJob [targetJob] < 30 && hasProtect) {
			protectJob [targetJob] += 5; 
		}
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.R)) {
            fightInit = true;
            startShowController.ShowStart(groundPool.transform.GetChild(centerIdx).position);
		}

		if (Input.GetKeyDown(KeyCode.C)) {
			canCover = !canCover;
		}

		if (Input.GetKeyDown(KeyCode.H)) {
			bool isHas = false;
			foreach (var v in allGcs) {
				if (v.linkData.Count != 0) {
					Debug.Log (v.name);
					isHas = true;
				}
			}
			if (!isHas) {
				Debug.Log ("Clear");
			}
		}

        if (Input.GetKeyDown(KeyCode.J))
        {
            startShowController.ShowCount();
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
			fightController.ShowData ();
		}
		if (Input.GetKey(KeyCode.K)) {
			fightController.ShowSoulDataC ();
		}

		if (Input.GetKeyDown(KeyCode.T)) {
			fightController.TestFunction ();
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			//fightController.ShowSkillData ();
		}


		if (Input.GetKeyDown(KeyCode.A)) {
			if (CheckStatus(FightStatus.RoundStart) && spaceCount > 0) {
				CheckOut ();
			}
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			ChangeStatus (FightStatus.SelSkillTarget);
		}

		if (CheckStatus(FightStatus.RoundStart)) {
			if (Input.GetKeyDown (KeyCode.Mouse0)) {
				TouchDown (false);
			}

			if (Input.GetKey (KeyCode.Mouse0)) {
				TouchDrap (false);
			}

			if (Input.GetKeyUp (KeyCode.Mouse0)) {
				TouchUp (false);
			}

			if (Input.touchCount == 1) {
				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					TouchDown (true);
				}

				if (Input.GetTouch (0).phase == TouchPhase.Stationary) {
					TouchDrap (true);
				}

				if (Input.GetTouch (0).phase == TouchPhase.Ended) {
					TouchUp (true);
				}
			}
		}

		if (onPress) {
			// 當前時間 -  最後按鈕按下時間 > 延遲1秒
			if (Time.time - lastIsDownTime > delay && !charaDetail) {
				SetCharaDetail ((int)charaIdx);
				charaIdx = null;
				charaDetail = true;
			}
		}
	}

	public void OnPressDown(int selIdx){
		onPress = true;
		SelectChara (selIdx);
		lastIsDownTime = Time.time;

	}


	public void OnPressUp(){
		charaDetail = false;
		onPress = false;
	}

	public void OnPressExit(){
		charaDetail = false;
		if (onPress) {
			charaIdx = null;
		}
		onPress = false;
	}

	void SetCharaDetail(int selIdx){

	}

	public void FightEnd(){
		UpEnerge ();
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType != 99) {
				gc.FightEnd ();
			}
		}

		fightController.FightEnd ();
		ChangeStatus (FightStatus.FightEnd);
		if (isResetGround) {
			ResetGround ();
		} else {
			NextRound ();
		}
	}


	void RoundStart(bool isCenter = true){
		ChangeStatus (FightStatus.RoundPrepare);
		groundPool.SetCreateGround (CreateGround + (int)Mathf.Ceil (resetGroundCount / 2));
		groundPool.RoundStart (isCenter);
		CheckLockStatus ();

		ChangeStatus (FightStatus.RoundStart);

        ResetTemple();

        checkButton.interactable = false;

	}

	private void CheckLockStatus (){
		foreach (KeyValuePair<int,Dictionary<StatusLargeData,int>> kv in playerStatus) {
			foreach (KeyValuePair<StatusLargeData,int> kv2 in kv.Value) {
				if (kv2.Key.charaStatus == (int)Nerf.UnTake) {
					playerButton [kv.Key].SetEnable (false);
				}
			}
		}
	}

	/// <summary>
	/// 進行下一回合擺放前抽選產生Ground
	/// </summary>
	/// <param name="isSpace">是否擺放角色</param>
	private void NextRound(bool isSpace = true){
		foreach (var v in playerButton) {
			v.SetTextColor (Color.black);
		}

		groundPool.ChangeLayer ();

		if (!groundPool.NextRound ()) {
			ResetGround ();
		};

		OnOpenButton ();

		newRaycastData = new List<RaycastData> ();
		CheckLockStatus ();

        ResetTemple();

        ChangeStatus (FightStatus.RoundStart);
	}

	private void RoundEnd(bool hasDamage)
	{
		spCount++;

		completeSe = new List<GroundSEController> ();
		canAttack = new List<int> ();

		ChangeStatus (FightStatus.FightStart);

		OnCloseButton (TargetType.All.ToString());

		ResetDamage (false);

		for (int i = 0; i < jobRatios.Length; i++) {
			if (jobRatios [i] != recJobRatios [i]) {
				for (int j = 0; j < fightController.players.Length; j++) {
					if (fightController.GetJob("P", j) == i) {
						AddCanAttack (j);
						playerButton [j].SetRatioTxt (jobRatios [i], true);
						playerButton[j].onComplete = RecycleShowUp;
						unCompleteCount++;
					}
				}
			}
		}
			
		recJobRatios = jobRatios;

        MonsterCdDown();


		if (hasDamage) {
			StartCoroutine (CheckRatio ());
		} 
		else {
			fightController.EnemyFight ();
		}

		groundPool.RoundEnd ();

		hasDamage = false;
		spaceCount = 0;
		charaIdx = null;
		startCharaImage = null;
		endCharaImage = null;
		startGc = null;
		endGc = null;
		ratioCount = new int[5] { 0, 0, 0, 0, 0 };

		recAllRatioData = allRatioData;
	}

	private void UpEnerge(){
		if (energe < 6) {
            energe += 2;
			if (energe > 6) {
				energe = 6;
			}
            SetEnergy();
		}
	}

	private void TouchDown(bool isTouch = false){
		var result = CanvasManager.Instance.GetRaycastResult (isTouch);

		if (result.Count > 0) {
			foreach (var r in result) {
                if (r.gameObject.CompareTag("raycastGCorner") || r.gameObject.CompareTag("Center") || r.gameObject.CompareTag("raycastG"))
                {
                    if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 0
						|| (int)r.gameObject.GetComponent<GroundController> ()._groundType == 99
						|| ((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover)) {
						TouchDown (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover));
					}
				}
			}
		}
	}

	private void TouchDown(GroundController gc, bool isCover){
		startGc = gc;
		if (charaIdx != null) {
			if (fightController.GetJob("P", (int)charaIdx) == 2) {
				startGc.onProtection = OnProtection;
			}

			charaGc.AddLast (startGc);
			if (isCover) {
				startCover = true;
				startGc.OnCover ();
			}
			startGc.ChangeChara (fightController.GetJob("P", (int)charaIdx));

			startCharaImage = SetChess(startGc);
			endCharaImage = SetChess(startGc);

			if ((int)gc.GetComponent<GroundController> ()._groundType == 99) {
				isResetGround = true;
			}
		} 
	}

	private void TouchDrap(bool isTouch = false){
		if (endCharaImage != null) {
			var result = CanvasManager.Instance.GetRaycastResult (isTouch);
			if (result.Count > 0) {
				foreach (var r in result) {
					if (r.gameObject.CompareTag("raycastGCorner") || r.gameObject.CompareTag("Center") || r.gameObject.CompareTag("raycastG"))
					{
                        endCharaImage = SetChess(r.gameObject.GetComponent<GroundController>(), endCharaImage);

						if (endGc != r.gameObject.GetComponent<GroundController> ()) {
							foreach (GroundController gc in charaGc) {
								gc.OnPrevType (false);
							}

							if (endGc != null) {
								if (charaGc.Last.Value == endGc) {
									if (endCover) {
										endGc.OnPrevCover ();
										endCover = false;
									} else {
										endGc.ResetType ();
									}
									charaGc.RemoveLast ();
								} 
							}

							if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 0 
								|| (int)r.gameObject.GetComponent<GroundController>()._groundType == 99
								|| ((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover))
							{
								TouchDrap (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover));
							}
							else
							{
								TouchError ();
							}
						}
					}
				}
			}
		}
	}

	private void TouchDrap(GroundController gc, bool isCover){
		GroundController checkGc = gc;

		Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, checkGc.transform.localPosition);

		if (IsCorrectDir(dir))
		{
			endGc = gc;

			if (isCover) {
				endCover = true;
				endGc.OnCover ();
			}
			endGc.ChangeChara (fightController.GetJob("P", (int)charaIdx));

			if (fightController.GetJob("P", (int)charaIdx) == 2) {
				endGc.onProtection = OnProtection;
			}

			charaGc.AddLast (endGc);

			CheckGround(false);
			spaceCorrect = true;
		}
		else
		{
			TouchError ();
		}
	}

	private void TouchError(){
		endGc = null;
		ResetDamage (spaceCount > 0);
		spaceCorrect = false;
	}

	private void TouchUp(bool isTouch = false){
		if (endCharaImage != null) {
			var result = CanvasManager.Instance.GetRaycastResult (isTouch);
			if (result.Count > 0) {
				foreach (var r in result) {
                    if (r.gameObject.CompareTag("raycastGCorner") || r.gameObject.CompareTag("Center") || r.gameObject.CompareTag("raycastG"))
                    {
                        if (spaceCorrect == true) {
							bool onlyAdd = false;

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}


							energe = energe - (spaceCount + 1);
                            SetEnergy();

							if (energe > (spaceCount + 1) && !isResetGround) {
								ResetStatus ();
								onlyAdd = true;
							}

							spaceCount++;

							checkButton.interactable = true;

							canCover = false;

                            fightController.SetJob((int)charaIdx);

							if (onlyAdd && !isResetGround) {
								preJobRatios = jobRatios;
								CheckGround (true);

								charaIdx = null;

                                ResetTemple();

								return;
							}
                            ResetTemple();
                            RoundEnd (CheckGround (false, true));
						} else {
							SetChess ();
                            SetChess();
							if (startCover) {
								startGc.OnPrevCover ();
								startCover = false;
							} else {
								startGc.ResetType ();
							}
							charaGc.RemoveLast ();
							isResetGround = false;
							ResetStatus ();
							ResetDamage (spaceCount > 0);
						}
					}
				}
			}
		}
	}

	public void CheckOut(){
		RoundEnd(CheckGround (false, true));
	}

	private bool CheckGround(bool isTouchUp, bool isRoundEnd = false) {
        ResetTemple();

		allRatioData = new List<RaycastData> ();
		ExtraRatios = new List<ExtraRatioData>();
		protectJob = new int[5]{ 0, 0, 0, 0, 0 };
		bool hasDamage = false;

		foreach (GroundController gc in charaGc) {
			gc.raycasted = false;
		}
		if (isRoundEnd) {
			foreach (GroundController gc in charaGc) {
				gc.OnPrevType (true);
			}

			if (endGc != null) {
				endGc.ChangeChara (fightController.GetJob("P", (int)charaIdx));
			}
		}


		foreach (GroundController gc in charaGc) {
			ResponseData(gc.OnChangeType(isTouchUp, isRoundEnd));
		}

		if (allRatioData.Count != recAllRatioData.Count) {
			if (isTouchUp || (!isTouchUp && isRoundEnd)) {
				CheckHitCount ();
			}
			hasDamage = true;
		}

		ChangeCharaRatio ();
		return hasDamage;
	}

	private void CheckHitCount(){
		List<RaycastData> newRData = new List<RaycastData> ();
		foreach (RaycastData newData in allRatioData) {
			bool isNew = true;
			foreach (RaycastData oldData in recAllRatioData) {
				if (newData.start.name == oldData.start.name && newData.end.name == oldData.end.name) {
					isNew = false;
				}
			}

			if (isNew) {
				bool isNewR = true;
				foreach (RaycastData oldData in newRaycastData) {
					
					if (newData.start.name == oldData.start.name && newData.end.name == oldData.end.name) {
						isNewR = false;
					}
				}

				if (isNewR) {
					newRaycastData.Add (newData);
					newRData.Add (newData);
				}
			}
		}

		fightController.OnHitCountStatus (newRData);
	}

	private void ResponseData(List<RaycastData> raycastData){
		foreach (var data in raycastData) {
			allRatioData.Add (data);
		}
	}

	private void ChangeCharaRatio()
	{
		jobRatios = new int[5]{ 0, 0, 0, 0, 0 };
		foreach (var data in allRatioData) {
			jobRatios [data.CharaJob] += data.ratio;
		}

		for (int i = 0; i < jobRatios.Length; i++) {
			for (int j = 0; j < fightController.players.Length; j++) {
				if (fightController.GetJob("P", j) == i) {
					playerButton [j].SetRatioTxt (jobRatios [i]);
					if (recJobRatios [fightController.GetJob("P", i)] != jobRatios [fightController.GetJob("P", i)]) {
						playerButton [i].SetTextColor (Color.red);
					} else {
						playerButton [i].SetTextColor (Color.black);
					}
				}
			}
		}
	}

	private void CheckActLevel()
	{
		foreach (var data in recAllRatioData) {
			if (data.hits.Count >= 4 && data.ratio > 250) {
				ChangeActLevel (data.CharaJob, 3);
			}
			else if (data.hits.Count >= 2 && data.ratio >= 150) {
				ChangeActLevel (data.CharaJob, 2);
			} else {
				ChangeActLevel (data.CharaJob, 1);
			}
		}
	}

	private void ChangeActLevel(int job, int level){
		if (recActLevel [job] < level) {
			recActLevel [job] = level;
		}
	}

	public void ChangeHpBar(int idx, string targetString, float hpRatio, bool isUp){
		if (targetString == "P") {
			playerButton [idx].SetHpBar (hpRatio, true, isUp);
		} 
		else {
			enemyButton [idx].SetHpBar (hpRatio, true, isUp);
		}
	}

    public void ChangeUniteHpBar(float hpRatio, bool isUp)
    {
        uniteHpBar.SetBar(hpRatio, true, isUp);
    }

    #region Skill
    public void OnRecovery(int idx, string targetString, float hpRatio){
		ChangeHpBar (idx, targetString, hpRatio, true);
	}

	public void OnRmAlarm(int cdtime, int idx){
		monsterCdTimes[idx] = cdtime;
	}

	public void OnCover(){
		canCover = true;
	}
	#endregion

	private void MonsterCdDown(){
		for (int i = 0; i < monsterCdTimes.Length; i++) {
			if (monsterCdTimes [i] > 0) {
				monsterCdTimes [i]--;
			}
		}

		fightController.SetCDTime (monsterCdTimes);
	}

	private IEnumerator CheckRatio() {
		if (allRatioData.Count > recAllRatioData.Count) {
			foreach (var data in allRatioData)
			{
				bool hasNew = true;

				if (recAllRatioData.Count > 0)
				{
					foreach (var recData in recAllRatioData)
					{
						if (data.start == recData.start && data.end == recData.end)
						{
							hasNew = false;
							break;
						}
					}
				}

				if (hasNew == true)
				{
					//Debug.LogWarning (data.ratio);
					List<Vector3> postions = new List<Vector3> ();
					for (int i = 0; i < fightController.players.Length; i++) {
						if (fightController.GetJob("P", i) == data.CharaJob) {
							postions.Add (playerButtonPos [i] + (Vector3.up * 30) * (ratioCount [i] - 1));

							ratioCount [i]++;
						}
					}
					if (SEPool.Count<=0) {
						CreateSE ();
					}
					GroundSEController gse = SEPool.Dequeue ();

					gse.SetReverseSE (data.hits, postions);
					gse.onRecycle = RecycleReverseItem;
					SEingPool.Enqueue (gse);
				}
			}
		}

		unShowed = SEingPool.Count;
		while (SEingPool.Count > 0) {
			GroundSEController gse = SEingPool.Dequeue();
			gse.gameObject.SetActive (true);
			gse.Run ();
			yield return new WaitForSeconds (0.5f*(gse.seGrounds.Count-1));
		}
	}

	private void ResetDamage(bool isPrev) {
		for (int i = 0; i < fightController.players.Length; i++) {
			if (!isPrev) {
				playerButton [i].SetRatioTxt (recJobRatios [fightController.GetJob("P", i)]);
			} else {
				playerButton [i].SetRatioTxt (preJobRatios [fightController.GetJob("P", i)]);
			}
			playerButton [i].SetTextColor (Color.black);
		}
	}

	public bool IsCorrectDir(Vector2 dirNormalized) {
		if (isPortrait) {
			if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 9 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 5) {
				return true;
			} else if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 0 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 10) {
				return true;
			}
		} else {
			if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 5 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 9) {
				return true;
			} else if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 10 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 0) {
				return true;
			}
		}
		return false;
	}


	public Vector2 ConvertDirNormalized(Vector2 org, Vector2 dir){

		return (dir - org).normalized;
	}

	private void ResetGround(){
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
		}

		for (int i = 0; i < 5; i++) {
			recJobRatios [i] = 0;
			ratioCount [i] = 0;
			recActLevel [i] = 0;
		}

		for (int i = 0; i < 5; i++) {
			playerButton [i].ResetRatio ();
		}

		allRatioData = new List<RaycastData> ();
		recAllRatioData = new List<RaycastData> ();

		charaGc = new LinkedList<GroundController> ();
		extraedGc = new List<ExtraRatioData> ();

		newRaycastData = new List<RaycastData> ();


		charaIdx = null;

		isResetGround = false;
		spaceCorrect = false;
		spaceCount = 0;

		ResetStatus ();


		while (_charaGroup.Count > 0) {
			SetChess ();
		}

		if (!fightInit) {
			resetGroundCount++;
			fightController.SetResetRatio (Mathf.CeilToInt(resetGroundCount/2));
		}
        else {
            fightInit = false;
        }

        OnOpenButton ();

		RoundStart ();
	}

	private void ResetStatus(){
		startCharaImage = null;
		endCharaImage = null;
		startGc = null;
		endGc = null;
	}

	public void GetHasProtect(bool isHas){
		hasProtect = isHas;
		if (!hasProtect) {
			protectJob = new int[5]{ 0, 0, 0, 0, 0 };
		}
	}

    private Image SetChess(GroundController linkGc = null, Image img = null) {
        Image image = img;
        if (linkGc != null)
        {
            if (image == null)
            {
                image = _imagePool.Pop();

                image.GetComponent<RectTransform>().SetParent(CharaGroup.transform.GetChild(linkGc.groundRow));
                image.sprite = CharaSprite[(int)charaIdx];

                image.transform.localPosition = linkGc.transform.localPosition.x * Vector3.right;

                CharaImageData imageData = new CharaImageData();
                image.transform.localScale = Vector3.one;
                image.SetNativeSize();

                image.gameObject.SetActive(true);
                imageData.image = image;
                _charaGroup.Add(imageData);

                return image;
            }
            else
            {
                image.GetComponent<RectTransform>().SetParent(CharaGroup.transform.GetChild(linkGc.groundRow));

                image.transform.localPosition = linkGc.transform.localPosition.x * Vector3.right;
                return image;
            }
        }
        else
        {
            if (_charaGroup.Count > 0)
            {
                image = _charaGroup[_charaGroup.Count - 1].image;
                _charaGroup.RemoveRange(_charaGroup.Count - 1, 1);
                image.GetComponent<RectTransform>().SetParent(imagePool);
                image.transform.localPosition = Vector3.zero;
                image.gameObject.SetActive(false);
                image.sprite = null;
                _imagePool.Push(image);
            }
            return null;
        }
    }


    private Image PopImage(Stack<Image> pool = null, GroundController linkGc = null, Vector3? position = null){
		Image image;
		if (pool != null) {
			image = pool.Pop ();
			image.GetComponent<RectTransform> ().SetParent (CharaGroup);
			image.sprite = CharaSprite [(int)charaIdx];

			if (position != null) {
				image.transform.localPosition = (Vector3)position;

			}
			CharaImageData imageData = new CharaImageData ();
			image.transform.localScale = Vector3.one;
			image.SetNativeSize ();

			image.gameObject.SetActive (true);
			imageData.image = image;
			_charaGroup.Add (imageData);
			return image;
		} else {
			if (_charaGroup.Count > 0) {
				image = _charaGroup [_charaGroup.Count - 1].image;
				_charaGroup.RemoveRange (_charaGroup.Count - 1, 1);
				image.GetComponent<RectTransform> ().SetParent (imagePool);
				image.transform.localPosition = Vector3.zero;
				image.gameObject.SetActive (false);
				image.sprite = null;
				_imagePool.Push (image);
			}
			return null;
		}
	}

	public void SelectChara (int idx){
		if (CheckStatus (FightStatus.RoundStart)) {
			charaIdx = idx;
		} 
		else if (CheckStatus (FightStatus.SelSkillTarget)) {
			fightController.SelectSkillTarget ("P", idx);
		}
	}

	LinkedList<int> lockOrder;

	public void LockEnemy (int idx){
		if (CheckStatus (FightStatus.RoundStart)) {
			fightController.LockOrder (idx);
		} 
		else if (CheckStatus (FightStatus.SelSkillTarget)) {
			fightController.SelectSkillTarget ("E", idx);
		}
	}


	public void SetLockUI(LinkedList<int> order){
		if (order.Count == 0) {
			for (int i = 0; i < enemyButton.Length; i++) {
				enemyButton [i].transform.GetChild (0).GetComponent<Text> ().text = string.Empty;
			}
		} 
		else {
			for (int i = 0; i < order.Count; i++) {
				enemyButton [order.ElementAt (i)].transform.GetChild (0).GetComponent<Text> ().text = (i + 1).ToString ();
			}
		}
	}

	public void SetUnLockUI(int idx){
		enemyButton [idx].transform.GetChild (0).GetComponent<Text> ().text = string.Empty;
	}

	private void CreateSE(){
		showItemCount += 16;
		for (int i = 0; i < 16; i++)
		{
			GroundSEController showItem = Instantiate(showGrounds) as GroundSEController;
			showItem.GetComponent<RectTransform>().SetParent(showGroup);
			showItem.transform.localPosition = Vector3.zero;
			showItem.gameObject.SetActive (false);
			SEPool.Enqueue(showItem);
		}
	}

	private void UseSkill(int idx){
		skillController.UseSkill (idx);
	}

	public void OnSkillCDEnd(int charaIdx){
		Debug.LogWarning ("Chara " + charaIdx + " Can Use Skill");
	}

    void SetEnergy(bool init = false)
    {
        energyNum.SetNumber(energe);
        for (int i = 0; i < energyStone.Length; i++)
        {
            if (i < energe)
            {
                energyStone[i].EnergyCharge(init);
            }
            else
            {
                energyStone[i].EneryEmpty(init);
            }
        }
    }


    #region ButtonControl
    public void OnSelectionDir(List<int> idxList, string targetString){
		OnCloseButton (TargetType.All.ToString());

		foreach (int idx in idxList) {
            if (targetString == "P"){
                playerButton[idx].SetEnable (true);
			} 
			else {
				enemyButton [idx].SetEnable (true);
			}
		}
	}

	private void OnCloseButton(string targetString) {
		if (targetString == "P") {
			foreach (FightItemButton btn in playerButton) {
				btn.SetEnable (false);
			}
		} else if (targetString == "E") {
			foreach (FightItemButton btn in enemyButton) {
				btn.SetEnable (false);
			}
		} 
		else {
			foreach (FightItemButton btn in playerButton) {
				btn.SetEnable (false);
			}
			foreach (FightItemButton btn in enemyButton) {
				btn.SetEnable (false);
			}
		}
	}

	private void OnOpenButton(){
		foreach (FightItemButton btn in enemyButton) {
			btn.SetEnable (true);
		}
		foreach (FightItemButton btn in playerButton) {
			btn.SetEnable (true);
		}
	}

	public void OnDead(int idx, string[] tType){
		if (tType[1] == "P") {
			playerStatus.Remove (idx);
			playerButton [idx].SetEnable (false, true);

		} 
		else {
			enemyStatus.Remove (idx);
			enemyButton [idx].SetEnable (false, true);
		}
	}

	private void SetButton(){
		foreach (FightItemButton btn in enemyButton) {
			btn.Init ();
		}
		foreach (FightItemButton btn in playerButton) {
			btn.Init ();
		}
	}
    #endregion

    #region StatusControl
    public void OnStatus(int idx, StatusLargeData data, int level, string targeString){
		if (targeString == "P") {
			if (playerStatus.ContainsKey (idx)) {
				if (!playerStatus [idx].ContainsKey (data)) {
					playerStatus [idx].Add (data, level);
				} 
				else {
					playerStatus [idx] [data] = level;
				}
			} else {
				Dictionary<StatusLargeData,int> sData = new Dictionary<StatusLargeData, int> ();
				sData.Add (data, level);
				playerStatus.Add (idx, sData);
			}
		}
		else {
			if (enemyStatus.ContainsKey (idx)) {
				if (!enemyStatus [idx].ContainsKey (data)) {
					enemyStatus [idx].Add (data, level);
				} 
				else {
					enemyStatus [idx] [data] = level;
				}
			} else {
				Dictionary<StatusLargeData,int> sData = new Dictionary<StatusLargeData, int> ();
				sData.Add (data, level);
				enemyStatus.Add (idx, sData);
			}
		}
	}

	public void OnStatusDown(int idx, StatusLargeData key, int time, string targeString){
        if (targeString == "P"){
            if (playerStatus.ContainsKey (idx)) {
				if (playerStatus [idx].ContainsKey (key)) {
					if (time == 0) {
						playerStatus [idx].Remove (key);
					} 
					else {
					
					}
				}
			}
		}
		else {
			if (enemyStatus.ContainsKey (idx)) {
				if (enemyStatus [idx].ContainsKey (key)) {
					if (time == 0) {
						enemyStatus [idx].Remove (key);
					} 
					else {

					}
				}
			}
		}
	}

    public void RmStatus(int idx, StatusLargeData key, string targetString){
        if (targetString == "P"){
            playerStatus[idx].Remove(key);
        }
        else{
            enemyStatus[idx].Remove(key);
        }
    }
    #endregion

    private void ResetTemple() { 
        foreach(GroundController gc in allGcs) {
            gc.ResetTemple(0);
        }
    }

    public void ChangeStatus(FightStatus status){
		fightController.fightStatus = status;
	}

	public bool CheckStatus(FightStatus status){
		return fightController.fightStatus == status;
	}

	public int GetCharaRatio(int job){
		return recJobRatios [job];
	}

	public int GetActLevel(int job){
		return recActLevel [job];
	}

	public bool GetEnerge(int need, bool isExpend = false) {
		return isExpend == true ? (energe - lockEnerge) > need : energe >= need;
    }

	public void AddEnerge(int erg){
		energe += erg;
        SetEnergy();
   	}

	public void LockEnerge(int erg){
		if (lockEnerge < erg) {
			lockEnerge = erg;
		}
	}

	public int GetJobGround(int job){
		int count = 0;
		foreach (GroundController gc in allGcs) {
			if (gc._groundType == GroundType.Chara && gc.charaJob == job) {
				count++;
			}
		}

		return count / 2;
	}

	public int GetLayerGround(int layer){
		int count = 0;
		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType >= layer && gc._groundType != GroundType.Chara && gc._groundType != GroundType.Caution) {
				count++;
			}
		}
		return count;
	}
}

public enum GroundType{
	None = 0,
	Copper = 1,
	Silver = 2,
	gold = 3,
	Chara = 10,
	Caution = 99,
}

public struct RaycastData {
	public GroundController start;
	public GroundController end;
	public List<GroundController> hits;
	public int CharaJob;
	public int ratio;
}

public struct ExtraRatioData {
	public int extraJob;
	public GroundController gc;
	public int upRatio;
	public Dictionary<GroundController,GroundController> linkData;
}

public struct CharaImageData{
	public Image image;
}

public enum SpecailEffectType{
	Reverse = 1,
	ExtraRatio = 2,
	Attack = 3,
	Damage = 4
}

