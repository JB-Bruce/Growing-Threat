using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    BuildingElements building;

    public Image image;
    public Button button;

    public TextMeshProUGUI costText;

    BuildingManager manager;

    public void Init(BuildingElements newBuilding)
    {
        manager = BuildingManager.instance;

        building = newBuilding;
        image.sprite = building.sprite;
        costText.text = building.cost.ToString();
    }

    public void ButtonClick()
    {
        manager.SelectBuilding(building);
    }
}
