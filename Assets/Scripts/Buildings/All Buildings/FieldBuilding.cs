using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBuilding : Building
{
    [SerializeField] float incomeInterval;
    [SerializeField] int incomeAmount;
    float interval = 0f;

    [SerializeField] ParticleSystem collectEffect;

    private void Update()
    {
        interval += Time.deltaTime;
        if(interval >= incomeInterval)
        {
            interval -= incomeInterval;
            RessourceManager.instance.AddCoin(incomeAmount);
            collectEffect.Play();
        }
    }
}
