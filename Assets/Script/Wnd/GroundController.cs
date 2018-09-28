using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GroundController : MonoBehaviour
{
    int _groundRate;

    public GroundType _groundType;

    public GroundController matchController;

    [HideInInspector]
    public int _layer;

    [HideInInspector]
    public bool isActived;


    [HideInInspector]
    public bool raycasted;

	private bool isChanged;

    [HideInInspector]
    public UIPolygon image;

    GroundType defaultType;

    [SerializeField]
    Sprite[] GetSprites;

	private GroundType _prevType = GroundType.None;

	private GroundType _roundPrevType;

    private bool activeLock;

	private bool roundActLock;

	public bool testRaycasted;

    int goldRatio = 75;

	public delegate void PlusRatio(ExtraRatioData plusRatioData);

    public PlusRatio plusRatio;

	public delegate void OnShowed (GroundController groundController, int number);

	//避免回傳時清除了別的GroundSEController的委派，分成三個
	public OnShowed onShowedFst;

	public OnShowed onShowedSec;

	public OnShowed onShowedThr;

	public delegate void OnShowing (int ratio, GroundController groundController, int number);

	public OnShowing onShowingFst;

	public OnShowing onShowingSec;

	public OnShowing onShowingThr;

	public delegate void OnProtection (int targetJob);

	public OnProtection onProtection;

	[HideInInspector]
	public int charaJob;

	private int? extraGoldJob;

	private int? extraSilverJob;

	[HideInInspector]
	public enum ExtraType {
		Silver,
		Gold
	}

    // Use this for initialization
    void Awake()
    {
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
		isActived = isChanged = activeLock = raycasted = false;
		testRaycasted = false;
		extraGoldJob = null;
		extraSilverJob = null;
    }

    public void ResetType()
    {
        _groundType = defaultType;
		isActived = isChanged = activeLock = raycasted = false;
		charaJob = 0;
		onProtection = null;
		_layer = (int)_groundType == 0 ? 1 : 0;
		ResetSprite (_groundType);
		extraGoldJob = null;
		extraSilverJob = null;
    }

	public void ChangeSprite(GroundType type)
    {
        if (image != null)
        {
			if ((int)type == 99)
            {
                image.sprite = GetSprites[4];
            }
			else if ((int)type < 4)
            {
				image.sprite = GetSprites[(int)type];
            }
        }
    }

	public IEnumerator ChangeSpriteWait(int number)
	{
		if (image != null)
		{
			if (image.sprite == GetSprites[1])
			{
				Reversing (50, number);
				image.sprite = GetSprites[2];
			}
			else if (image.sprite == GetSprites[2])
			{
				Reversing (75, number);
				image.sprite = GetSprites[3];
			}
		}

		//避免同物件進行回調時回調被清掉或回調錯誤
		yield return new WaitForSeconds (0.75f);
		switch(number){
		case 1:
			if (onShowedFst != null) {
				onShowedFst.Invoke (this, number);
			}
			break;
		case 2:
			if (onShowedSec != null) {
				onShowedSec.Invoke (this, number);
			}
			break;
		case 3:
			if (onShowedThr != null) {
				onShowedThr.Invoke (this, number);
			}
			break;
		}
	}


	public void ChangeSprite(int number)
	{
		if (image != null)
		{
			if (image.sprite == GetSprites[1])
			{
				Reversing (50, number);
				image.sprite = GetSprites[2];
			}
			else if (image.sprite == GetSprites[2])
			{
				Reversing (75, number);
				image.sprite = GetSprites[3];
			}
		}

		//避免同物件進行回調時回調被清掉或回調錯誤
		switch(number){
		case 1:
			if (onShowedFst != null) {
				onShowedFst.Invoke (this, number);
			}
			break;
		case 2:
			if (onShowedSec != null) {
				onShowedSec.Invoke (this, number);
			}
			break;
		case 3:
			if (onShowedThr != null) {
				onShowedThr.Invoke (this, number);
			}
			break;
		}
	}

	private void Reversing(int ratio, int number){
		switch(number){
		case 1:
			if (onShowingFst != null) {
				onShowingFst.Invoke (ratio, this, number);
			}
			break;
		case 2:
			if (onShowingSec != null) {
				onShowingSec.Invoke (ratio, this, number);
			}
			break;
		case 3:
			if (onShowingThr != null) {
				onShowingThr.Invoke (ratio, this, number);
			}
			break;
		}
	}

	public void ResetSprite(GroundType type){
		matchController.ChangeSprite (type);
	}

	/// <summary>
	/// Sets the type.
	/// </summary>
	/// <param name="jobIdx">Job index.</param>
	/// <param name="hasPre">是否止執行AddJob<c>true</c> has pre.</param>
	public void SetType()
    {
		if (isActived) {
			activeLock = true;
			roundActLock = activeLock;
		}

		raycasted = false;
		isChanged = false;

		if ((int)_groundType == 0) {
			_layer = 1;
		} 
		else {
			_layer = 0;
		}
    }

	public void SetJob(int jobIdx, ExtraType extraType){
		if (extraType == ExtraType.Gold) {
			extraGoldJob = jobIdx;
		} else {
			extraSilverJob = jobIdx;
		}
	}

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
			_layer ++;
        }
    }

	public List<RaycastData> OnChangeType(bool isTouchUp, bool isEnd, bool isTest = false){
		return RaycastRound(false, isTouchUp, isEnd, isTest);	
	}

	public void OnPrevType(bool isEnd){
		RaycastRound (true, false, isEnd);
	}

	private Dictionary<int, List<RaycastData>> OnPrevType(RaycastHit2D[] hits, bool isEnd)
	{
		foreach (var hit in hits) {
			hit.collider.GetComponent<GroundController> ().PrevType (isEnd);
		}
        return null;
    }

	/// <summary>
	/// 向六方向射出射線進行偵測.
	/// </summary>
	/// <returns>The round.</returns>
	/// <param name="isPrev">是否還原狀態 <c>true</c> is previous.</param>
	/// <param name="isTouchUp">是否屏幕碰觸結束<c>true</c> is touch up.</param>
	/// <param name="isEnd">是否結束此回合<c>true</c> is end.</param>
	private List<RaycastData> RaycastRound(bool isPrev, bool isTouchUp, bool isEnd, bool isTest = false)
	{
		RaycastHit2D[] hits;
        List<RaycastData> dataList = new List<RaycastData> ();
		bool hasActived = false;
        for (int i = 0; i < 6; i++)
        {
            hits = GetRaycastHits(transform.localPosition, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

            if (hits.Length == 0) {
                continue;
            }

			bool hitNone = false;
			bool hasOcclusion = false;
            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
				if ((int)hits [j].transform.GetComponent<GroundController> ()._groundType > 0 && (int)hits [j].transform.GetComponent<GroundController> ()._groundType < 10) {
					hasOcclusion = true;
				}
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 0 || (int)hits[j].transform.GetComponent<GroundController>()._groundType == 99)
                {
					hitNone = true;
                }
                else if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 10)
                {
					if (hits[j].transform.GetComponent<GroundController>().charaJob != charaJob)
                    {
						if (onProtection != null && !hasOcclusion) {
							onProtection.Invoke (hits [j].transform.GetComponent<GroundController> ().charaJob);
						}
                    }
                    else
                    {
						if (!hitNone) {
							if (isPrev) {
								OnPrevType (hitGcs.ToArray (), isEnd);
							} else {

								int ratio = CalculateRatio (hitGcs.ToArray (), charaJob, isTouchUp, isEnd, isTest);

								if (ratio > 0) {
									RaycastData data = new RaycastData ();

									data.start = GetComponent<GroundController> ().matchController;
									data.end = hits [j].transform.GetComponent<GroundController> ().matchController;
									data.ratio = ratio;
									data.hits = new List<GroundController> ();
									if (charaJob != 0) {
										data.CharaJob = charaJob;
									}
									for (int h = 0; h < hitGcs.Count - 1; h++) {
										data.hits.Add (hitGcs [h].collider.GetComponent<GroundController> ().matchController);
									}
									dataList.Add (data);

									hasActived = true;
								}

								if (j > 0) {
									if (hits [j].collider.GetComponent<GroundController> ().isActived) {
										hasActived = true;
									}
								}
							}
						} 
						else {
							
						}

                        break;
                    }
                }
            }
        }

		if (!isPrev) {
			OnRaycasted (hasActived, isTouchUp);
		}

		return dataList;
	}

	//計算加成
	private int CalculateRatio(RaycastHit2D[] hits, int charaJob, bool isTouchUp, bool isEnd, bool isTest)
    {
        int extraRatio = 0;

        bool hasChange = !(hits[hits.Length - 1].collider.GetComponent<GroundController>().isActived == true && isActived == true);


        foreach (var hit in hits)
        {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
            {
				hit.collider.GetComponent<GroundController> ().raycasted = true;
				if (!hits [hits.Length - 1].collider.GetComponent<GroundController> ().raycasted || isTest) {
					if (hasChange || isTouchUp) {
						hit.collider.GetComponent<GroundController> ().ChangeType (isTouchUp, isEnd);
					}

					switch ((int)hit.collider.GetComponent<GroundController> ()._groundType) {
					case 1:
						//記錄回傳用資料
						if ((isTouchUp || isEnd)&& !hasChange) {
							hit.collider.GetComponent<GroundController> ().SetJob (charaJob, ExtraType.Silver);
						}
						break;
					case 2:
						extraRatio = extraRatio + 50;

						//記錄回傳用資料
						if (isTouchUp || isEnd) {
							hit.collider.GetComponent<GroundController> ().SetJob (charaJob, ExtraType.Gold);
						}
						break;
					case 3:
						extraRatio = extraRatio + goldRatio;
						break;
					}
				}
            }
        }

		return extraRatio;
    }

	//改變狀態，並記錄回朔狀態
	public void ChangeType(bool isTouchUp, bool isEnd)
    {
		if (isTouchUp && !isEnd) {
			_roundPrevType = _groundType;
			return;
		}

		if ((int)_groundType == 0) {
			_groundType = GroundType.Copper;
			matchController.ChangeSprite (_groundType);
		} else {

			if ((int)_groundType == 1) {
				_groundType = GroundType.Silver;

				if (isEnd) {
					OnPlusRatio (ExtraType.Silver);
				}
			} 
			else if ((int)_groundType == 2) {
				_groundType = GroundType.gold;

				if (isEnd) {
					OnPlusRatio (ExtraType.Gold);
				}
			}
			isChanged = true;
		}

		if (isEnd) {
			_prevType = _groundType;
			_roundPrevType = _groundType;
		} else {
			matchController.ChangeSprite (_groundType);
		}
		_layer = 0;
    }

	public void OnRaycasted(bool hasAcitve, bool isTouchUp)
    {
        if (hasAcitve)
        {
            isActived = true;
			if (isTouchUp) {
				roundActLock = true;
			}
        }
        raycasted = true;
    }

	public void ChangeChara(int job)
	{
		isChanged = true;
		_groundType = GroundType.Chara;
		charaJob = job;

		_layer = 0;
	}

	//回朔狀態，以是否為回合結束為基準;
	public void PrevType(bool isEnd) {
		if ((int)_groundType != 10) {
			if (isChanged) {
				if (isEnd) {
					_groundType = _prevType;
					isChanged = false;
					raycasted = false;
				} else {
					_groundType = _roundPrevType;
				}

				matchController.ChangeSprite (_groundType);
			}
		} 
		else {
			if (!isEnd) {
				if (!roundActLock) {
					isActived = false;
				}
			} else {
				if (!activeLock) {
					isActived = false;
				}
			}
		}
    }

	//覆蓋功能用
	public void OnCover(){
		_roundPrevType = _groundType;
		_groundType = defaultType;
		matchController.ChangeSprite (_groundType);

		_layer = 0;
	}

	//還原覆蓋用
	public void OnPrevCover(){
		if (isChanged) {
			_groundType = _prevType;
			isChanged = false;
			raycasted = false;

			isActived = false;
			matchController.ChangeSprite (_groundType);
		}
	}

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis)
    {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
    }

	//回傳額外傷害加成，回傳後移除回傳資料，避免重複回傳
	private void OnPlusRatio(ExtraType extraType) {
		if (extraType == ExtraType.Silver) {
			if (extraSilverJob != null) {
				Debug.Log ("Silver");
				ExtraRatioData data = new ExtraRatioData();

				data.gc = matchController;
				data.extraJob = (int)extraSilverJob;
				data.upRatio = 50;

				plusRatio.Invoke(data);
			}
		}
        else {
			if (extraGoldJob != null) {
				ExtraRatioData data = new ExtraRatioData ();
				Debug.Log ("Gold");
				data.gc = matchController;
				data.extraJob = (int)extraGoldJob;
				data.upRatio = 25;

				plusRatio.Invoke (data);

				if (extraSilverJob != null) {
					data = new ExtraRatioData ();
					Debug.Log ("Silve to Gold");
					data.gc = matchController;
					data.extraJob = (int)extraGoldJob;
					data.upRatio = 25;

					plusRatio.Invoke (data);
				}
			}
			extraGoldJob = null;
			extraSilverJob = null;
        }
    }
}
