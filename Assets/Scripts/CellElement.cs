using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellElement : Entity
{
    public string elementName;

    public Sprite elementSprite;
    public bool blockFrienflyUnits;
    public bool blockEnemyUnits;
    public bool canBeDestroyed;

    public virtual void Select() { }
    public virtual void UnSelect() { }
}
