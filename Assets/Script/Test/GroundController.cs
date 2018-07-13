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

	private bool activeChange;

    [HideInInspector]
    public bool raycasted;

	private bool isChanged;

    [HideInInspector]
    public UIPolygon image;

    GroundType defaultType;

    [SerializeField]
    Sprite[] GetSprites;

    [SerializeField]
    bool activeLock;

	public bool testRaycasted;
	 

    // Use this for initialization
    void Awake()
    {
        selfGc = GetComponent<GroundController>();
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
		isActived = activeChange = isChanged = activeLock = raycasted = false;
		testRaycasted = false;
    }

    // Update is called once per frame
    public void ResetType()
    {
        _groundType = defaultType;

		isActived = activeChange = isChanged = activeLock = raycasted = false;

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

	public List<RaycastData> OnChangeType(){
		return RaycastRound();	
	}

	public void OnPrevType(){
		RaycastRound (true);
	}




	private List<RaycastData> RaycastRound(bool isPrev = false)
	{
		RaycastHit2D[] hits;
		List<RaycastData> dataList = new List<RaycastData> ();
		bool hasActived = false;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits (transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

			List<RaycastHit2D> hitGcs = new List<RaycastHit2D> ();
			for (int j = 0; j < hits.Length; j++) {
				hitGcs.Add (hits [j]);
				if ((int)hits [j].transform.GetComponent<GroundController> ()._groundType == 10) {
                    if (hits[j].transform.GetComponent<GroundController>().charaIdx != charaIdx)
                    {
                        break;
                    }
                    else {
						if (j > 0) {
							RaycastData data = new RaycastData ();

							data.start = GetComponent<GroundController> ();
							data.end = hits [j].transform.GetComponent<GroundController> ();
							data.damage = CalculateDamage (hitGcs.ToArray (), isPrev);
							data.charaIdx = (int)charaIdx;
							dataList.Add (data);

							if (data.damage > 0) {
								hasActived = true;
								OnActived (hitGcs[hitGcs.Count-1].collider.GetComponent<GroundController>());
							}
						}
						break;
					}
				}
			}
		}

		if (hasActived) {
			OnActived ();
		}

		if (!isPrev) {
			OnRaycasted ();
		}
		return dataList;
	}


	private int CalculateDamage(RaycastHit2D[] hits, bool isPrev=false)
	{
		int extraDamage = 0;
		if (isPrev) {
			foreach (var hit in hits) {
				hit.collider.GetComponent<GroundController> ().PrevType ();
			}
		} else { 
			if (Array.TrueForAll (hits, HasDamage)) {
				bool hasChange = !(hits [hits.Length - 1].collider.GetComponent<GroundController> ().isActived == true && isActived == true);
				bool according = hits [hits.Length - 1].collider.GetComponent<GroundController> ().raycasted;
				foreach (var hit in hits) {
					if ((int)hit.collider.GetComponent<GroundController> ()._groundType != 10) {
						if (!according) {
							if (hasChange) {
								hit.collider.GetComponent<GroundController> ().ChangeType ();
							}

							switch ((int)hit.collider.GetComponent<GroundController> ()._groundType) {
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
			}
		}
		return extraDamage;
	}

    public void ChangeType()
    {
        if ((int)_groundType == 0)
        {
            _groundType = GroundType.Copper;
        }
        else if ((int)_groundType == 1)
        {
            _groundType = GroundType.Silver;
        }
        else if ((int)_groundType == 2)
        {
            _groundType = GroundType.gold;
        }

		isChanged = true;

		selfGc.matchController.ChangeSprite(_groundType);
        _layer = 0;
    }

	public void ChangeType(int? idx = null)
	{
		charaIdx = idx;

		_groundType = GroundType.Chara;

		isChanged = true;
		_layer = 0;
	}

	public void OnActived(GroundController lastHit=null){
		if (lastHit != null) {
			lastHit.OnActived ();
		} else {
			isActived = true;
			activeChange = true;
		}
	}

	public void OnRaycasted(){
		raycasted = true;
		isChanged = true;
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
            raycasted = false;

			if (!activeLock && activeChange)
            {
                isActived = false;
            }
			selfGc.matchController.ChangeSprite(_groundType);
		}
    }

    private bool HasDamage(RaycastHit2D hit)
    {
        if ((int)hit.collider.GetComponent<GroundController>()._groundType == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis)
    {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
    }

	#region TEST
	public void OnTestChangeType(){
		TestRaycastRound();	
	}

	private void TestRaycastRound()
	{
		RaycastHit2D[] hits;
		bool hasActived = false;
		for (int i = 0; i < 6; i++) {
			hits = GetRaycastHits (transform.localPosition, new Vector2 (Mathf.Sin (Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos (Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

			List<RaycastHit2D> hitGcs = new List<RaycastHit2D> ();
			for (int j = 0; j < hits.Length; j++) {
				hitGcs.Add (hits [j]);
				if ((int)hits [j].transform.GetComponent<GroundController> ()._groundType == 10) {
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
		if (Array.TrueForAll (hits, HasDamage)) {
			bool hasChange = !(hits [hits.Length - 1].collider.GetComponent<GroundController> ().isActived == true && isActived == true);
			bool according = hits [hits.Length - 1].collider.GetComponent<GroundController> ().testRaycasted;
			foreach (var hit in hits) {
				if ((int)hit.collider.GetComponent<GroundController> ()._groundType != 10) {
					if (!according) {

						switch ((int)hit.collider.GetComponent<GroundController> ()._groundType) {
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
		}
		return extraDamage;
	}

	#endregion
}
