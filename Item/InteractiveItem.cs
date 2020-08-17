using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple script attached to every pickUp in the game to define what tipe of item it is.
public enum ItemType {Null, Shield, Medkit, Pistol_Magazine, Grn_Magazine, Tsu_Magazine, Sniper_Magazine }
public class InteractiveItem : MonoBehaviour
{
    public ItemType _itemType;

    void Start()
    {
        if (_itemType == ItemType.Shield)
        { gameObject.name = "Shield"; }
        else if (_itemType == ItemType.Medkit)
        { gameObject.name = "Medkit"; }
        else if (_itemType == ItemType.Pistol_Magazine)
        { gameObject.name = "Pistol_Magazine"; }
        else if (_itemType == ItemType.Grn_Magazine)
        { gameObject.name = "Grn_Magazine"; }
        else if (_itemType == ItemType.Tsu_Magazine)
        { gameObject.name = "7su_Magazine"; }
        else if (_itemType == ItemType.Sniper_Magazine)
        { gameObject.name = "Sniper_Magazine"; }
    }
}

   
