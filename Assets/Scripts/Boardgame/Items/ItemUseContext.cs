using UnityEngine;

public class ItemUseContext
{
    public TurnManager turnManager;
    public PlayerController userPlayer;
    public PlayerStats userStats;
    public ItemDefinition itemDefinition;
    public PlayerInventory inventory;

    public ItemUseContext(
        TurnManager turnManager,
        PlayerController userPlayer,
        PlayerStats userStats,
        ItemDefinition itemDefinition,
        PlayerInventory inventory)
    {
        this.turnManager = turnManager;
        this.userPlayer = userPlayer;
        this.userStats = userStats;
        this.itemDefinition = itemDefinition;
        this.inventory = inventory;
    }
}