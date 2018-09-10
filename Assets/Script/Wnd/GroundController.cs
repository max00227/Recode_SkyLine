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

    public int? charaIdx;

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

	public GroundType _prevType = GroundType.None;

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

	public delegate void OnProtection (int target);

	public OnProtection onProtection;

	[HideInInspector]
	public int charaJob;

	private List<int> charaJobs;

	public GroundController pairGc;

    // Use this for initialization
    void Awake()
    {
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
		isActived = isChanged = activeLock = raycasted = false;
		testRaycasted = false;
    }

    // Update is called once per frame
    public void ResetType()
    {
        _groundType = defaultType;

		isActived = isChanged = activeLock = raycasted = false;

        charaIdx = null;

		charaJob = 0;

		onProtection = null;

		_layer = (int)_groundType == 0 ? 1 : 0;

		ResetSprite (_groundType);

		pairGc = null;

		testRaycasted = false;

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

	public List<RaycastData> OnChangeType(){
		charaJobs = new List<int> ();
		return RaycastRound();	
	}

	public void OnPrevType(){
		RaycastRound (true);
	}

    private Dictionary<int, List<RaycastData>> OnPrevType(RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            hit.collider.GetComponent<GroundController>().PrevType();
        }

        return null;
    }

	private List<RaycastData> RaycastRound(bool isPrev = false)
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
            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 0 || (int)hits[j].transform.GetComponent<GroundController>()._groundType == 99)
                {
					hitNone = true;
                }
                else if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 10)
                {
					
					if (hits[j].transform.GetComponent<GroundController>().charaJob != charaJob)
                    {
						if (onProtection != null) {
							onProtection.Invoke ((int)hits [j].transform.GetComponent<GroundController> ().charaIdx);
						}
                    }
                    else
                    {
                        if (isPrev)
                        {
                            OnPrevType(hitGcs.ToArray());
                        }
                        else
                        {
							if (!hitNone) {
								int ratio = CalculateRatio (hitGcs.ToArray ());

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

    private int CalculateRatio(RaycastHit2D[] hits)
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
                    if (hasChange)
                    {
                        hit.collider.GetComponent<GroundController>().ChangeType();
                    }

					switch ((int)hit.collider.GetComponent<GroundController> ()._groundType) {
					case 2:
						extraRatio = extraRatio + 50;
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

	public void ChangeType()
    {
		if (isChanged == false) {
			_prevType = _groundType;
		}

		if ((int)_groundType == 0) {
			_groundType = GroundType.Copper;
		} 
		else {
			int upRatio = 0;
			if ((int)_groundType == 1) {
				upRatio = 50;
				_groundType = GroundType.Silver;
			} else if ((int)_groundType == 2) {
				upRatio = 25;
				_groundType = GroundType.gold;
				if (raycasted == true) {
					OnPlusRatio (upRatio);
				}
			}

			isChanged = true;
		}
			
		matchController.ChangeSprite(_groundType);
        _layer = 0;
    }

    public void OnRaycasted(bool hasAcitve)
    {
        if (hasAcitve)
        {
            isActived = true;
        }
        raycasted = true;
    }

	public void ChangeChara(int idx, int job, GroundController pair, bool isAct = false)
	{
		charaIdx = idx;
		_groundType = GroundType.Chara;
		charaJob = job;

		isActived = isAct;

		pairGc = pair;

		_layer = 0;
	}

    public void PrevType() {
		if (isChanged) {
			_groundType = _prevType;

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

	private void OnPlusRatio(int ratio) {
		ExtraRatioData data = new ExtraRatioData();
        data.gc = matchController;
		data.charaJobs = charaJobs;
		data.ratio = ratio;


        plusRatio.Invoke(data);
    }
}
