using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using model.data;
using System;
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

	GroundController[] allGcs;

	List<GroundController> norGcs;

	GroundController startGc, endGc;

	bool isResetGround=false;

	private CharaLargeData[] characters;

	private MonsterLargeData[] monsters;

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

	int[] jobRatios;

	[SerializeField]
	public RatioSetting[] ratioTxt;

	private bool spaceCorrect;

	bool hasDamage;

	List<RaycastData> recAllRatios, allRatios;


	List<GroundController[]> raycasted;

	int[] ratioCount = new int[5];

	List<ExtraRatioData> ExtraRatios;

	List<GroundSEController> completeSe;

	Dictionary<int, int> recActLevel;

	public Button[] charaButton;

	private int energe;

	private int spaceCount = 0;

	bool fightStart;

	int unCompleteCount;

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

	void SetData() {
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

		groundPool.SetController ();
	}

	// Use this for initialization
	void Start () {
		norGcs = new List<GroundController> ();

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

		foreach (GroundController gc in allGcs) {
			if ((int)gc._groundType == 0) {
				gc.plusRatio = OnPlusRatio;
				norGcs.Add (gc);
			}
		}

		resetGroundCount = 0;

		CreateGround = 3;

		characters = new CharaLargeData[5];
		monsters = new MonsterLargeData[5];

		fightStart = false;

		SetData ();
		ResetGround(true);
	}


	#region ShowRecycle
	private void RecycleReverseItem(GroundSEController rg) {
		SEPool.Enqueue (rg);
		completeSe.Add (rg);
		rg.onRecycle = null;
		if (SEPool.Count == showItemCount && SEingPool.Count == 0) {
			foreach (GroundSEController se in completeSe) {
				se.gameObject.SetActive (false);
				se.CloseSE ();
			}
			foreach (RatioSetting rs in ratioTxt) {
				rs.run ();
			}
		}
	}

	private void RecycleShowUp(){
		unCompleteCount--;
		if (unCompleteCount == 0) {
			if (ExtraRatios.Count > 0) {
				StartCoroutine (OnShowExtra ());
			} else {
				if (isResetGround) {
					ResetGround ();
				} else {
					NextRound ();
				}
			}
		}
	}

	private void RecycleExtraItem(GroundSEController rg) {
		rg.gameObject.SetActive (false);
		SEPool.Enqueue (rg);
		rg.onRecycle = null;

		if (SEPool.Count == showItemCount && SEingPool.Count == 0) {
			rg.onExtraUp = null;
			recAllRatios = allRatios;
			if (isResetGround) {
				ResetGround ();
			} else {
				NextRound ();
			}
		}
	}


	private void OnPlusRatio(ExtraRatioData plusDamage) {
		if (ExtraRatios.Count > 0) {
			for (int i = 0; i < ExtraRatios.Count; i++) {
				if (ExtraRatios [i].gc == plusDamage.gc) {
					ExtraRatioData changeData = new ExtraRatioData ();
					changeData.ratio = ExtraRatios [i].ratio + plusDamage.ratio;
					changeData.charaJobs = plusDamage.charaJobs;
					changeData.gc = ExtraRatios [i].gc;
					ExtraRatios [i]=changeData;
					return;
				}
			}
		}
		ExtraRatios.Add(plusDamage);
	}

	private IEnumerator OnShowExtra(){
		foreach (var data in ExtraRatios) {
			List<GroundController> org = new List<GroundController> ();
			org.Add (data.gc);
			foreach (var jobIdx in data.charaJobs) {
				recJobRatios [jobIdx - 1] += 25;
				for (int i = 0; i < characters.Length; i++) {
					if (characters [i].job == jobIdx) {
						GroundSEController rg = SEPool.Dequeue ();
						rg.SetExtraSE (org, charaButton [i].transform.localPosition, i);
						rg.onRecycle = RecycleExtraItem;
						rg.onExtraUp = ExtraRatioUp;
						SEingPool.Enqueue (rg);
					}
				}
			}
		}


		while (SEingPool.Count > 0) {
			GroundSEController rg = SEingPool.Dequeue ();
			rg.gameObject.SetActive (true);
			rg.Run ();
			yield return new WaitForSeconds (0.2f);
		}
	}

	private void ExtraRatioUp(int idx){
		ratioTxt [idx].SetExtra ();
	}
	#endregion

	private void OnProtection(int targetIdx){
		//Debug.Log ("Guardian : " + guardian + " , Target" + target);
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.G)) {
			RoundStart();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			ResetGround();
		}

		if (Input.GetKeyDown(KeyCode.Y)) {
			foreach (var v in charaGc) {
				Debug.Log (v.name);
			}
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			CheckOut ();
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			foreach (var v in _charaGroup) {
				Debug.Log (v.linkGc.name);
			}
		}

		if (Input.GetKeyDown(KeyCode.O)) {
			foreach (var v in recAllRatios) {
				Debug.Log (v.start+" : "+v.end);
			}
		}

		if (!fightStart) {
			if (Input.GetKeyDown (KeyCode.Mouse0)) {
				TouchDown ();
			}

			if (Input.GetKey (KeyCode.Mouse0)) {
				TouchDrap ();
			}

			if (Input.GetKeyUp (KeyCode.Mouse0)) {
				TouchUp ();
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
			// 当前时间 -  按钮最后一次被按下的时间 > 延迟时间0.2秒
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

		int focusCount = 1;

		foreach (var v in ratioTxt) {
			v.SetColor (Color.black);
		}

		groundPool.ChangeLayer ();

		if (!groundPool.NextRound ()) {
			ResetGround ();
		};

		foreach (Button cBtn in charaButton) {
			cBtn.enabled = true;
		}

	}

	private void RoundEnd()
	{
		completeSe = new List<GroundSEController> ();

		fightStart = true;

		foreach (Button cBtn in charaButton) {
			cBtn.enabled = false;
		}

		if (energe < 3) {
			energe++;
		} 
		else {
			MonsterCdDown (true);
		}

		if (spaceCount > 1) {
			MonsterCdDown ();
		}

		groundPool.RoundEnd ();

		ResetDamage ();

		for (int i = 0; i < jobRatios.Length; i++) {
			if (jobRatios [i] != recJobRatios [i]) {
				for (int j = 0; j < characters.Length; j++) {
					if (characters [j].job == i + 1) {
						ratioTxt [j].SetShowUp (jobRatios [i]);
						ratioTxt [j].onComplete = RecycleShowUp;
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
			if (isResetGround) {
				ResetGround ();
				return;
			}
			NextRound ();
		}

		hasDamage = false;
		spaceCount = 0;
		charaIdx = null;
		startCharaImage = endCharaImage = null;
		startGc = endGc = null;
		ratioCount = new int[5] { 0, 0, 0, 0, 0 };

		recAllRatios = allRatios;
	}

	private void TouchDown(bool isTouch = false){
		var result = CanvasManager.Instance.GetRaycastResult (isTouch);
		if (result.Count > 0) {
			foreach (var r in result) {
				if (r.gameObject.CompareTag("fightG")) {
					if ((int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 0
						|| (int)r.gameObject.GetComponent<GroundController> ().matchController._groundType == 99) {
						startGc = r.gameObject.GetComponent<GroundController> ().matchController;
						if (charaIdx != null) {
							if (characters [(int)charaIdx].job == 3) {
								startGc.onProtection = OnProtection;
							}

							charaGc.AddLast (startGc);
							startGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, null);

							startCharaImage = PopImage (_imagePool, startGc, r.gameObject.transform.localPosition);
							endCharaImage = PopImage (_imagePool, null, r.gameObject.transform.localPosition);

							if ((int)r.gameObject.GetComponent<GroundController> ()._groundType == 99) {
								isResetGround = true;
							}
						} 
					}
				}
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
								gc.OnPrevType ();
							}

							if (endGc != null) {
								if (charaGc.Last.Value == endGc) {
									endGc.ResetType ();
									charaGc.RemoveLast ();
								} 
							}

							if ((int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 0 || (int)r.gameObject.GetComponent<GroundController>().matchController._groundType == 99)
							{
								GroundController checkGc = r.gameObject.GetComponent<GroundController>().matchController;

								Vector2 dir = ConvertDirNormalized(startGc.transform.localPosition, checkGc.transform.localPosition);

								if (IsCorrectDir(dir))
								{
									endGc = r.gameObject.GetComponent<GroundController>().matchController;

									endGc.ChangeChara ((int)charaIdx, characters [(int)charaIdx].job, startGc);

									if (characters [(int)charaIdx].job == 3) {
										endGc.onProtection = OnProtection;
									}

									charaGc.AddLast (endGc);

									CheckGround();
									spaceCorrect = true;
								}
								else
								{
									TouchError ();
								}
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

	private void TouchError(){
		endGc = null;
		ResetDamage();
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

						startGc.pairGc = endGc;



						if (spaceCount > 0) {
							energe--;
						}

						if (energe > 0 && !isResetGround) {
							startCharaImage = endCharaImage = null;
							startGc = endGc = null;
							onlyAdd = true;
						}

						spaceCount++;

						foreach (GroundController gc in norGcs) {
							gc.SetJob(characters[(int)charaIdx].job);
						}
						if (onlyAdd) {
							charaIdx = null;
							return;
						}

						RoundEnd();
					}
					else {
						PopImage ();
						PopImage ();
						startCharaImage = endCharaImage = null;
						startGc.ResetType ();
						charaGc.RemoveLast ();
						ResetDamage ();
					}
				}
			}
		}
	}

	private void CheckOut(){
		RoundEnd();
	}

	private void CheckGround() {
		allRatios = new List<RaycastData> ();
		ExtraRatios = new List<ExtraRatioData>();
		hasDamage = false;

		foreach (GroundController gc in charaGc) {
			gc.raycasted = false;
		}
		foreach (GroundController gc in charaGc) {
			ResponseData(gc.OnChangeType());
		}

		if (allRatios.Count != recAllRatios.Count) {
			hasDamage = true;
		}

		ChangeCharaRatio ();
	}

	private void ResponseData(List<RaycastData> raycastData){
		foreach (var data in raycastData) {
			allRatios.Add (data);
		}
	}

	private void ChangeCharaRatio()
	{
		jobRatios = new int[5]{ 0, 0, 0, 0, 0 };
		foreach (var data in allRatios) {
			if (data.hits.Count >= 5 && data.ratio > 250) {
				ChangeActLevel (data.CharaJob, 3);
			}
			else if (data.hits.Count >= 3 && data.ratio > 150) {
				ChangeActLevel (data.CharaJob, 2);
			} else {
				ChangeActLevel (data.CharaJob, 1);
			}
			jobRatios [data.CharaJob - 1] = jobRatios [data.CharaJob - 1] + data.ratio;
		}

		for (int i = 0; i < jobRatios.Length; i++) {
			for (int j = 0; j < characters.Length; j++) {
				if (characters [j].job == i + 1) {
					ratioTxt [j].SetRatio (jobRatios [i]);
					if (recJobRatios [characters [i].job - 1] != jobRatios [characters [i].job - 1]) {
						ratioTxt [i].SetColor (Color.red);
					} else {
						ratioTxt [i].SetColor (Color.black);
					}
				}
			}
		}
	}

	private void ChangeActLevel(int job, int level){
		if (recActLevel [job - 1] < level) {
			recActLevel [job - 1] = level;
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
	}

	private IEnumerator CheckRatio() {
		if (allRatios.Count > recAllRatios.Count) {
			foreach (var data in allRatios)
			{
				bool hasNew = true;

				if (recAllRatios.Count > 0)
				{
					foreach (var recData in recAllRatios)
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
					List<Vector3> postions = new List<Vector3> ();
					for (int i = 0; i < characters.Length; i++) {
						if (characters [i].job == data.CharaJob) {
							postions.Add (charaButton [i].transform.localPosition + (Vector3.up * 30) * (ratioCount [i] - 1));
							ratioCount [i]++;
						}
					}
					GroundSEController rg = SEPool.Dequeue ();
					rg.SetReverseSE (data.hits, postions);
					rg.onRecycle = RecycleReverseItem;
					SEingPool.Enqueue (rg);
				}
			}
		}

		while (SEingPool.Count > 0) {
			GroundSEController rg = SEingPool.Dequeue();
			rg.gameObject.SetActive (true);
			rg.Run ();
			yield return new WaitForSeconds (0.5f*(rg.seGrounds.Count-1));
		}
	}

	private void ResetDamage() {
		for (int i = 0; i < characters.Length; i++) {
			ratioTxt [i].SetRatio (recJobRatios [characters [i].job - 1]);
			ratioTxt [i].SetColor (Color.black);
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

	private void ResetGround(bool isInit=false){
		foreach (GroundController gc in allGcs) {
			gc.ResetType ();
		}

		recJobRatios = ratioCount =  new int[5] { 0, 0, 0, 0, 0 };
		for (int i = 0; i < 5; i++) {
			ratioTxt [i].ResetRatio ();
			ratioTxt [i].SetColor (Color.black);
		}

		allRatios = new List<RaycastData> ();
		recAllRatios = new List<RaycastData> ();
		recActLevel = new Dictionary<int, int> ();


		for (int i = 0; i < 5; i++) {
			recActLevel.Add (i, 0);
		}


		charaGc = new LinkedList<GroundController> ();

		charaIdx = null;
		startCharaImage = endCharaImage = null;
		startGc = endGc = null;
		hasDamage = fightStart = isResetGround = spaceCorrect = false;
		spaceCount = 0;


		while (_charaGroup.Count > 0) {
			PopImage ();
		}

		if (isInit == false) {
			resetGroundCount++;
		} else {
			energe = 1;
		}

		RoundStart ();
	}

	private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis) {
		LayerMask mask = 1 << 8;
		RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

		return hits;
	}

	private void ChangeLayer(){
		foreach (GroundController gc in norGcs) {
			if ((int)gc._groundType != 0 && (int)gc._groundType != 99) {
				ChangeLayer (gc.transform.localPosition);
			}
		}
	}

	private void ChangeLayer(Vector2 center){
		RaycastHit2D[] hits;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits(center, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + i * 60))), 0.97f);
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController>().UpLayer();
			}
		}
	}

	/// <summary>
	/// Randoms the list.
	/// </summary>
	/// <returns>The list.</returns>
	/// <param name="randomCount">抽選數量</param>
	/// <param name="array">抽選清單</param>
	/// <param name="lastCount">真實數量</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	List<T> RandomList<T>(int randomCount, T[] array, int? lastCount= null){
		List<T> ListT = new List<T> ();
		int count = lastCount==null?array.Length:(int)lastCount;
		if (count > randomCount) {
			for (int i = 0; i < randomCount; i++) {
				int idx = UnityEngine.Random.Range (0, array.Length);
				while (ListT.Contains (array [idx])) {
					idx = UnityEngine.Random.Range (0, array.Length);
				}

				ListT.Add (array [idx]);
			}
		} else if (count <= randomCount) {
			for (int i = 0; i < array.Length; i++) {
				if (!ListT.Contains (array [i])) {
					ListT.Add (array [i]);
				}
			}
		}
		return ListT;
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

	public void SelectChara(int idx){
		if (!fightStart) {
			charaIdx = idx;
		}
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
	public List<int> charaJobs;
	public GroundController gc;
	public int ratio;
}

public struct CharaImageData{
	public Image image;
	public GroundController linkGc;
}

public enum SpecailEffectType{
	Reverse = 1,
	ExtraRatio = 2
}