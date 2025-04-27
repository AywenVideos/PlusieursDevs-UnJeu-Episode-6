using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/ShopItem")]
public class ShopItem : ScriptableObject
{
    public new string name;
    public BuildingBlockData blockData;
    public Image image;
    public float price;
}
