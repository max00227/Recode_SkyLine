using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
using System.Linq;
using UnityEngine.Profiling;
using TMPro;

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
    NumberSetting energyNum;

    [SerializeField]
    Button checkButton;

    [SerializeField]
    TweenColor healingIcon;

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

	int resetGroundCount;

	public Sprite[] CharaSprite;

    int usedEnergy;

    int[] conditionDown;
	Dictionary<int, Dictionary<StatusLargeData,int>> playerStatus;
	Dictionary<int, Dictionary<StatusLargeData,int>> enemyStatus;

	private bool spaceCorrect;

	//bool hasDamage;

	List<RaycastData> recAllRatioData, allRatioData, testAllRatioData;


	List<GroundController[]> raycasted;

	int[] ratioCount = new int[5];

	List<ExtraRatioData> ExtraRatios;

	List<FightSEController> completeSe;

	public FightItemButton[] playerButton;
	Vector3[] playerButtonPos;
	public FightItemButton[] enemyButton;
	Vector3[] enemyButtonPos;

    int dirIdx;

	private int energe;

	private int lockEnerge;

	private int spaceCount = 0;

	int unCompleteCount;

	int unShowed;

	int lockCount;

	List<ExtraRatioData> extraedGc;

	int spCount;

	bool canCover;

	bool startCover, endCover;

	List<RaycastData> newRaycastData;

    int round;

    int[] specialRatio;

	#region GroundShow 格子轉換效果
	[SerializeField]
	FightSEController showGrounds;

	[SerializeField]
	Transform showGroup;

    int centerIdx;

	Queue<FightSEController> SEPool = new Queue<FightSEController>();
	Queue<FightSEController> SEingPool = new Queue<FightSEController>();
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
		monsterCdTimes = new int[5]{7,5,10,3,6};
		fightController.SetCDTime (monsterCdTimes, false);
		fightController.SetData ();
		playerStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();
		enemyStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();

        startShowController.callback = StartShowEnd;
        fightInit = true;

        centerIdx = 30;
        groundPool.SetCenter(groundPool.transform.GetChild(centerIdx).GetComponent<GroundController>());

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

        specialRatio = new int[Enum.GetNames(typeof(SpecailGround)).Length - 1];

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
            FightSEController showItem = Instantiate(showGrounds) as FightSEController;
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
            gc.plusGroundType = OnPlusGroundType;
        }

        CreateGround = 3;

        lockCount = 0;


        SetData();

        conditionDown = new int[3];

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

    #region ShowView
    private void OnPlusGroundType(GroundController gc, bool useEnergy) {
        if (useEnergy) {
            if (gc._groundType == GroundType.Copper)
            {
                usedEnergy += 2;
            }
            else {
                usedEnergy += 1;
            }
        }

        SpecialGroundEffecnt(gc.specailType);

        conditionDown[(int)gc._groundType - 1]++;

        for (int i = 0; i < usedEnergy; i++) {
            if (energe - 1 - i >= 0)
            {
                energyStone[energe - 1 - i].UseEnergy();
            }
        }

        foreach (FightItemButton button in playerButton) {
            button.SetMinus(conditionDown);
        }
        if (usedEnergy > energe)
        {
            energyNum.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
        else {
            energyNum.GetComponent<TextMeshProUGUI>().color = Color.white;
        }
    }

    public void SpecialGroundEffecnt(SpecailGround specailGround) {
        if (specailGround != SpecailGround.None)
        {
            specialRatio[(int)specailGround - 1]++;
        }
    }


	public void OnFight(){
        conditionDown = new int[3];
        MonsterCdDown();
        RoundEnd();
		fightController.FightStart (lockCount != 0);
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

            for (int j = 0; j < allDamage[i].damage.Length; j++)
            {
                FightSEController gse = SEPool.Dequeue();
                gse.SetAttackShow(orgPos, targetPos, j, target, allDamage[i]);
                gse.onRecycleDamage = ShowFightEnd;
                gse.gameObject.SetActive(true);
                gse.Run();
                if (j == 0)
                {
                    if (allDamage[i].damage.Length == 1)
                    {
                        yield return new WaitForSeconds(0.3f);//該名角色最後攻擊所需時間
                    }
                    yield return new WaitForSeconds(0.1f);//該名角色的物理及魔法攻擊的顯示間隔
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);//該名角色最後攻擊所需時間
                }
            }

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



	private void ShowFightEnd(FightSEController gse, int damageIdx, DamageData damageData, FightItemButton target, Vector3 tPos){
		gse.gameObject.SetActive (false);
        if (damageData.tType[1] == "P")
        {
            SetUniteHp(damageData.hpRatio[damageIdx], false);
        }
        else
        {
            target.SetHpBar(damageData.hpRatio[damageIdx]);
        }
		SEPool.Enqueue (gse);
		gse.onRecycle = null;
        gse.SetDamageShow(damageIdx, damageData, tPos);
		gse.onRecycle = ShowDamageEnd;
		gse.gameObject.SetActive (true);
		gse.Run ();
	}


	private void ShowDamageEnd(FightSEController gse){
		gse.gameObject.SetActive (false);
		SEPool.Enqueue (gse);
		gse.CloseSE ();
		gse.onRecycle = null;
	}
	#endregion

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


/*            if (Input.touchCount == 1) {
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
*/
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

    public void SetUniteHp(float hpRatio, bool isUp) {
        uniteHpBar.SetBar(hpRatio, true, isUp);
        uniteHpBar.OnRun();
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
        fightController.ConditionDown(conditionDown);

		CheckLockStatus ();

		ChangeStatus (FightStatus.RoundStart);

        ResetTemple();
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
        conditionDown = new int[3]; 
        isResetGround = !groundPool.NextRound();

        if (isResetGround)
        {
            ResetGround();
        }
        else {
            if (round < 4)
            {
                round++;
            }
            energe = 4 + round;
            SetEnergy();
            fightController.ConditionDown(conditionDown);
        }

		OnOpenButton ();

		newRaycastData = new List<RaycastData> ();
		CheckLockStatus ();

        ResetTemple();

        ChangeStatus (FightStatus.RoundStart);
	}

	private void RoundEnd()
	{
		spCount++;

		completeSe = new List<FightSEController> ();

		ChangeStatus (FightStatus.FightStart);

		OnCloseButton (TargetType.All.ToString());

		//ResetDamage (false);
		groundPool.RoundEnd ();

        spaceCount = 0;
		charaIdx = null;
		startCharaImage = null;
		endCharaImage = null;
		startGc = null;
		endGc = null;
	}

	private void TouchDown(bool isTouch = false){
        Debug.Log("456456");
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
		if (isCover) {
			startCover = true;
			startGc.OnCover ();
		}

		startCharaImage = SetChess(startGc);
		endCharaImage = SetChess(startGc);

		if ((int)gc.GetComponent<GroundController> ()._groundType == 99) {
			isResetGround = true;
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
                            startGc.OnPrevType(dirIdx, endGc);

                            foreach (FightItemButton button in playerButton) {
                                button.CloseMinus();
                            }
                            foreach (EnergyStone stone in energyStone)
                            {
                                stone.CloseUseBg();
                            }

                            TouchDrap (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover));
						}
					}
				}
			}
		}
	}

	private void TouchDrap(GroundController gc, bool isCover){
        endGc = gc;
        usedEnergy = 0;
        conditionDown = new int[3];
        ResetSpecialRatio();
        healingIcon.gameObject.SetActive(false);
        Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, endGc.transform.localPosition);
        dirIdx = IsCorrectDir(dir);


        if (dirIdx != -99 && startGc != endGc)
		{

			if (isCover) {
				endCover = true;
				endGc.OnCover ();
			}

            startGc.ChangeChara();
            endGc.ChangeChara ();

            startGc.OnChangeType(false, dirIdx, endGc);

            spaceCorrect = true;

            if (specialRatio[(int)SpecailGround.Heal - 1] > 0)
            {
                healingIcon.PlayForward();
                healingIcon.gameObject.SetActive(true);
            }
        }
		else
		{
			TouchError ();
		}
	}

	private void TouchError(){
		endGc = null;
		//ResetDamage (spaceCount > 0);
		spaceCorrect = false;
	}

	private void TouchUp(bool isTouch = false){
        if ((startGc != null && startGc.defaultType == GroundType.Caution) || (endGc!=null && endGc.defaultType == GroundType.Caution))
        {
            isResetGround = true;
        }
        if (endCharaImage != null) {
			var result = CanvasManager.Instance.GetRaycastResult (isTouch);
			if (result.Count > 0) {
				foreach (var r in result) {
                    if (r.gameObject.CompareTag("raycastGCorner") || r.gameObject.CompareTag("Center") || r.gameObject.CompareTag("raycastG"))
                    {
                        if (((int)r.gameObject.GetComponent<GroundController>()._groundType <= 10 && (int)r.gameObject.GetComponent<GroundController>()._groundType > 0) ||
                            energe < usedEnergy)
                        {
                            spaceCorrect = false;
                        }
                        if (spaceCorrect == true)
                        {
                            energe = energe - usedEnergy;
                            SetEnergy();

                            fightController.ConditionDown(conditionDown, true);

                            spaceCount++;
                            canCover = false;

                            foreach (GroundController gc in allGcs) {
                                gc.SetType();
                            }

                            fightController.OnHealing(specialRatio[(int)SpecailGround.Heal - 1]);
                            ResetStatus();
                            ResetTemple();
                        }
                        else
                        {
                            SetChess();
                            SetChess();
                            if (startCover)
                            {
                                startGc.OnPrevCover();
                                startCover = false;
                            }
                            else
                            {
                                startGc.OnPrevType(dirIdx, endGc);
                                if (endGc != null)
                                {
                                    endGc.ResetType();
                                }
                                startGc.ResetType();
                            }
                            isResetGround = false;
                            ResetStatus();
                            ResetTemple();
                        }
					}
				}
			}
		}
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

	public int IsCorrectDir(Vector2 dirNormalized) {
		if (isPortrait) {
			if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 9 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 5) {
                if (dirNormalized.x > 0)
                {
                    if (dirNormalized.y > 0) {
                        return 1;
                    }
                    else {
                        return 2;
                    }
                }
                else {
                    if (dirNormalized.y > 0)
                    {
                        return 5;
                    }
                    else
                    {
                        return 4;
                    }
                }
            } else if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 0 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 10) {
                if (dirNormalized.y > 0) {
                    return 0;
                }
                else {
                    return 3;
                }
            }
		} else {
			if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 5 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 9) {
                if (dirNormalized.x > 0)
                {
                    if (dirNormalized.y > 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    if (dirNormalized.y > 0)
                    {
                        return 5;
                    }
                    else
                    {
                        return 3;
                    }
                }
            } else if (Mathf.Round (Mathf.Abs (dirNormalized.x * 10)) == 10 && Mathf.Round (Mathf.Abs (dirNormalized.y * 10)) == 0) {
                if (dirNormalized.x > 0)
                {
                    return 1;
                }
                else
                {
                    return 4;
                }
            }
		}
		return -99;
	}


	public Vector2 ConvertDirNormalized(Vector2 org, Vector2 dir){

		return (dir - org).normalized;
	}

	private void ResetGround(){
		foreach (GroundController gc in allGcs) {
			gc.ResetType (true);
		}

		for (int i = 0; i < 5; i++) {
			playerButton [i].ResetRatio ();
		}

		allRatioData = new List<RaycastData> ();
		recAllRatioData = new List<RaycastData> ();

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

        round = 0;
        energe = 4 + round;
        SetEnergy(true);

        RoundStart ();
	}

	private void ResetStatus(){
		startCharaImage = null;
		endCharaImage = null;
		startGc = null;
		endGc = null;
        usedEnergy = 0;
        ResetSpecialRatio();
        healingIcon.gameObject.SetActive(false);
	}


    private void ResetSpecialRatio(bool init = false) {
        if (init)
        {
            specialRatio = new int[Enum.GetNames(typeof(SpecailGround)).Length - 1];
        }
        else {
            specialRatio[(int)SpecailGround.Heal - 1] = 0;
        }
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
				enemyButton [i].transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = string.Empty;
			}
		} 
		else {
			for (int i = 0; i < order.Count; i++) {
				enemyButton [order.ElementAt (i)].transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = (i + 1).ToString ();
			}
		}
	}

	public void SetUnLockUI(int idx){
		enemyButton [idx].transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = string.Empty;
	}

    public void SetButtonCondition(int idx, int[] condition, bool isInit, int? level = null) {
        if (isInit)
        {
            playerButton[idx].InitConditonText(condition, level);
        }
        else
        {
            playerButton[idx].SetConditonText(condition);
        }
    }

	private void CreateSE(){
		showItemCount += 16;
		for (int i = 0; i < 16; i++)
		{
			FightSEController showItem = Instantiate(showGrounds) as FightSEController;
			showItem.GetComponent<RectTransform>().SetParent(showGroup);
			showItem.transform.localPosition = Vector3.zero;
			showItem.gameObject.SetActive (false);
			SEPool.Enqueue(showItem);
		}
	}

	private void UseSkill(int idx){
		//skillController.UseSkill (idx);
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
            SetUnLockUI(idx);
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
		/*foreach (GroundController gc in allGcs) {
			if (gc._groundType == GroundType.Chara && gc.charaJob == job) {
				count++;
			}
		}*/

		return count / 2;
	}

    public int GetCharaGround()
    {
        int count = 0;
        foreach (GroundController gc in allGcs)
       {
            if (gc._groundType == GroundType.Chara)
            {
                count++;
            }
       }

        return count;
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
	Attack = 3,
	Damage = 4
}

public enum SpecailGround
{
    None = 0,
    Heal = 1,
    Physical = 2,
    Magic = 3,
}

