using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    BuildingElements building;

    public Image image;
    public Button button;

    BuildingManager manager;

    public void Init(BuildingElements newBuilding)
    {
        building = newBuilding;
        image.sprite = building.sprite;

        manager = BuildingManager.instance;
    }

    public void ButtonClick()
    {
        manager.SelectBuilding(building);
    }
}
