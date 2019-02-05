using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSEController : MonoBehaviour
{
    public TweenColor light;
    public TweenColor colorLight;
    public TweenColor HealingLight;
    public bool init = false;
    bool isHealing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenLight(Color lightColor, Color colorTransparent, bool isShow, GroundType gType)
    {
        if (isShow)
        {
            light.Stop(Color.white);
            colorLight.Stop(lightColor);
            colorLight.SetFromAndTo(lightColor, lightColor * colorTransparent);
            light.SetFromAndTo(Color.white, colorTransparent);
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

    public void SetHealing(bool isHeal) {
        if (isHeal != isHealing)
        {
            if (isHeal)
            {
                HealingLight.PlayForward();
            }
            else
            {
                HealingLight.PlayReverse();
            }
        }

        isHealing = isHeal;
    }

    public void CloseLight()
    {
        light.gameObject.SetActive(false);
    }
}
