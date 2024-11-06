using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncomePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void SetValue(int value)
    {
        text.text = "+" + value;

        Destroy(gameObject, 2f);
    }


}
