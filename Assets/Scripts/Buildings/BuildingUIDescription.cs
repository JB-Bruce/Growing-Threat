using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUIDescription : MonoBehaviour
{
    [SerializeField] GameObject go;

    [SerializeField] TextMeshProUGUI buildingNameText;
    [SerializeField] TextMeshProUGUI buildingDescriptionText;
    [SerializeField] TextMeshProUGUI buildingCostText;

    [SerializeField] float minX, maxX;

    public void Init(string bName, string bDesc, int bCost)
    {
        buildingNameText.text = bName;
        buildingDescriptionText.text = bDesc;
        buildingCostText.text = bCost.ToString();
    }

    public void Active(bool active)
    {
        go.SetActive(active);
    }

    public void SetPosition(float posX)
    {
        Vector2 newPos = new Vector2(Mathf.Clamp(posX, minX, maxX), go.transform.position.y);
        transform.position = newPos;
    }
}
