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

	public delegate void PlusRatio(PlusRatioData plusRatioData);

    public PlusRatio plusRatio;

	public delegate void OnShowed (GroundController groundController);

	public OnShowed onShowed;

	public delegate void OnShowing (int ratio, GroundController groundController);

	public OnShowing onShowing;

	public delegate void OnProtection (int Guardian, int target);

	public OnProtection onProtection;

	[HideInInspector]
	public int charaJob;

	public GroundController pairGc;

	public bool isRuined;

    // Use this for initialization
    void Awake()
    {
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
		isActived = isChanged = activeLock = raycasted = isRuined = false;
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

	public void ChangeSprite()
	{
		if (image != null)
		{
			if (image.sprite == GetSprites[1])
			{
				Reversing (50);
				image.sprite = GetSprites[2];
			}
			else if (image.sprite == GetSprites[2])
			{
				Reversing (25);
				image.sprite = GetSprites[3];
			}
		}
		if (onShowed != null) {
			onShowed.Invoke (this);
		}
	}

	private void Reversing(int ratio){
		if (onShowing != null) {
			onShowing.Invoke (25, this);
		}
	}

	public void ResetSprite(GroundType type){
		matchController.ChangeSprite (type);
	}

	public void SetType(bool isNext)
    {
        if (isActived) {
            activeLock = true;
        }

		if (isChanged == true) {
			matchController.ChangeSprite (_prevType);
		}

        raycasted = false;
		isChanged = false;

		if (isNext) {
			isRuined = false;	
		}
		if ((int)_groundType == 0) {
			_layer = 1;
		} 
		else {
			_layer = 0;
		}

		/*foreach (var img in lightText) {
			img.gameObject.SetActive (false);
		}*/
    }

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
			_layer ++;
        }
    }

	public List<RaycastData> OnChangeType(){
		return RaycastRound();	
	}

	public void OnPrevType(){
		RaycastRound (true);
	}

	public void OnRuined(){
		isRuined = true;
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
							onProtection.Invoke ((int)charaIdx, (int)hits [j].transform.GetComponent<GroundController> ().charaIdx);
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
                if (!hits[hits.Length - 1].collider.GetComponent<GroundController>().raycasted)
                {
                    if (hasChange)
                    {
                        hit.collider.GetComponent<GroundController>().ChangeType(charaIdx);
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

    public void ChangeType(int? idx = null)
    {
		if (isChanged == false) {
			_prevType = _groundType;
		}

		if ((int)_groundType == 0) {
			_groundType = GroundType.Copper;
		} 
		else {
			if ((int)_groundType == 1) {
				charaIdx = idx;
				_groundType = GroundType.Silver;
			} else if ((int)_groundType == 2) {
				OnPlusRatio ();
				_groundType = GroundType.gold;
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

    private void OnPlusRatio() {
		PlusRatioData data = new PlusRatioData();
        data.gc = matchController;
        data.charaIdx = (int)charaIdx;

        plusRatio.Invoke(data);
    }
}
