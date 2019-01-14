using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenUI : MonoBehaviour
{

    [HideInInspector]
    public GameObject showGameObject;

    public bool isLoop;

    public AnimationCurve[] animationCurves;

    [HideInInspector]
    public AnimationCurve mainAniCurve;

    [HideInInspector]
    public float oriTime = 0;
    [HideInInspector]
    public float recTime;

    [HideInInspector]
    public bool isRun = false;

    [HideInInspector]
    public bool runForward = true;

    [SerializeField]
    public float TweenTime = 1;

    // Use this for initialization
    public void PlayForward()
    {
        Play(true);
    }

    public void PlayReverse()
    {
        Play(false);
    }



    void Play(bool isForward)
    {
        runForward = isForward;
        recTime = Time.realtimeSinceStartup;
        isRun = true;
    }
}
