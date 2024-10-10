using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;

    int gold = 0;

    public static RessourceManager instance;
    private void Awake()
    {
        instance = this;

        goldText.text = "0";
    }

    private void Start()
    {
        AddCoin(10);
    }

    public void AddCoin(int amount)
    {
        gold += amount;
        goldText.text = gold.ToString();
    }
}
