using System.Collections.Generic;
using UnityEngine;

public class ItemMenuUI : MonoBehaviour
{
    public static ItemMenuUI Instance;

    [Header("Menu Root")]
    public GameObject menuRoot;

    [Header("All Fixed Item Slots")]
    public ItemSlotUI[] itemSlots;

    private bool isOpen = false;
    private int selectedIndex = -1;
    private PlayerInventory currentInventory;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;

        if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (ItemUseManager.Instance != null && ItemUseManager.Instance.IsItemInUse)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveSelection(1);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ConfirmSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ItemUseManager.Instance != null && ItemUseManager.Instance.ShouldBlockMenuCloseThisFrame())
                return;

            CloseMenu();
        }
    }

    public void OpenMenu(PlayerInventory inventory)
    {
        if (inventory == null)
            return;

        currentInventory = inventory;
        isOpen = true;

        if (menuRoot != null)
            menuRoot.SetActive(true);

        SelectFirstOwnedItem();
        RefreshAllSlots();
    }

    public void CloseMenu()
    {
        isOpen = false;
        currentInventory = null;
        selectedIndex = -1;

        if (menuRoot != null)
            menuRoot.SetActive(false);

        if (ItemUseManager.Instance != null && !ItemUseManager.Instance.IsItemInUse)
        {
            ItemUseManager.Instance.CloseItemMenuFromUI();
        }
    }

    public void CloseMenuSilently()
    {
        isOpen = false;
        currentInventory = null;
        selectedIndex = -1;

        if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    public void RefreshAllSlots()
    {
        if (itemSlots == null)
            return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            ItemSlotUI slot = itemSlots[i];

            if (slot == null || slot.ItemDefinition == null)
                continue;

            int count = currentInventory != null ? currentInventory.GetItemCount(slot.ItemDefinition) : 0;
            bool selected = (i == selectedIndex);

            slot.Refresh(count, selected);
        }
    }

    private void SelectFirstOwnedItem()
    {
        selectedIndex = -1;

        if (itemSlots == null || currentInventory == null)
            return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            ItemSlotUI slot = itemSlots[i];

            if (slot != null && slot.ItemDefinition != null && currentInventory.GetItemCount(slot.ItemDefinition) > 0)
            {
                selectedIndex = i;
                return;
            }
        }
    }

    private void MoveSelection(int direction)
    {
        if (itemSlots == null || itemSlots.Length == 0 || currentInventory == null)
            return;

        if (!HasAnyOwnedItems())
            return;

        int startIndex = selectedIndex;

        if (selectedIndex == -1)
        {
            SelectFirstOwnedItem();
            RefreshAllSlots();
            return;
        }

        do
        {
            selectedIndex += direction;

            if (selectedIndex < 0)
                selectedIndex = itemSlots.Length - 1;

            if (selectedIndex >= itemSlots.Length)
                selectedIndex = 0;

            ItemSlotUI slot = itemSlots[selectedIndex];

            if (slot != null && slot.ItemDefinition != null && currentInventory.GetItemCount(slot.ItemDefinition) > 0)
            {
                RefreshAllSlots();
                return;
            }
        }
        while (selectedIndex != startIndex);
    }

    private void ConfirmSelection()
    {
        if (selectedIndex < 0 || selectedIndex >= itemSlots.Length)
            return;

        ItemSlotUI slot = itemSlots[selectedIndex];
        if (slot == null || slot.ItemDefinition == null)
            return;

        if (currentInventory == null || currentInventory.GetItemCount(slot.ItemDefinition) <= 0)
            return;

        ItemUseManager.Instance.StartUsingSelectedItem(TurnManager.Instance, slot.ItemDefinition);
    }

    private bool HasAnyOwnedItems()
    {
        if (itemSlots == null || currentInventory == null)
            return false;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            ItemSlotUI slot = itemSlots[i];

            if (slot != null && slot.ItemDefinition != null && currentInventory.GetItemCount(slot.ItemDefinition) > 0)
                return true;
        }

        return false;
    }
}