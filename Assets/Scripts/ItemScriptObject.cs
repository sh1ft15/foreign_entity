using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemScriptObject", order = 1)]
public class ItemScriptObject : ScriptableObject
{
    public string itemName;
    public int range, cost, damage;
    public Sprite sprite;
}