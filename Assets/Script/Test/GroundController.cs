using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    int _groundRate;

    public GroundType _groundType;

    public GroundController matchController;

    public GroundController selfGc;

    public int? charaIdx;

    [HideInInspector]
    public int _layer;

    [HideInInspector]
    public bool isActived;

    [HideInInspector]
    public UIPolygon image;

    GroundType defaultType;

    GroundType? prevType;

    [SerializeField]
    Sprite[] GetSprites;

    int typeIdx;

    public delegate void RaycastRound();

    public event RaycastRound onRaycastRound;

    // Use this for initialization
    void Start()
    {
        selfGc = GetComponent<GroundController>();
        defaultType = _groundType;
        image = GetComponent<UIPolygon>();
        _layer = 1;
        prevType = null;
        isActived = false;
        typeIdx = 1;
    }

    // Update is called once per frame
    public void ResetType(bool isResetGround = false)
    {
        if (gameObject.name == "29")
        {
            Debug.Log("Reset");
        }
        if (isResetGround)
        {
            _groundType = defaultType;
            typeIdx = 1;
            prevType = null;
        }
        else
        {
            if (prevType != null)
            {
                _groundType = (GroundType)prevType;
                typeIdx = (int)_groundType + 1;
                prevType = null;
            }
        }
        charaIdx = null;
        _layer = 1;

        if (image != null)
        {
            ChangeSprite();
        }
    }

    public void ChangeType(GroundType type = GroundType.None, int? idx = null)
    {
        charaIdx = idx;

        if ((int)_groundType != 0 || (int)type != 0)
        {
            prevType = _groundType;
        }
        if (type == GroundType.None)
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
        }
        else
        {
            _groundType = type;
        }

        if (image != null)
        {
            ChangeSprite();
        }
        _layer = 0;
    }

    public void ChangeSprite()
    {
        if (image != null)
        {
            if ((int)_groundType == 99)
            {
                image.sprite = GetSprites[4];
            }
            else if ((int)_groundType < 4)
            {
                image.sprite = GetSprites[(int)_groundType];
            }
        }
    }

    public void SetType()
    {
        prevType = null;
    }

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
            _layer++;
        }
    }


    private List<RaycastData> Raycast()
    {
        if (image == null)
        {
            RaycastHit2D[] hits;
            List<RaycastData> dataList = new List<RaycastData>();
            for (int i = 0; i < 6; i++)
            {
                hits = GetRaycastHits(transform.localPosition, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (30 + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (30 + i * 60))), 0.97f * 8);

                List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
                for (int j = 0; j < hits.Length; j++)
                {
                    hitGcs.Add(hits[j]);
                    if ((int)hits[j].transform.GetComponent<GroundController>()._groundType == 10)
                    {
                        if (hits[j].transform.GetComponent<GroundController>().charaIdx == charaIdx)
                        {
                            if (j > 0)
                            {
                                RaycastData data = new RaycastData();
                                data.start = GetComponent<GroundController>();
                                data.end = hits[j].transform.GetComponent<GroundController>();
                                data.damage = CalculateDamage(hitGcs.ToArray());

                                dataList.Add(data);
                            }
                            break;
                        }
                    }
                }
            }
            return dataList;
        }

        return null;
    }

    private int CalculateDamage(RaycastHit2D[] hits)
    {
        int extraDamage = 0;

        if (Array.TrueForAll(hits, HasDamage))
        {
            foreach (var hit in hits)
            {
                if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
                {
                    if (isActived)
                    {
                        ChangeType();
                    }

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

    public void ChangeType()
    {
        if (_groundType == GroundType.None)
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
        }

        selfGc.matchController.ChangeSprite();
        _layer = 0;
    }

    public void PrevType() {
        switch ((int)_groundType) {
            case 10:
                _groundType = GroundType.None;
                charaIdx = null;
                break;
            case 3:
                _groundType = GroundType.Silver;
                break;
            case 2:
                _groundType = GroundType.Copper;
                break;
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
}
