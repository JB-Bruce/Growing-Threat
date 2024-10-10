using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInit : MonoBehaviour
{
    BuildingElements building;

    GameObject buildingInstance;
    Vector2 position = Vector2.zero;

    public GameObject cross;

    public void Init(BuildingElements newBuilding, Vector2 pos)
    {
        building = newBuilding;
        transform.position = pos;
        position = pos;
        name = newBuilding.name;

        ShowPreview();
    }

    public void ShowPreview()
    {
        buildingInstance = Instantiate(building.preview, position, Quaternion.identity, transform);
    }

    public void HidePreview()
    {
        Destroy(buildingInstance);
    }

    public void SetPossibility(bool possibility)
    {
        cross.SetActive(possibility);
    }

    public void Place()
    {
        if (buildingInstance != null)
        {
            Destroy(buildingInstance);

            buildingInstance = Instantiate(building.prefab, transform);
            buildingInstance.transform.localPosition = Vector2.zero;
        }
    }
}
