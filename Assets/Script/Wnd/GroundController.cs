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

	private GroundType _prevCrossType;

	[HideInInspector]
	public bool isCross = false;

    private bool activeLock;

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

	private List<int> charaJobs;

	public GroundController pairGc;

	bool isPrevLock;

	bool lockPrev;

    // Use this for initialization
    void Awake()
    {
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
		isActived = isChanged = activeLock = raycasted = isPrevLock = isCross = lockPrev = false;
		testRaycasted = false;
    }

    // Update is called once per frame
    public void ResetType()
    {
        _groundType = defaultType;
		isActived = isChanged = activeLock = raycasted = isPrevLock = isCross = lockPrev = false;
		charaJob = 0;
		onProtection = null;
		_layer = (int)_groundType == 0 ? 1 : 0;
		ResetSprite (_groundType);
		pairGc = null;
		charaJobs = new List<int> ();
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
				//連線重疊避免回朔圖片錯誤
				if ((int)type == 3) {
					if (image.sprite != GetSprites [(int)type]) {
						matchController.isCross = true;
						matchController._prevCrossType = GroundType.Silver;
					}
				}
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
		}

		if (isChanged) {
			matchController.ChangeSprite (_prevType);
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

	public void SetJob(int jobIdx){
		if ((int)_groundType > 0 && (int)_groundType < 10) {
			AddJob (jobIdx);
		}
	}

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
			_layer ++;
        }
    }

	public List<RaycastData> OnChangeType(bool isEnd){
		charaJobs = new List<int> ();
		return RaycastRound(false, isEnd);	
	}

	public void OnPrevType(bool isInit = false){
		RaycastRound (true, isInit);
	}

	private Dictionary<int, List<RaycastData>> OnPrevType(RaycastHit2D[] hits, bool isEnd)
	{
		if (!isPrevLock) {
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController> ().PrevType (isEnd);
			}
		}
        return null;
    }

	private List<RaycastData> RaycastRound(bool isPrev = false, bool isEnd = false)
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
							} 
							else {
								int ratio = CalculateRatio (hitGcs.ToArray (), charaJob, isEnd);

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

                        break;
                    }
                }
            }
        }

		if (!isPrev) {
            OnRaycasted (hasActived);
		}

		return dataList;
	}

	private int CalculateRatio(RaycastHit2D[] hits, int charaJob, bool isEnd)
    {
        int extraRatio = 0;

        bool hasChange = !(hits[hits.Length - 1].collider.GetComponent<GroundController>().isActived == true && isActived == true);

        foreach (var hit in hits)
        {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
            {
				hit.collider.GetComponent<GroundController> ().raycasted = true;
                if (!hits[hits.Length - 1].collider.GetComponent<GroundController>().raycasted)
                {
					if (hasChange && !isPrevLock)
                    {
						
						hit.collider.GetComponent<GroundController>().ChangeType(isEnd);
                    }

					switch ((int)hit.collider.GetComponent<GroundController> ()._groundType) {
					case 2:
						extraRatio = extraRatio + 50;
						if (isEnd) {
							hit.collider.GetComponent<GroundController>().SetJob (charaJob);
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

	public void ChangeType(bool isEnd = false)
    {
		if (isChanged == false) {
			_prevType = _groundType;
		}

		if ((int)_groundType == 0) {
			_groundType = GroundType.Copper;
		} 
		else {
			if ((int)_groundType == 1) {
				_groundType = GroundType.Silver;
			} else if ((int)_groundType == 2) {
				_groundType = GroundType.gold;

				if (isEnd) {
					lockPrev = true;
				}
				if (raycasted == true) {
					OnPlusRatio ();
				}
			}

			isChanged = true;
		}
			
		matchController.ChangeSprite(_groundType);
        _layer = 0;
    }

	public void OnPrevLock(bool unlock = false){
		isPrevLock = !unlock;
	}

    public void OnRaycasted(bool hasAcitve)
    {
        if (hasAcitve)
        {
            isActived = true;
        }
        raycasted = true;
    }

	public void ChangeChara(int job, GroundController pair, bool isAct = false)
	{
		_groundType = GroundType.Chara;
		charaJob = job;

		isActived = isAct;

		pairGc = pair;

		_layer = 0;
	}

	public void PrevType(bool isEnd) {
		if (isChanged && !lockPrev) {
			if (!isCross || isEnd) {
				_groundType = _prevType;
			} 
			else {
				_groundType = _prevCrossType;
				isCross = false;
			}

			isChanged = false;

			matchController.ChangeSprite(_groundType);
		}

        raycasted = false;

        if (!activeLock)
        {
            isActived = false;
        }
    }

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis)
    {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
    }

	public void AddJob(int job){
		if (!charaJobs.Contains (job)) {
			charaJobs.Add (job);
		}
	}

	private void OnPlusRatio() {
		ExtraRatioData data = new ExtraRatioData();
        data.gc = matchController;
		data.charaJobs = charaJobs;


        plusRatio.Invoke(data);
    }
}
