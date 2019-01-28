using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GroundController : MonoBehaviour
{
    [SerializeField]
    bool isPortrait = false;

    public Image background;

    int constAngle;

    public GroundType _groundType;
    
    public TweenColor light;
    public TweenColor colorLight;

    [SerializeField]
    private Color[] lightColor;

    [HideInInspector]
    public bool raycasted;

    GroundType defaultType;

    bool isChanged;

    public Sprite[] GetSprites;

    private GroundType _prevType = GroundType.None;

    private GroundType _roundPrevType;
    int goldRatio = 75;

    public delegate void PlusGroundType(GroundType groundType, bool useEnergy);

    public PlusGroundType plusGroundType;

    //[HideInInspector]
    public bool isChara;

    private Color colorTransparent = new Color(1, 1, 1, 0);
    
    public int groundRow;


    [HideInInspector]
    public enum ExtraType
    {
        Silver,
        Gold
    }

    // Use this for initialization
    void Awake()
    {
        constAngle = isPortrait == true ? 0 : 30;
        defaultType = _groundType;
        raycasted = false;
    }

    public void ResetType(bool init= false)
    {
        if (init)
        {
            _groundType = defaultType;
        }
        raycasted = false;
        isChara = false;
        ResetSprite(_groundType);
    }

    public void ChangeSprite(GroundType type, bool isSpeed = false)
    {
        if (background != null)
        {
            if ((int)type == 99)
            {
                background.sprite = GetSprites[4];
            }
            else if ((int)type == 0)
            {
                light.Stop(Color.white);
                background.sprite = GetSprites[0];
                light.gameObject.SetActive(false);
            }
            else
            {
                background.sprite = GetSprites[0];
                light.gameObject.SetActive(true);

                light.SetFromAndTo(Color.white, colorTransparent);
                light.PlayForward(System.Convert.ToInt32(!isSpeed));
                colorLight.SetFromAndTo(lightColor[(int)type - 1], (lightColor[(int)type - 1] * colorTransparent));
                colorLight.PlayForward(System.Convert.ToInt32(!isSpeed));
            }
        }
    }


    public void ChangeSprite(int number)
    {
        if (background != null)
        {
            if (background.sprite == GetSprites[1])
            {
                background.sprite = GetSprites[2];
            }
            else if (background.sprite == GetSprites[2])
            {
                background.sprite = GetSprites[3];
            }
        }
    }

   

    public void ResetSprite(GroundType type)
    {
        ChangeSprite(type);
    }

    /// <summary>
    /// Sets the type.
    /// </summary>
    /// <param name="jobIdx">Job index.</param>
    /// <param name="hasPre">是否止執行AddJob<c>true</c> has pre.</param>
    public void SetType()
    {
        if (isChara) {
            _groundType = GroundType.Chara;
        }
        _prevType = _groundType;
    }

    public void OnChangeType(bool isTouchUp)
    {
        RaycastRound(false, isTouchUp);
    }

    public void OnPrevType()
    {
        RaycastRound(true, false);
    }

    private void OnPrevType(RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            hit.collider.GetComponent<GroundController>().PrevType();
        }
    }

    /// <summary>
    /// 向六方向射出射線進行偵測.
    /// </summary>
    /// <returns>The round.</returns>
    /// <param name="isPrev">是否還原狀態 <c>true</c> is previous.</param>
    /// <param name="isTouchUp">是否屏幕碰觸結束<c>true</c> is touch up.</param>
    /// <param name="isEnd">是否結束此回合<c>true</c> is end.</param>
    private void RaycastRound(bool isPrev, bool isTouchUp)
    {
        RaycastHit2D[] hits;
        List<RaycastData> dataList = new List<RaycastData>();
        for (int i = 0; i < 6; i++)
        {
            hits = transform.parent.GetComponent<GroundRaycastController>().GetRaycastHits (transform.position, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (constAngle + constAngle + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (constAngle + constAngle + i * 60))), 116f * 8);

            if (hits.Length == 0)
            {
                continue;
            }

            bool hasOcclusion = false;
            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();

            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
                if (hits[j].transform.GetComponent<GroundController>().isChara)
                {
                    if (isPrev)
                    {
                        OnPrevType(hitGcs.ToArray());
                    }
                    else
                    {
                        CalculateRatio(hitGcs.ToArray(), isTouchUp);
                    }

                    break;
                }
            }
        }

        if (!isPrev)
        {
            raycasted = true;
        }
    }

    //計算加成
    private void CalculateRatio(RaycastHit2D[] hits, bool isTouchUp)
    {
        foreach (var hit in hits)
        {
            if ((int)hit.collider.GetComponent<GroundController>()._groundType != 10)
            {
                hit.collider.GetComponent<GroundController>().raycasted = true;
                if (!hits[hits.Length - 1].collider.GetComponent<GroundController>().raycasted)
                {
                    hit.collider.GetComponent<GroundController>().ChangeType(isTouchUp);
                }
            }
        }
    }

    //改變狀態，並記錄回朔狀態
    public void ChangeType(bool isTouchUp, bool useEnergy = true)
    {
        if (!isTouchUp)
        {
            _prevType = _groundType;
        }

        if ((int)_groundType == 0)
        {
            _groundType = GroundType.Copper;
            ChangeSprite(_groundType, true);
            plusGroundType.Invoke(_groundType, useEnergy);
        }
        else
        {
            if ((int)_groundType == 1)
            {
                _groundType = GroundType.Silver;
                plusGroundType.Invoke(_groundType, useEnergy);
            }
            else if ((int)_groundType == 2)
            {
                _groundType = GroundType.gold;
                plusGroundType.Invoke(_groundType, useEnergy);
            }
            isChanged = true;
        }

        if (isTouchUp)
        {
            _prevType = _groundType;
        }
        
        ChangeSprite(_groundType);
    }

    public void OnRaycasted(bool hasAcitve, bool isTouchUp)
    {
        raycasted = true;
    }

    public void ChangeChara()
    {
        isChanged = true;
        //_groundType = GroundType.Chara;
        isChara = true;
    }

    //回朔狀態，以是否為回合結束為基準;
    public void PrevType()
    {
        isChara = false;
        raycasted = false;

        if ((int)_groundType != 10)
        {
            _groundType = _prevType;
            ChangeSprite(_groundType, true);
        }
    }

    //覆蓋功能用
    public void OnCover()
    {
        _roundPrevType = _groundType;
        _groundType = defaultType;
        ChangeSprite(_groundType);
    }

    //還原覆蓋用
    public void OnPrevCover()
    {
        if (isChanged)
        {
            _groundType = _roundPrevType;
            isChanged = false;
            raycasted = false;

            ChangeSprite(_groundType);
        }
    }

    public void FightEnd()
    {
        raycasted = false;
        isChanged = false;
    }

    public void OpenLight(GroundType gType = GroundType.None, bool isShow = true)
    {
        if (isShow)
        {
            light.Stop(Color.white);
            colorLight.Stop(lightColor[(int)gType - 1]);
            colorLight.SetFromAndTo(lightColor[(int)gType - 1], lightColor[(int)gType - 1] * colorTransparent);
            light.SetFromAndTo(Color.white, colorTransparent);
            light.PlayForward(0);
            colorLight.PlayForward(0);
        }
        else
        {
            if (gType != GroundType.None)
            {
                colorLight.Stop(lightColor[(int)gType - 1]);
            }
            else {
                colorLight.Stop(lightColor[(int)_groundType - 1]);
            }
            light.Stop(Color.white);
            light.gameObject.SetActive(true);
        }
    }

    public void ResetTemple(int idx = 0) {
        if (_groundType != GroundType.Caution && _groundType != GroundType.None && _groundType != GroundType.Chara)
        {
            light.PlayForward(idx);
            colorLight.PlayForward(idx);
        }
    }

    public void CloseLight()
    {
        light.gameObject.SetActive(false);
    }

    public void SetTag()
    {
        RaycastHit2D[] hits;
        for (int i = 0; i < 6; i++)
        {
            hits = transform.parent.GetComponent<GroundRaycastController>().GetRaycastHits(transform.position, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (constAngle + constAngle + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (constAngle + constAngle + i * 60))), 116f * 8);
            for (int j = 1; j < hits.Length; j++)
            {
                hits[j].transform.tag = "raycastGCorner";
            }
        }
    }
}