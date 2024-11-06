using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBuilding : Building
{
    [SerializeField] float incomeInterval;
    [SerializeField] int incomeAmount;
    float interval = 0f;

    [SerializeField] GameObject incomePrefab;

    private void Update()
    {
        interval += Time.deltaTime;
        if(interval >= incomeInterval)
        {
            interval -= incomeInterval;
            RessourceManager.instance.AddCoin(incomeAmount);
            GameObject go = Instantiate(incomePrefab, transform.position, Quaternion.identity);
            go.GetComponent<IncomePopup>().SetValue(incomeAmount);
            go.transform.position = transform.position;
        }
    }
}
