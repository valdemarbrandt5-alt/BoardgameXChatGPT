using UnityEngine;

public class DebugGiveItems : MonoBehaviour
{
    public PlayerInventory inventory;

    public ItemDefinition sniper;
    public ItemDefinition medkit;
    public ItemDefinition grenade;
    public ItemDefinition portal;
    public ItemDefinition magnet;
    public ItemDefinition rccar;
    public ItemDefinition shotgun;
    public ItemDefinition rifle;
    public ItemDefinition gambling;
    public ItemDefinition rocket;

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("No PlayerInventory found for DebugGiveItems.");
            return;
        }

        if (sniper != null)
            inventory.AddItem(sniper);

        if (gambling != null)
            inventory.AddItem(gambling);

        if (rocket != null)
            inventory.AddItem(rocket);

        if (shotgun != null)
            inventory.AddItem(shotgun);
        if (rifle != null)
            inventory.AddItem(rifle);

        if (rccar != null)
            inventory.AddItem(rccar);

        if (grenade != null)
            inventory.AddItem(grenade);

        if (magnet != null)
            inventory.AddItem(magnet);

        if (medkit != null)
            inventory.AddItem(medkit);

        if (medkit != null)
            inventory.AddItem(medkit);

        if (portal != null)
            inventory.AddItem(portal);

        Debug.Log("Debug items added.");
    }
}