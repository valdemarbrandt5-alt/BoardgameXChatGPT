using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string displayName;
    public Sprite icon;
    public ItemRarity rarity;
    public ItemUseType useType;
    public bool consumesTurn;

    [Header("World Visual")]
    public GameObject worldPrefab;

    [Header("Behaviour")]
    public BaseItemBehaviour behaviour;
}