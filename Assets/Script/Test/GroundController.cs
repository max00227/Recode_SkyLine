using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    int _groundRate;

    public GroundType _groundType;

    public GroundController matchController;

    GroundController selfGc;

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

    
    private bool activeLock;

	public bool testRaycasted;

    int goldDamage = 75;

    public delegate void PlusDamage(PlusDamageData plusDamageData);

    public PlusDamage plusDamage;

    // Use this for initialization
    void Awake()
    {
        selfGc = GetComponent<GroundController>();
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

		_layer = (int)_groundType == 0 ? 1 : 0;

        if (image != null)
        {
			ChangeSprite(_groundType);
        }

		testRaycasted = false;
    }

	public void ChangeSprite(GroundType type)
    {
		//Debug.Log (gameObject.name+", ChangeSprite : " + type);
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

    public void SetType()
    {
        if (isActived) {
            activeLock = true;
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

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
            _layer++;
        }
    }

	public Dictionary<int, List<RaycastData>> OnChangeType(){
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

    private Dictionary <int,List<RaycastData>> RaycastRound(bool isPrev = false)
	{
		RaycastHit2D[] hits;
        Dictionary<int, List<RaycastData>> dataDic = new Dictionary<int, List<RaycastData>>();
        List<RaycastData> dataList = new List<RaycastData> ();
		bool hasActived = false;
        for (int i = 0; i < 6; i++)
        {
            hits = GetRaycastHits(transform.localPosition, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 0)
                {
                    break;
                }
                else if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 10)
                {
                    if (hits[j].transform.GetComponent<GroundController>().charaIdx != charaIdx)
                    {
                        break;
                    }
                    else
                    {
                        if (j > 0)
                        {
                            if (isPrev)
                            {
                                OnPrevType(hitGcs.ToArray());
                            }
                            else
                            {
                                int damage = CalculateDamage(hitGcs.ToArray());

                                if (damage > 0)
                                {
                                    RaycastData data = new RaycastData();

                                    data.start = GetComponent<GroundController>();
                                    data.end = hits[j].transform.GetComponent<GroundController>();
                                    data.damage = damage;
                                    dataList.Add(data);

                                    hasActived = true;
                                }

                                if (hitGcs[hitGcs.Count - 1].collider.GetComponent<GroundController>().isActived)
                                {
                                    hasActived = true;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        if (dataList.Count > 0) {
            dataDic.Add((int)charaIdx, dataList);
        }
		if (!isPrev) {
            OnRaycasted (hasActived);
		}

		return dataDic;
	}

    private int CalculateDamage(RaycastHit2D[] hits)
    {
        int extraDamage = 0;

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

                    switch ((int)hit.collider.GetComponent<GroundController>()._groundType)
                    {
                        case 2:
                            extraDamage = extraDamage + 50;
                            break;
                        case 3:
                            extraDamage = extraDamage + goldDamage;
                            break;
                    }
                }
            }
        }
        return extraDamage;
    }

    public void ChangeType(int? idx = null)
    {
        if ((int)_groundType == 0)
        {
            _groundType = GroundType.Copper;
        }
        else if ((int)_groundType == 1)
        {
            charaIdx = idx;
            _groundType = GroundType.Silver;
        }
        else if ((int)_groundType == 2)
        {
            OnPlusDamage();
            _groundType = GroundType.gold;
        }

		isChanged = true;

		selfGc.matchController.ChangeSprite(_groundType);
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

    public void ChangeChara(int? idx = null)
	{
		charaIdx = idx;

		_groundType = GroundType.Chara;

		isChanged = true;
		_layer = 0;
	}

    public void PrevType() {
		if (isChanged) {
			switch ((int)_groundType) {
			case 3:
				_groundType = GroundType.Silver;
				break;
			case 2:
				_groundType = GroundType.Copper;
				break;
			}
			isChanged = false;

			selfGc.matchController.ChangeSprite(_groundType);
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

    private void OnPlusDamage() {
        PlusDamageData data = new PlusDamageData();
        data.gc = matchController;
        data.charaIdx = (int)charaIdx;

        plusDamage.Invoke(data);
    }

	#region TEST
	public void OnTestChangeType(){
		TestRaycastRound();	
	}

	private void TestRaycastRound()
	{
		RaycastHit2D[] hits;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits (transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

			List<RaycastHit2D> hitGcs = new List<RaycastHit2D> ();
			for (int j = 0; j < hits.Length; j++) {
				hitGcs.Add (hits [j]);
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 0) {
                    break;
                }
				else if ((int)hits [j].transform.GetComponent<GroundController> ()._groundType == 10) {
					if (hits[j].transform.GetComponent<GroundController>().charaIdx != charaIdx)
					{
						break;
					}
					else {
						if (j > 0) {

							int damage = TestCalculateDamage (hitGcs.ToArray ());
							if (damage > 0) {
								Debug.Log (gameObject.name + " , " + hitGcs [hitGcs.Count - 1].collider.name + " : " + damage);
							}
						}
						break;
					}
				}
			}
		}
			
		OnTestRaycasted ();
	}

	public void OnTestRaycasted(){
		testRaycasted = true;
	}

    private int TestCalculateDamage(RaycastHit2D[] hits)
    {
        int extraDamage = 0;
        bool hasChange = !(hits[hits.Length - 1].collider.GetComponent<GroundController>().isActived == true && isActived == true);
        bool according = hits[hits.Length - 1].collider.GetComponent<GroundController>().testRaycasted;
        foreach (var hit in hits)
        {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
            {
                if (!according)
                {

                    switch ((int)hit.collider.GetComponent<GroundController>()._groundType)
                    {
                        case 2:
                            extraDamage = extraDamage + 50;
                            break;
                        case 3:
                            extraDamage = extraDamage + 75;
                            break;
                    }
                }
            }
        }
        return extraDamage;
    }

	#endregion
}
