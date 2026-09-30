using UnityEngine;

public class TileResolver : MonoBehaviour
{
    public static TileResolver Instance;

    public ItemDefinition testItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResolveTile(PlayerStats player, BoardTile tile)
    {
        switch (tile.tileType)
        {
            case TileType.Normal:
                Debug.Log("Normal tile");
                break;

            case TileType.Danger:
                int dmg = Random.Range(9, 12);
                player.TakeDamage(dmg);
                Debug.Log("Danger: -" + dmg + " HP");
                break;

            case TileType.Healing:
                player.hp = Mathf.Min(player.hp + 10, player.maxHp);
                Debug.Log("Healing +10");
                break;

            case TileType.Point:
                player.coins += 5;
                Debug.Log("Coins +5");
                break;

            case TileType.Item:
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();

                if (inventory != null && testItem != null)
                {
                    inventory.AddItem(testItem);
                    Debug.Log("Got item: " + testItem.displayName);
                }
                else
                {
                    Debug.LogWarning("Missing PlayerInventory or testItem on TileResolver.");
                }
                break;

            case TileType.Trophy:
                Debug.Log("Trophy tile (kommer senere)");
                break;
        }
    }
}