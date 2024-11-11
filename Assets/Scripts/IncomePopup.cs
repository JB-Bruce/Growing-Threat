using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncomePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public float minSize, maxSize;
    public int maxSizeThreshold;

    public void SetValue(int value)
    {
        text.text = "+" + value;

        text.fontSize = Mathf.Lerp(minSize, maxSize, Mathf.Clamp(value, 0, maxSizeThreshold) / (float)maxSizeThreshold);

        Destroy(gameObject, 2f);
    }


}
