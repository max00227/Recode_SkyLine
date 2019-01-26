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

	List<GroundSEController> completeSe;

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
                gc.plusGroundType = OnPlusGroundType;
            }
        }

        CreateGround = 3;

        lockCount = 0;


        SetData();

        energe = 5;
        conditionDown = new int[3];
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

    #region ShowView
    private void OnPlusGroundType(GroundType groundType, bool useEnergy) {
        if (useEnergy) {
            if (groundType == GroundType.Copper)
            {
                usedEnergy += 2;
            }
            else {
                usedEnergy += 1;
            }
        }

        conditionDown[(int)groundType - 1]++;
    }


	private void OnFight(){
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
        fightController.ConditionDown(conditionDown);

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
        isResetGround = !groundPool.NextRound();

        if (isResetGround)
        {
            ResetGround();
        }
        else {
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

		completeSe = new List<GroundSEController> ();
		canAttack = new List<int> ();

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
                            startGc.OnPrevType();

							if (endGc != null) {
                                endGc.OnPrevType();
                                if (charaGc.Last.Value == endGc) {
									if (endCover) {
										endGc.OnPrevCover ();
										endCover = false;
									}
									charaGc.RemoveLast ();
								} 
							}

							/*if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 0 
								|| (int)r.gameObject.GetComponent<GroundController>()._groundType == 99
								|| ((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover))
							{*/
								TouchDrap (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>()._groundType !=10 && canCover));
							/*}
							else
							{
                                Debug.Log("Error");
								TouchError ();
							}*/
						}
					}
				}
			}
		}
	}

	private void TouchDrap(GroundController gc, bool isCover){
		GroundController checkGc = gc;
        usedEnergy = 0;
        conditionDown = new int[3];
		Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, checkGc.transform.localPosition);

		if (IsCorrectDir(dir))
		{
			endGc = gc;

			if (isCover) {
				endCover = true;
				endGc.OnCover ();
			}

            endGc.ChangeChara (fightController.GetJob("P", (int)charaIdx));

			charaGc.AddLast (endGc);

            startGc.OnChangeType(false);
            endGc.OnChangeType(false);

            spaceCorrect = true;
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
                            if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 99)
                            {
                                isResetGround = true;
                            }


                            energe = energe - usedEnergy;
                            SetEnergy();

                            fightController.ConditionDown(conditionDown);

                            spaceCount++;

                            checkButton.interactable = true;

                            canCover = false;

                            fightController.SetJob((int)charaIdx);
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
                                startGc.OnPrevType();
                                if (endGc != null)
                                {
                                    endGc.OnPrevType();
                                    endGc.ResetType();
                                }
                                startGc.ResetType();
                            }
                            charaGc.RemoveLast();
                            isResetGround = false;
                            ResetStatus();
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
			gc.ResetType (true);
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
        usedEnergy = 0;
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

    public void SetButtonCondition(int idx, List<int> condition, bool isInit, int? level = null) {
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
	Attack = 3,
	Damage = 4
}

