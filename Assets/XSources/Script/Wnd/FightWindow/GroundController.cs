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

    public ParticleSystem particle;

    int constAngle;

    int _groundRate;

    public GroundType _groundType;

    public GroundController matchController;

    public TweenColor light;
    public TweenColor colorLight;

    [SerializeField]
    private Color[] lightColor;


    [HideInInspector]
    public int _layer;

    [HideInInspector]
    public bool isActived;


    [HideInInspector]
    public bool raycasted;

    private bool isChanged;

    GroundType defaultType;

    public Sprite[] GetSprites;

    private GroundType _prevType = GroundType.None;

    private GroundType _roundPrevType;

    private bool activeLock;

    private bool roundActLock;

    public bool testRaycasted;

    int goldRatio = 75;

    public delegate void PlusRatio(ExtraRatioData plusRatioData);

    public PlusRatio plusRatio;

    public delegate void OnShowed(GroundController groundController, int number);

    //避免回傳時清除了別的GroundSEController的委派，分成三個
    public OnShowed onShowedFst;

    public OnShowed onShowedSec;

    public OnShowed onShowedThr;

    public delegate void OnShowing(int ratio, GroundController groundController, int number);

    public OnShowing onShowingFst;

    public OnShowing onShowingSec;

    public OnShowing onShowingThr;

    public delegate void OnProtection(int targetJob);

    public OnProtection onProtection;

    public Dictionary<ExtraType, Dictionary<GroundController, GroundController>> linkData;

    [HideInInspector]
    public int charaJob;

    private int? extraGoldJob;

    private int? extraSilverJob;

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
        _layer = 1;
        isActived = isChanged = activeLock = raycasted = false;
        testRaycasted = false;
        extraGoldJob = null;
        extraSilverJob = null;
        linkData = new Dictionary<ExtraType, Dictionary<GroundController, GroundController>>();
    }

    public void ResetType()
    {
        _groundType = defaultType;
        isActived = isChanged = activeLock = raycasted = false;
        charaJob = 0;
        onProtection = null;
        _layer = (int)_groundType == 0 ? 1 : 0;
        ResetSprite(_groundType);
        extraGoldJob = null;
        extraSilverJob = null;
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

    public IEnumerator ChangeSpriteWait(int number)
    {
        if (background != null)
        {
            if (matchController._groundType == GroundType.Silver)
            {
                Reversing(50, number);
            }
            else if (matchController._groundType == GroundType.gold)
            {
                Reversing(75, number);
            }
            OpenLight(matchController._groundType,false);
        }

        //避免同物件進行回調時回調被清掉或回調錯誤
        yield return new WaitForSeconds(0.75f);
        switch (number)
        {
            case 1:
                if (onShowedFst != null)
                {
                    onShowedFst.Invoke(this, number);
                }
                break;
            case 2:
                if (onShowedSec != null)
                {
                    onShowedSec.Invoke(this, number);
                }
                break;
            case 3:
                if (onShowedThr != null)
                {
                    onShowedThr.Invoke(this, number);
                }
                break;
        }
    }


    public void ChangeSprite(int number)
    {
        if (background != null)
        {
            if (background.sprite == GetSprites[1])
            {
                Reversing(50, number);
                background.sprite = GetSprites[2];
            }
            else if (background.sprite == GetSprites[2])
            {
                Reversing(75, number);
                background.sprite = GetSprites[3];
            }
        }

        //避免同物件進行回調時回調被清掉或回調錯誤
        switch (number)
        {
            case 1:
                if (onShowedFst != null)
                {
                    onShowedFst.Invoke(this, number);
                }
                break;
            case 2:
                if (onShowedSec != null)
                {
                    onShowedSec.Invoke(this, number);
                }
                break;
            case 3:
                if (onShowedThr != null)
                {
                    onShowedThr.Invoke(this, number);
                }
                break;
        }
    }

    private void Reversing(int ratio, int number)
    {
        switch (number)
        {
            case 1:
                if (onShowingFst != null)
                {
                    onShowingFst.Invoke(ratio, this, number);
                }
                break;
            case 2:
                if (onShowingSec != null)
                {
                    onShowingSec.Invoke(ratio, this, number);
                }
                break;
            case 3:
                if (onShowingThr != null)
                {
                    onShowingThr.Invoke(ratio, this, number);
                }
                break;
        }
    }

    public void ResetSprite(GroundType type)
    {
        matchController.ChangeSprite(type);
    }

    /// <summary>
    /// Sets the type.
    /// </summary>
    /// <param name="jobIdx">Job index.</param>
    /// <param name="hasPre">是否止執行AddJob<c>true</c> has pre.</param>
    public void SetType()
    {

    }

    public void SetJob(int jobIdx, ExtraType extraType)
    {
        if (extraType == ExtraType.Gold)
        {
            extraGoldJob = jobIdx;
        }
        else
        {
            extraSilverJob = jobIdx;
        }
    }

    public void UpLayer()
    {
        if ((int)_groundType == 0)
        {
            _layer++;
        }
    }

    public List<RaycastData> OnChangeType(bool isTouchUp, bool isEnd, bool isTest = false)
    {
        return RaycastRound(false, isTouchUp, isEnd, isTest);
    }

    public void OnPrevType(bool isEnd)
    {
        RaycastRound(true, false, isEnd);
    }

    private Dictionary<int, List<RaycastData>> OnPrevType(RaycastHit2D[] hits, bool isEnd)
    {
        foreach (var hit in hits)
        {
            hit.collider.GetComponent<GroundController>().PrevType(isEnd);
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
        List<RaycastData> dataList = new List<RaycastData>();
        bool hasActived = false;
        for (int i = 0; i < 6; i++)
        {
            hits = GetRaycastHits(transform.position, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (constAngle + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (constAngle + i * 60))), 0.97f * 8);

            if (hits.Length == 0)
            {
                continue;
            }

            bool hitNone = false;
            bool hasOcclusion = false;
            List<RaycastHit2D> hitGcs = new List<RaycastHit2D>();
            for (int j = 0; j < hits.Length; j++)
            {
                hitGcs.Add(hits[j]);
                if ((int)hits[j].transform.GetComponent<GroundController>()._groundType > 0 && (int)hits[j].transform.GetComponent<GroundController>()._groundType < 10)
                {
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
                        if (onProtection != null && !hasOcclusion)
                        {
                            onProtection.Invoke(hits[j].transform.GetComponent<GroundController>().charaJob);
                        }
                    }
                    else
                    {
                        if (!hitNone)
                        {
                            if (isPrev)
                            {
                                OnPrevType(hitGcs.ToArray(), isEnd);
                            }
                            else
                            {

                                int ratio = CalculateRatio(hitGcs.ToArray(), charaJob, isTouchUp, isEnd, isTest);

                                if (ratio > 0)
                                {
                                    RaycastData data = new RaycastData();

                                    data.start = GetComponent<GroundController>().matchController;
                                    data.end = hits[j].transform.GetComponent<GroundController>().matchController;
                                    data.ratio = ratio;
                                    data.hits = new List<GroundController>();
                                    if (charaJob != 0)
                                    {
                                        data.CharaJob = charaJob;
                                    }
                                    for (int h = 0; h < hitGcs.Count - 1; h++)
                                    {
                                        data.hits.Add(hitGcs[h].collider.GetComponent<GroundController>().matchController);
                                    }
                                    dataList.Add(data);

                                    hasActived = true;
                                }

                                if (j > 0)
                                {
                                    if (hits[j].collider.GetComponent<GroundController>().isActived)
                                    {
                                        hasActived = true;
                                    }
                                }
                            }
                        }
                        else
                        {

                        }

                        break;
                    }
                }
            }
        }

        if (!isPrev)
        {
            OnRaycasted(hasActived, isTouchUp);
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
                hit.collider.GetComponent<GroundController>().raycasted = true;
                if (!hits[hits.Length - 1].collider.GetComponent<GroundController>().raycasted || isTest)
                {
                    bool isChange = hasChange || isTouchUp;
                    switch ((int)hit.collider.GetComponent<GroundController>()._groundType)
                    {
                        case 1:
                            if (isChange && isEnd)
                            {
                                hit.collider.GetComponent<GroundController>().SetExtraData(
                                    this.GetComponent<GroundController>(),
                                    hits[hits.Length - 1].collider.GetComponent<GroundController>(),
                                    ExtraType.Silver
                                );
                            }
                            //記錄回傳用資料
                            if ((isTouchUp || isEnd) && !hasChange)
                            {

                                hit.collider.GetComponent<GroundController>().SetJob(
                                    charaJob,
                                    ExtraType.Silver
                                );
                            }
                            break;
                        case 2:
                            if (isChange && isEnd)
                            {
                                hit.collider.GetComponent<GroundController>().SetExtraData(
                                    this.GetComponent<GroundController>(),
                                    hits[hits.Length - 1].collider.GetComponent<GroundController>(),
                                    ExtraType.Gold
                                );
                            }
                            //記錄回傳用資料
                            if (isTouchUp || isEnd)
                            {
                                hit.collider.GetComponent<GroundController>().SetJob(
                                    charaJob,
                                    ExtraType.Gold
                                );
                            }
                            break;
                    }


                    if (hasChange || isTouchUp)
                    {
                        extraRatio += hit.collider.GetComponent<GroundController>().ChangeType(isTouchUp, isEnd);
                    }
                    else
                    {
                        switch ((int)hit.collider.GetComponent<GroundController>()._groundType)
                        {
                            case 2:
                                extraRatio += 50;
                                break;
                            case 3:
                                extraRatio += goldRatio;
                                break;
                        }
                    }
                }
            }
        }
        return extraRatio;
    }

    //改變狀態，並記錄回朔狀態
    public int ChangeType(bool isTouchUp, bool isEnd)
    {
        int ratio = 0;
        if (isTouchUp && !isEnd)
        {
            _roundPrevType = _groundType;
            return ratio;
        }

        if ((int)_groundType == 0)
        {
            _groundType = GroundType.Copper;
            matchController.ChangeSprite(_groundType, true);
        }
        else
        {
            if ((int)_groundType == 1)
            {
                _groundType = GroundType.Silver;
                ratio = 50;
                if (isEnd)
                {
                    OnPlusRatio(ExtraType.Silver);
                }
            }
            else if ((int)_groundType == 2)
            {
                _groundType = GroundType.gold;
                ratio = goldRatio;
                if (isEnd)
                {
                    OnPlusRatio(ExtraType.Gold);
                }
            }
            isChanged = true;
        }

        if (isEnd)
        {
            _prevType = _groundType;
            _roundPrevType = _groundType;
        }
        else
        {
            matchController.ChangeSprite(_groundType);
        }
        _layer = 0;

        return ratio;
    }

    public void OnRaycasted(bool hasAcitve, bool isTouchUp)
    {
        if (hasAcitve)
        {
            isActived = true;
            if (isTouchUp)
            {
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
    public void PrevType(bool isEnd)
    {
        if ((int)_groundType != 10)
        {
            if (isChanged)
            {
                if (isEnd)
                {
                    _groundType = _prevType;
                    isChanged = false;
                    raycasted = false;
                }
                else
                {
                    _groundType = _roundPrevType;
                    matchController.ChangeSprite(_groundType, true);
                }
            }
        }
        else
        {
            if (!isEnd)
            {
                if (!roundActLock)
                {
                    isActived = false;
                }
            }
            else
            {
                if (!activeLock)
                {
                    isActived = false;
                }
            }
        }
    }

    //覆蓋功能用
    public void OnCover()
    {
        _roundPrevType = _groundType;
        _groundType = defaultType;
        matchController.ChangeSprite(_groundType);

        _layer = 0;
    }

    //還原覆蓋用
    public void OnPrevCover()
    {
        if (isChanged)
        {
            _groundType = _roundPrevType;
            isChanged = false;
            raycasted = false;

            isActived = false;
            matchController.ChangeSprite(_groundType);
        }
    }

    private RaycastHit2D[] GetRaycastHits(Vector2 org, Vector2 dir, float dis)
    {
        LayerMask mask = 1 << 8;
        RaycastHit2D[] hits = Physics2D.RaycastAll(org, dir, dis, mask);

        return hits;
    }

    //回傳額外傷害加成，回傳後移除回傳資料，避免重複回傳
    private void OnPlusRatio(ExtraType extraType)
    {
        if (extraType == ExtraType.Silver)
        {
            if (extraSilverJob != null)
            {
                ExtraRatioData data = new ExtraRatioData();

                data.gc = matchController;
                data.extraJob = (int)extraSilverJob;
                data.upRatio = 50;
                data.linkData = linkData[ExtraType.Silver];

                plusRatio.Invoke(data);
            }
        }
        else
        {
            if (extraGoldJob != null)
            {
                ExtraRatioData data = new ExtraRatioData();
                data.gc = matchController;
                data.extraJob = (int)extraGoldJob;
                data.linkData = linkData[ExtraType.Gold];
                data.upRatio = 25;

                plusRatio.Invoke(data);

                if (extraSilverJob != null)
                {
                    data = new ExtraRatioData();
                    data.gc = matchController;
                    data.linkData = linkData[ExtraType.Silver];
                    data.extraJob = (int)extraSilverJob;
                    data.upRatio = 50;

                    plusRatio.Invoke(data);
                }
            }
        }

    }

    public void SetExtraData(GroundController start, GroundController end, ExtraType extraType)
    {
        Dictionary<GroundController, GroundController> link = new Dictionary<GroundController, GroundController>();
        link.Add(start, end);
        //Debug.Log (this.name + " : " + extraType);
        linkData.Add(extraType, link);
    }

    public void FightEnd()
    {
        if (isActived)
        {
            activeLock = true;
            roundActLock = activeLock;
        }

        raycasted = false;
        isChanged = false;


        if ((int)_groundType == 0)
        {
            _layer = 1;
        }
        else
        {
            _layer = 0;
        }
        extraGoldJob = null;
        extraSilverJob = null;
        linkData = new Dictionary<ExtraType, Dictionary<GroundController, GroundController>>();
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
            matchController.light.PlayForward(idx);
            matchController.colorLight.PlayForward(idx);
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
            hits = GetRaycastHits(transform.position, new Vector2(Mathf.Sin(Mathf.Deg2Rad * (constAngle + i * 60)), Mathf.Cos(Mathf.Deg2Rad * (constAngle + i * 60))), 0.97f * 8);
            for (int j = 1; j < hits.Length; j++)
            {
                hits[j].transform.tag = "raycastGCorner";
            }
        }
    }
}