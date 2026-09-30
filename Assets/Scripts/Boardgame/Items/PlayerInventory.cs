using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<ItemDefinition, int> items = new Dictionary<ItemDefinition, int>();

    public void AddItem(ItemDefinition item)
    {
        if (item == null)
            return;

        if (!items.ContainsKey(item))
            items[item] = 0;

        items[item]++;
    }

    public bool UseItem(ItemDefinition item)
    {
        if (item == null)
            return false;

        if (!items.ContainsKey(item) || items[item] <= 0)
            return false;

        items[item]--;

        if (items[item] == 0)
            items.Remove(item);

        return true;
    }

    public bool HasItem(ItemDefinition item)
    {
        return item != null && items.ContainsKey(item) && items[item] > 0;
    }

    public int GetItemCount(ItemDefinition item)
    {
        if (item == null)
            return 0;

        return items.ContainsKey(item) ? items[item] : 0;
    }

    public Dictionary<ItemDefinition, int> GetAllItems()
    {
        return items;
    }

    public List<ItemDefinition> GetOwnedItems()
    {
        List<ItemDefinition> owned = new List<ItemDefinition>();

        foreach (var pair in items)
        {
            if (pair.Key != null && pair.Value > 0)
            {
                owned.Add(pair.Key);
            }
        }

        return owned;
    }

    public ItemDefinition RemoveRandomOwnedItem()
    {
        List<ItemDefinition> owned = GetOwnedItems();

        if (owned.Count == 0)
            return null;

        ItemDefinition chosen = owned[Random.Range(0, owned.Count)];

        if (UseItem(chosen))
            return chosen;

        return null;
    }
}