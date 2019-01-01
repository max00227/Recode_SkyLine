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
	GroundRaycastController groundPool;

	[SerializeField]
	Transform CharaGroup;

	[SerializeField]
	Transform rayGroup;

	[SerializeField]
	Transform imagePool;

	[SerializeField]
	FightController fightController;

	GroundController[] allGcs;

	GroundController startGc, endGc;

	bool isResetGround = false;

	private int[] monsterCdTimes = new int[5];

	int CreateGround, ResetCount;

	List<CharaImageData> _charaGroup = new List<CharaImageData> ();

	Stack<Image> _imagePool = new Stack<Image> ();

	[SerializeField]
	Image spriteImage;

	int? charaIdx;

	Image startCharaImage, endCharaImage;

	LinkedList<GroundController> charaGc;

	int resetGroundCount;

	public Sprite[] CharaSprite;

	int[] recJobRatios = new int[5];

	int[] preJobRatios;

	int[] jobRatios;

	Dictionary<int, Dictionary<StatusLargeData,int>> charaStatus;
	Dictionary<int, Dictionary<StatusLargeData,int>> enemyStatus;

	/*[SerializeField]
	public NumberSetting[] ratioTxt;*/

	private bool spaceCorrect;

	//bool hasDamage;

	List<RaycastData> recAllRatioData, allRatioData, testAllRatioData;


	List<GroundController[]> raycasted;

	int[] ratioCount = new int[5];

	List<ExtraRatioData> ExtraRatios;

	List<GroundSEController> completeSe;

	int[] recActLevel = new int[5];

	public FightItemButton[] charaButton;

	public FightItemButton[] enemyButton;

	private int energe;

	[SerializeField]
	private NumberSetting energeNum;

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

	void SetData() {
		monsterCdTimes = new int[5]{7,5,10,10,6};
		fightController.SetCDTime (monsterCdTimes, false);
		fightController.SetData ();
		charaStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();
		enemyStatus = new Dictionary<int, Dictionary<StatusLargeData, int>> ();

		foreach (FightItemButton btn in charaButton) {
			btn.SetHpBar (1, false);
		}

		foreach (FightItemButton btn in enemyButton) {
			btn.SetHpBar (1, false);
		}

		lockOrder = new LinkedList<int> ();

		groundPool.SetController ();
	}

	// Use this for initialization
	void Start () {
		lockOrder = new LinkedList<int> ();

		for (int i = 0; i < imageItem; i++) {
			Image _image = Instantiate (spriteImage) as Image;
			_image.GetComponent<RectTransform> ().SetParent (imagePool);
			_image.transform.localPosition = Vector3.zero;
			_image.gameObject.SetActive (false);
			_imagePool.Push (_image);
		}

		for (int i = 0; i < showItemCount; i++)
		{
			GroundSEController showItem = Instantiate(showGrounds) as GroundSEController;
			showItem.GetComponent<RectTransform>().SetParent(showGroup);
			showItem.transform.localPosition = Vector3.zero;
			showItem.gameObject.SetActive (false);
			SEPool.Enqueue(showItem);
		}

		allGcs = groundPool.GetComponentsInChildren<GroundController> ();

		resetGroundCount = 0;

		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType != 99) {
				gc.plusRatio = OnPlusRatio;
			}
		}

		CreateGround = 3;

		lockCount = 0;

		SetData ();

		energe = 2;
		energeNum.SetNumber (energe);

		ResetGround(true);
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
			foreach (FightItemButton btn in charaButton) {
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
			recJobRatios [data.extraJob] += 25;
			for (int i = 0; i < fightController.characters.Length; i++) {
				if (fightController.GetJob (TargetType.Player, i) == data.extraJob) {
					GroundSEController gse = SEPool.Dequeue ();
					gse.SetExtraSE (org, charaButton [i].transform.localPosition, i, data.upRatio);
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
		charaButton [idx].SetExtra (upRatio);
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
			FightItemButton org = null;
			FightItemButton target = null;
			if (allDamage [0].isSelf) {
				org = allDamage [i].tType == TargetType.Enemy ? enemyButton [allDamage [i].orgIdx] : charaButton [allDamage [i].orgIdx];
			} else {
				org = allDamage [i].tType == TargetType.Enemy ? charaButton [allDamage [i].orgIdx] : enemyButton [allDamage [i].orgIdx];
			}
			target = allDamage [i].tType == TargetType.Player ? charaButton [allDamage [i].targetIdx] : enemyButton [allDamage [i].targetIdx];

			GroundSEController gse = SEPool.Dequeue ();
			gse.SetAttackShow (org.transform.localPosition, target, allDamage [i]);
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


	private void ShowFightEnd(GroundSEController gse, DamageData damageData, FightItemButton target){
		gse.gameObject.SetActive (false);
		target.SetHpBar (damageData.hpRatio);
		SEPool.Enqueue (gse);
		gse.onRecycle = null;
		gse.SetDamageShow (damageData, target.transform.localPosition);
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
			ResetGround (true);
		}

		if (Input.GetKeyDown(KeyCode.C)) {
			canCover = !canCover;
		}

		if (Input.GetKeyDown(KeyCode.H)) {
			foreach (var v in charaGc) {
				Debug.Log (v.name);
			}
		}

		if (Input.GetKeyDown(KeyCode.Y)) {
			fightController.ShowSoulData ();
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
	}

	private void CheckLockStatus (){
		foreach (KeyValuePair<int,Dictionary<StatusLargeData,int>> kv in charaStatus) {
			foreach (KeyValuePair<StatusLargeData,int> kv2 in kv.Value) {
				if (kv2.Key.charaStatus [0] == (int)Nerf.UnTake) {
					charaButton [kv.Key].SetEnable (false);
				}
			}
		}
	}

	/// <summary>
	/// 進行下一回合擺放前抽選產生Ground
	/// </summary>
	/// <param name="isSpace">是否擺放角色</param>
	private void NextRound(bool isSpace = true){
		foreach (var v in charaButton) {
			v.SetTextColor (Color.black);
		}

		groundPool.ChangeLayer ();

		if (!groundPool.NextRound ()) {
			ResetGround ();
		};

		OnOpenButton ();

		newRaycastData = new List<RaycastData> ();
		CheckLockStatus ();

		ChangeStatus (FightStatus.RoundStart);
	}

	private void RoundEnd(bool hasDamage)
	{
		spCount++;

		completeSe = new List<GroundSEController> ();
		canAttack = new List<int> ();

		ChangeStatus (FightStatus.FightStart);

		OnCloseButton (TargetType.Both);

		ResetDamage (false);

		for (int i = 0; i < jobRatios.Length; i++) {
			if (jobRatios [i] != recJobRatios [i]) {
				for (int j = 0; j < fightController.characters.Length; j++) {
					if (fightController.GetJob(TargetType.Player, j) == i) {
						AddCanAttack (j);
						charaButton [j].SetRatioTxt (jobRatios [i], true);
						charaButton[j].onComplete = RecycleShowUp;
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
		if (energe < 5) {
            energe += 2;
			energeNum.SetNumber (energe);
		}
	}

	private void TouchDown(bool isTouch = false){
		var result = CanvasManager.Instance.GetRaycastResult (isTouch);

		if (result.Count > 0) {
			foreach (var r in result) {
				if (r.gameObject.CompareTag("fightG")) {
					if ((int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 0
						|| (int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 99
						|| ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType !=10 && canCover)) {
						TouchDown (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>().matchController._groundType !=10 && canCover));
					}
				}
			}
		}
	}

	private void TouchDown(GroundController gc, bool isCover){
		startGc = gc.matchController;
		if (charaIdx != null) {
			if (fightController.GetJob(TargetType.Player, (int)charaIdx) == 2) {
				startGc.onProtection = OnProtection;
			}

			charaGc.AddLast (startGc);
			if (isCover) {
				startCover = true;
				startGc.OnCover ();
			}
			startGc.ChangeChara (fightController.GetJob(TargetType.Player, (int)charaIdx));

			startCharaImage = PopImage (_imagePool, startGc, gc.transform.localPosition);
			endCharaImage = PopImage (_imagePool, null, gc.transform.localPosition);

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
					if (r.gameObject.CompareTag("fightG"))
					{
						endCharaImage.transform.localPosition = r.gameObject.transform.localPosition;

						if (endGc != r.gameObject.GetComponent<GroundController> ().matchController) {
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

							if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 0 
								|| (int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99
								|| ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType !=10 && canCover))
							{
								TouchDrap (r.gameObject.GetComponent<GroundController> (),((int)r.gameObject.GetComponent<GroundController>().matchController._groundType !=10 && canCover));
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
		GroundController checkGc = gc.matchController;

		Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, checkGc.transform.localPosition);

		if (IsCorrectDir(dir))
		{
			endGc = gc.matchController;

			if (isCover) {
				endCover = true;
				endGc.OnCover ();
			}
			endGc.ChangeChara (fightController.GetJob(TargetType.Player, (int)charaIdx));

			if (fightController.GetJob(TargetType.Player, (int)charaIdx) == 2) {
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
					if (spaceCorrect == true && r.gameObject.CompareTag("fightG"))
					{
						bool onlyAdd = false;

						if ((int)r.gameObject.GetComponent<GroundController>()._groundType == 99) {
							isResetGround = true;
						}


                        energe = energe - (spaceCount + 1);
					    energeNum.SetNumber (energe);

						if (energe > 0 && !isResetGround) {
							ResetStatus ();
							onlyAdd = true;
						}

						spaceCount++;

						canCover = false;

						if (onlyAdd && !isResetGround) {
							preJobRatios = jobRatios;
							CheckGround (true);

							charaIdx = null;
							return;
						}

						RoundEnd(CheckGround (false, true));
					}
					else {
						PopImage ();
						PopImage ();
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

	public void CheckOut(){
		RoundEnd(CheckGround (false, true));
	}

	private bool CheckGround(bool isTouchUp, bool isRoundEnd = false) {
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
				endGc.ChangeChara (fightController.GetJob(TargetType.Player, (int)charaIdx));
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
		int newCount = 0;
		int job = 0;
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
			for (int j = 0; j < fightController.characters.Length; j++) {
				if (fightController.GetJob(TargetType.Player, j) == i) {
					charaButton [j].SetRatioTxt (jobRatios [i]);
					if (recJobRatios [fightController.GetJob(TargetType.Player, i)] != jobRatios [fightController.GetJob(TargetType.Player, i)]) {
						charaButton [i].SetTextColor (Color.red);
					} else {
						charaButton [i].SetTextColor (Color.black);
					}
				}
			}
		}
	}

	private void CheckActLevel()
	{
		foreach (var data in recAllRatioData) {
			if (data.hits.Count >= 5 && data.ratio > 250) {
				ChangeActLevel (data.CharaJob, 3);
			}
			else if (data.hits.Count >= 3 && data.ratio > 150) {
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

	public void ChangeHpBar(int idx, TargetType tType, float hpRatio, bool isUp){
		if (tType == TargetType.Player) {
			charaButton [idx].SetHpBar (hpRatio, true, isUp);
		} 
		else {
			enemyButton [idx].SetHpBar (hpRatio, true, isUp);
		}
	}

	#region Skill
	public void OnRecovery(int idx, TargetType tType, float hpRatio){
		ChangeHpBar (idx, tType, hpRatio, true);
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
					for (int i = 0; i < fightController.characters.Length; i++) {
						if (fightController.GetJob(TargetType.Player, i) == data.CharaJob) {
							postions.Add (charaButton [i].transform.localPosition + (Vector3.up * 30) * (ratioCount [i] - 1));
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
		for (int i = 0; i < fightController.characters.Length; i++) {
			if (!isPrev) {
				charaButton [i].SetRatioTxt (recJobRatios [fightController.GetJob(TargetType.Player, i)]);
			} else {
				charaButton [i].SetRatioTxt (preJobRatios [fightController.GetJob(TargetType.Player, i)]);
			}
			charaButton [i].SetTextColor (Color.black);
		}
	}

	public bool IsCorrectDir(Vector2 dirNormalized) {
		if (Mathf.Round(Mathf.Abs(dirNormalized.x * 10)) == 5 && Mathf.Round(Mathf.Abs(dirNormalized.y * 10)) == 9){
			return true;
		}
		else if (Mathf.Round(Mathf.Abs(dirNormalized.x * 10)) == 10 && Mathf.Round(Mathf.Abs(dirNormalized.y * 10)) == 0)
		{
			return true;
		}
		return false;
	}


	public Vector2 ConvertDirNormalized(Vector2 org, Vector2 dir){

		return (dir - org).normalized;
	}

	private void ResetGround(bool isInit = false){
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
		}

		for (int i = 0; i < 5; i++) {
			recJobRatios [i] = 0;
			ratioCount [i] = 0;
			recActLevel [i] = 0;
		}

		for (int i = 0; i < 5; i++) {
			charaButton [i].ResetRatio ();
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
			PopImage ();
		}

		if (isInit == false) {
			resetGroundCount++;
			fightController.SetResetRatio (Mathf.CeilToInt(resetGroundCount/2));
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
			imageData.linkGc = linkGc;
			_charaGroup.Add (imageData);
			return image;
		} else {
			image = _charaGroup [_charaGroup.Count - 1].image;
			_charaGroup.RemoveRange (_charaGroup.Count - 1, 1);
			image.GetComponent<RectTransform> ().SetParent (imagePool);
			image.transform.localPosition = Vector3.zero;
			image.gameObject.SetActive (false);
			image.sprite = null;
			_imagePool.Push (image);
			return null;
		}
	}

	public void SelectChara (int idx){
		if (CheckStatus (FightStatus.RoundStart)) {
			charaIdx = idx;
		} 
		else if (CheckStatus (FightStatus.SelSkillTarget)) {
			fightController.SelectSkillTarget (TargetType.Player, idx);
		}
	}

	LinkedList<int> lockOrder;

	public void LockEnemy (int idx){
		if (CheckStatus (FightStatus.RoundStart)) {
			fightController.LockOrder (idx);
		} 
		else if (CheckStatus (FightStatus.SelSkillTarget)) {
			fightController.SelectSkillTarget (TargetType.Enemy, idx);
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

	public void OnSkillCDEnd(int charaIdx){
		Debug.LogWarning ("Chara " + charaIdx + " Can Use Skill");
	}

	public void OnSelectionDir(List<int> idxList, TargetType tType){
		OnCloseButton (TargetType.Both);

		foreach (int idx in idxList) {
			if (tType == TargetType.Player) {
				charaButton [idx].SetEnable (true);
			} 
			else {
				enemyButton [idx].SetEnable (true);
			}
		}
	}

	private void OnCloseButton(TargetType tType) {
		if (tType == TargetType.Player) {
			foreach (FightItemButton btn in charaButton) {
				btn.SetEnable (false);
			}
		} else if (tType == TargetType.Enemy) {
			foreach (FightItemButton btn in enemyButton) {
				btn.SetEnable (false);
			}
		} 
		else {
			foreach (FightItemButton btn in charaButton) {
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
		foreach (FightItemButton btn in charaButton) {
			btn.SetEnable (true);
		}
	}

	public void OnDead(int idx, TargetType tType){
		if (tType == TargetType.Player) {
			charaButton [idx].SetEnable (false, true);
		} 
		else {
			enemyButton [idx].SetEnable (false, true);
		}
	}

	private void SetButton(){
		foreach (FightItemButton btn in enemyButton) {
			btn.Init ();
		}
		foreach (FightItemButton btn in charaButton) {
			btn.Init ();
		}
	}

	public void OnStatus(int idx, StatusLargeData data, int level, TargetType tType){
		if (tType == TargetType.Player) {
			if (charaStatus.ContainsKey (idx)) {
				if (!charaStatus [idx].ContainsKey (data)) {
					charaStatus [idx].Add (data, level);
				} 
				else {
					charaStatus [idx] [data] = level;
				}
			} else {
				Dictionary<StatusLargeData,int> sData = new Dictionary<StatusLargeData, int> ();
				sData.Add (data, level);
				charaStatus.Add (idx, sData);
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

	public void OnStatusDown(int idx, StatusLargeData key, int time, TargetType tType){
		if (tType == TargetType.Player) {
			if (charaStatus.ContainsKey (idx)) {
				if (charaStatus [idx].ContainsKey (key)) {
					if (time == 0) {
						charaStatus [idx].Remove (key);
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

	public void RmStatus(int idx, StatusLargeData key, TargetType tType){
		if (tType == TargetType.Player) {
			charaStatus [idx].Remove (key);
		} else {
			enemyStatus [idx].Remove (key);
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

    public int GetEnerge() {
        return energe;
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
}

public struct CharaImageData{
	public Image image;
	public GroundController linkGc;
}

public enum SpecailEffectType{
	Reverse = 1,
	ExtraRatio = 2,
	Attack = 3,
	Damage = 4
}

