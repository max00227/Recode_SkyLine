using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSEController : MonoBehaviour
{
    public TweenColor light;
    public TweenColor colorLight;
    public TweenColor spcColor;
    public TweenColor spcLight;

    public bool init = false;
    bool isHealing = false;

    SpecailGround specailType = SpecailGround.None;

    // Start is called before the first frame update
    private void Awake()
    {
    }

    void Start()
    {
        specailType = SpecailGround.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenLight(Color lightColor, bool isShow, GroundType gType)
    {
        if (isShow)
        {
            light.Stop(Color.white);
            colorLight.Stop(lightColor);
            colorLight.SetFromAndTo(lightColor, lightColor * Const.colorHalfTransparent);
            light.SetFromAndTo(Color.white, Const.colorHalfTransparent);
            light.PlayForward(0);
            colorLight.PlayForward(0);
        }
        else
        {
            if (gType != GroundType.None)
            {
                colorLight.Stop(lightColor);
            }
            else
            {
                colorLight.Stop(lightColor);
            }
            light.Stop(Color.white);
            light.gameObject.SetActive(true);
        }
    }

    public void ResetTemple(GroundType gType,int idx = 0)
    {
        if (gType != GroundType.Caution && gType != GroundType.None && gType != GroundType.Chara)
        {
            light.PlayForward(idx);
            colorLight.PlayForward(idx);
        }
    }

    public void SetSpecial(SpecailGround sg) {
        if (sg != specailType)
        {
            Color sc = SpecialColor(sg);
            if (sc != Color.clear)
            {
                if (specailType == SpecailGround.None)
                {
                    spcColor.SetFromAndTo(sc * Const.colorTransparent, sc);
                }
                else {
                    spcColor.SetFromAndTo(SpecialColor(specailType), sc);
                }
                spcColor.PlayForward();
                spcLight.PlayForward();
            }
            else
            {
                spcColor.PlayReverse();
                spcLight.PlayReverse();
            }
        }

        specailType = sg;
    }

    private Color SpecialColor(SpecailGround sg)
    {
        switch (sg) {
            case SpecailGround.Heal:
                return Color.white;
            case SpecailGround.Physical:
                return new Color(1f, 0.27f, 0.27f, 1f);
            case SpecailGround.Magic:
                return new Color(0.275f, 0.443f, 1f, 1f);
            default:
                return Color.clear;
        }
    }

    public void CloseLight()
    {
        light.gameObject.SetActive(false);
    }
}
