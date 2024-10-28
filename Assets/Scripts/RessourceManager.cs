using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    public int gold { get; private set; } = 0;

    public static RessourceManager instance;
    private void Awake()
    {
        instance = this;

        goldText.text = "0";
    }

    private void Start()
    {
        AddCoin(100);
    }

    public void AddCoin(int amount)
    {
        gold += amount;
        goldText.text = gold.ToString();
    }

    public bool TryRemoveCoin(int amount) 
    { 
        if(gold - amount < 0) return false;

        AddCoin(-amount);
        return true;
    }
}
