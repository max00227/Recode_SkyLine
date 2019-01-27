using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyStone : MonoBehaviour
{
    public ParticleSystem[] particles;
    public TweenColor bg;

    public Color[] energyColor;

    public TweenColor useBg;

    bool isEmpty = false;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnergyCharge(bool init)
    {
        if (init) {
            isEmpty = false;
        }

        foreach (ParticleSystem ps in particles)
        {
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }

        useBg.gameObject.SetActive(false);

        if (init || !isEmpty) {
            bg.PlayForward();
        }
    }

    public void EneryEmpty(bool init)
    {
        if (init)
        {
            isEmpty = true;
        }

        foreach (ParticleSystem ps in particles)
        {
            if (ps.isPlaying)
            {
                if (ps.transform.name == "point")
                {
                    ps.Stop();
                }
                else
                {
                    ps.Pause();
                }
            }
        }

        useBg.gameObject.SetActive(false);
        if (!init || !isEmpty)
        {
            bg.PlayReverse();
        }
    }

    public void UseEnergy() {
        useBg.gameObject.SetActive(true);
        useBg.PlayForward();
    }

    public void CloseUseBg()
    {
        useBg.gameObject.SetActive(false);
    }
}
