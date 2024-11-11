using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankBuilding : Building
{
    public int goldStorage;

    new protected void Start()
    {
        base.Start();
        RessourceManager.instance.AddBank(goldStorage);
    }

    private void OnDestroy()
    {
        RessourceManager.instance.RemoveBank(goldStorage);

    }
}
