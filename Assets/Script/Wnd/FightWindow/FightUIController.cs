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

	int[] recJobRatios;

	int[] preJobRatios;

	int[] jobRatios;

	/*[SerializeField]
	public NumberSetting[] ratioTxt;*/

	private bool spaceCorrect;

	//bool hasDamage;

	List<RaycastData> recAllRatioData, allRatioData, testAllRatioData;


	List<GroundController[]> raycasted;

	int[] ratioCount = new int[5];

	List<ExtraRatioData> ExtraRatios;

	List<GroundSEController> completeSe;

	int[] recActLevel;

	public FightItemButton[] charaButton;

	public FightItemButton[] enemyButton;

	private int energe;

	[SerializeField]
	private NumberSetting energeNum;

	private int spaceCount = 0;

	bool fightStart;

	int unCompleteCount;

	int unShowed;

	int lockCount;

	List<int> canAttack;

	List<ExtraRatioData> extraedGc;

	int spCount;

	bool canCover;

	bool startCover, endCover;

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
		monsterCdTimes = new int[5]{7,5,3,1,6};
		fightController.SetCDTime (monsterCdTimes, false);
		fightController.onProtect = GetHasProtect;
		fightController.onSkillCDEnd = OnSkillCDEnd;
		fightController.SetData ();

		lockOrder = new LinkedList<int> ();
		SetLockUI ();



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

		fightStart = false;

		SetData ();

		energe = 1;
		energeNum.SetNumber (energe);

		fightController.onLockButton = OnLockButton;

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
				if (fightController.characters [i].job == data.extraJob) {
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
		fightController.onComplete = FightEnd;
		CheckActLevel ();
		fightController.SetProtect (protectJob);
		fightController.onShowFight = OnShowFight;
		fightController.FightStart (lockCount != 0, canAttack, recJobRatios, recActLevel);
	}

	private void OnShowFight(int orgIdx, DamageData damageData, InitiatorType iType){
		FightItemButton org = iType == InitiatorType.Player ? charaButton [orgIdx] : enemyButton [orgIdx];
		FightItemButton target = iType == InitiatorType.Enemy ? charaButton [damageData.targetIdx] : enemyButton [damageData.targetIdx];

		GroundSEController gse = SEPool.Dequeue ();
		gse.SetDamageShow (org.transform.localPosition, target, damageData.hpRatio);
		gse.onRecycleDamage = ShowFightEnd;
		gse.gameObject.SetActive (true);
		gse.Run ();
	}

	private void ShowFightEnd(GroundSEController gse, float ratio, FightItemButton target){
		gse.gameObject.SetActive (false);
		target.SetHpBar (ratio);
		SEPool.Enqueue (gse);
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

		if (Input.GetKeyDown(KeyCode.A)) {
			if (!fightStart && spaceCount > 0) {
				CheckOut ();
			}
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			fightController.ShowHasAnim ();
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			foreach (GroundController gc in charaGc) {
				Debug.LogWarning (gc.name + " : " + gc.isActived);
			}
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			testAllRatioData = new List<RaycastData> ();
			List<string[]> arraySt = new List<string[]> ();
			foreach (GroundController gc in charaGc) {
				foreach (var data in gc.OnChangeType(false, false, true)) {
					string str = data.start.name;
					string end = data.end.name;
					if (arraySt.Count == 0) {
						arraySt.Add (new string[2]{ str, end });
						testAllRatioData.Add (data);
					} else {
						bool has = false;

						for (int i = 0; i < arraySt.Count; i++) {
							if ((arraySt [i] [0] == str && arraySt [i] [1] == end) || (arraySt [i] [1] == str && arraySt [i] [0] == end)) {
								has = true;
							} 
						}

						if (!has) {
							arraySt.Add (new string[2]{ str, end });
							testAllRatioData.Add (data);
						}
					}
				}

			}

			string dataString = string.Empty;
			foreach (var data in testAllRatioData) {
				Debug.LogWarning (data.start.name + " : " + data.end.name + " : " + data.ratio);
			}
		}

		if (!fightStart) {
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

	void FightEnd(){
		UpEnerge ();

		if (isResetGround) {
			ResetGround ();
		} else {
			NextRound ();
		}
	}


	void RoundStart(bool isCenter = true){
		groundPool.SetCreateGround (CreateGround + (int)Mathf.Ceil (resetGroundCount / 2));
		groundPool.RoundStart (isCenter);
	}

	/// <summary>
	/// 進行下一回合擺放前抽選產生Ground
	/// </summary>
	/// <param name="isSpace">是否擺放角色</param>
	private void NextRound(bool isSpace = true){
		fightStart = false;

		foreach (var v in charaButton) {
			v.SetTextColor (Color.black);
		}

		groundPool.ChangeLayer ();

		if (!groundPool.NextRound ()) {
			ResetGround ();
		};

		foreach (FightItemButton cBtn in charaButton) {
			cBtn.SetEnable (true);
		}
	}

	private void RoundEnd(bool hasDamage)
	{
		spCount++;

		/*Debug.LogWarning ("ROUND " + spCount);
		foreach (RaycastData data in allRatioData) {
			Debug.LogWarning (data.ratio);
		}*/

		completeSe = new List<GroundSEController> ();
		canAttack = new List<int> ();

		fightStart = true;

		foreach (FightItemButton cBtn in charaButton) {
			cBtn.SetEnable (false);
		}

		if (energe >= 3) {
			MonsterCdDown (true);
		}

		if (spaceCount > 1) {
			MonsterCdDown ();
		}


		ResetDamage (false);

		for (int i = 0; i < jobRatios.Length; i++) {
			if (jobRatios [i] != recJobRatios [i]) {
				for (int j = 0; j < fightController.characters.Length; j++) {
					if (fightController.characters [j].job == i) {
						AddCanAttack (j);
						charaButton [j].SetRatioTxt (jobRatios [i], true);
						charaButton[j].onComplete = RecycleShowUp;
						unCompleteCount++;
					}
				}
			}
		}
			
		recJobRatios = jobRatios;

		if (hasDamage) {
			StartCoroutine (CheckRatio ());
		} 
		else {
			fightController.onComplete = FightEnd;
			fightController.onShowFight = OnShowFight;
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
		if (energe < 3) {
			energe++;
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
			if (fightController.characters [(int)charaIdx].job == 2) {
				startGc.onProtection = OnProtection;
			}

			charaGc.AddLast (startGc);
			if (isCover) {
				startCover = true;
				startGc.OnCover ();
			}
			startGc.ChangeChara (fightController.characters [(int)charaIdx].job);

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
			endGc.ChangeChara (fightController.characters [(int)charaIdx].job);

			if (fightController.characters [(int)charaIdx].job == 2) {
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
							

						if (spaceCount > 0) {
							energe--;
							energeNum.SetNumber (energe);
						}

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
				endGc.ChangeChara (fightController.characters [(int)charaIdx].job);
			}
		}


		foreach (GroundController gc in charaGc) {
			ResponseData(gc.OnChangeType(isTouchUp, isRoundEnd));
		}

		if (allRatioData.Count != recAllRatioData.Count) {
			hasDamage = true;
		}

		ChangeCharaRatio ();
		return hasDamage;
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
				if (fightController.characters [j].job == i) {
					charaButton [j].SetRatioTxt (jobRatios [i]);
					if (recJobRatios [fightController.characters [i].job] != jobRatios [fightController.characters [i].job]) {
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



	private void MonsterCdDown(bool overfill = false){
		if (overfill) {
			for (int i = 0; i < monsterCdTimes.Length; i++) {
				monsterCdTimes [i]--;
			}
			return;
		}
		for (int i = 0; i < monsterCdTimes.Length; i++) {
			monsterCdTimes [i] = monsterCdTimes [i] - (spaceCount - 1);
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
						if (fightController.characters [i].job == data.CharaJob) {
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
				charaButton [i].SetRatioTxt (recJobRatios [fightController.characters [i].job]);
			} else {
				charaButton [i].SetRatioTxt (preJobRatios [fightController.characters [i].job]);
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

		recJobRatios = ratioCount = recActLevel =  new int[5] { 0, 0, 0, 0, 0 };
		for (int i = 0; i < 5; i++) {
			charaButton [i].ResetRatio ();
		}

		allRatioData = new List<RaycastData> ();
		recAllRatioData = new List<RaycastData> ();

		charaGc = new LinkedList<GroundController> ();
		extraedGc = new List<ExtraRatioData> ();


		charaIdx = null;


		fightStart = false;
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

		foreach (FightItemButton cBtn in charaButton) {
			cBtn.SetEnable (true);
		}


		RoundStart ();
	}

	private void ResetStatus(){
		startCharaImage = null;
		endCharaImage = null;
		startGc = null;
		endGc = null;
	}

	private void GetHasProtect(bool isHas){
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
		if (!fightStart) {
			charaIdx = idx;
		}
	}

	LinkedList<int> lockOrder;

	public void LockEnemy (int idx){
		if (!fightStart) {
			if (lockOrder.Count < 3) {
				lockOrder = fightController.LockOrder (idx);
			} 
			else {
				lockOrder = fightController.UnLockOrder ();
			}
		}
		SetLockUI ();
	}

	private void SetLockUI(){
		if (lockOrder.Count == 0) {
			for (int i = 0; i < enemyButton.Length; i++) {
				enemyButton [i].transform.GetChild (0).GetComponent<Text> ().text = string.Empty;
			}
		} 
		else {
			for (int i = 0; i < lockOrder.Count; i++) {
				enemyButton [lockOrder.ElementAt (i)].transform.GetChild (0).GetComponent<Text> ().text = (i + 1).ToString ();
			}
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

	private void OnSkillCDEnd(int charaIdx){
		Debug.LogWarning ("Chara " + charaIdx + " Can Use Skill");
	}

	private void OnLockButton(int idx, InitiatorType atype){
		
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
	Damage = 3
}