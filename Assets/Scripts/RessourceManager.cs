using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI maxGoldText;

    public GameObject incomePopup;
    public Transform incomePopupParent;
    public Transform incomePopupStartPoint;

    public float incomeRandomRotation;
    public float incomeRandomPosition;

    public float minTimeBetweenPopup;
    int actualIncome = 0;
    float actualTime = 0f;


    public int maxGold;
    

    public int gold { get; private set; } = 0;

    public static RessourceManager instance;
    private void Awake()
    {
        instance = this;

        goldText.text = "0";
    }

    private void Start()
    {
        SetMaxGoldText();
        gold = 100;
        SetGoldText();
    }

    private void Update()
    {
        actualTime += Time.deltaTime;

        if(actualTime > minTimeBetweenPopup && actualIncome > 0)
        {
            actualTime = 0f;

            GameObject go = Instantiate(incomePopup, incomePopupStartPoint.position, Quaternion.identity, incomePopupParent);
            go.GetComponent<IncomePopup>().SetValue(actualIncome);
            go.transform.position = new Vector2(go.transform.position.x + Random.Range(-incomeRandomPosition, incomeRandomPosition), go.transform.position.y);
            go.transform.Rotate(new Vector3(0, 0, Random.Range(-incomeRandomRotation, incomeRandomRotation)));

            actualIncome = 0;
        }
    }

    public void AddCoin(int amount)
    {
        if (amount < 0)
        {
            gold += amount;
            SetGoldText();
            return;
        }

        if (gold + amount > maxGold)
            amount = maxGold - gold;
        gold += amount;
        SetGoldText();

        actualIncome += amount;
    }

    public bool TryRemoveCoin(int amount) 
    { 
        if(gold - amount < 0) return false;

        AddCoin(-amount);
        return true;
    }

    public void AddBank(int amount)
    {
        maxGold += amount;
        SetMaxGoldText();
    }

    public void RemoveBank(int amount)
    {
        maxGold -= amount;
        SetMaxGoldText();
    }

    private void SetMaxGoldText()
    {
        maxGoldText.text = "/" + maxGold;
    }

    private void SetGoldText()
    {
        goldText.text = gold.ToString();
    }
}
