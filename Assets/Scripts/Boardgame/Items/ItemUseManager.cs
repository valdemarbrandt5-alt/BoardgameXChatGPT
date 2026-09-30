using UnityEngine;

public class ItemUseManager : MonoBehaviour
{
    public static ItemUseManager Instance;

    private bool itemMenuOpen = false;
    private bool itemInUse = false;

    private BaseItemBehaviour activeBehaviour;
    private ItemUseContext activeContext;
    private bool blockMenuCloseThisFrame = false;
    private PlayerInventory activeInventory;
    private ItemDefinition selectedItem;

    public bool IsItemMenuOpen => itemMenuOpen;
    public bool IsItemInUse => itemInUse;
    public bool IsBusyWithItems => itemMenuOpen || itemInUse;

    private void Awake()
    {
        Instance = this;
    }

    public bool ShouldBlockMenuCloseThisFrame()
    {
        return blockMenuCloseThisFrame;
    }

    private void Update()
    {
        if (!itemMenuOpen)
            return;

        if (itemInUse && activeBehaviour != null)
        {
            activeBehaviour.HandleInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseItemMenu();
        }
    }

    private void LateUpdate()
    {
        blockMenuCloseThisFrame = false;
    }

    public void OpenItemMenu(TurnManager turnManager)
    {
        if (itemMenuOpen)
            return;

        if (!turnManager.CanUseItemsNow())
            return;

        PlayerController currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null)
            return;

        PlayerInventory inventory = currentPlayer.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Current player has no PlayerInventory.");
            return;
        }

        activeInventory = inventory;
        itemMenuOpen = true;
        itemInUse = false;

        turnManager.HideDiceVisual();

        if (ControlsUI.Instance != null)
            ControlsUI.Instance.ShowItemMenuControls();

        if (ItemMenuUI.Instance != null)
            ItemMenuUI.Instance.OpenMenu(inventory);

        Debug.Log("Item menu opened");
    }

    public void CloseItemMenu()
    {
        CloseItemMenuInternal(true);
    }

    public void CloseItemMenuFromUI()
    {
        CloseItemMenuInternal(false);
    }

    private void CloseItemMenuInternal(bool notifyUI)
    {
        if (!itemMenuOpen)
            return;

        if (itemInUse)
            return;

        itemMenuOpen = false;

        if (notifyUI && ItemMenuUI.Instance != null)
        {
            ItemMenuUI.Instance.CloseMenuSilently();
        }

        if (ControlsUI.Instance != null)
            ControlsUI.Instance.ShowNormalControls();

        if (TurnManager.Instance != null && TurnManager.Instance.CanShowDiceAgain())
        {
            TurnManager.Instance.ShowDiceVisual();
        }

        Debug.Log("Item menu closed");
    }

    public void StartUsingSelectedItem(TurnManager turnManager, ItemDefinition item)
    {
        if (!itemMenuOpen || itemInUse)
            return;

        if (item == null || item.behaviour == null)
            return;

        PlayerController currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null)
            return;

        PlayerStats stats = currentPlayer.GetComponent<PlayerStats>();
        PlayerInventory inventory = currentPlayer.GetComponent<PlayerInventory>();

        if (stats == null || inventory == null)
            return;

        if (!inventory.HasItem(item))
        {
            Debug.Log("Player does not have item: " + item.displayName);
            return;
        }

        selectedItem = item;
        activeInventory = inventory;
        itemInUse = true;

        activeContext = new ItemUseContext(
            turnManager,
            currentPlayer,
            stats,
            item,
            inventory
        );

        activeBehaviour = Instantiate(item.behaviour);
        activeBehaviour.BeginUse(activeContext);

        if (ControlsUI.Instance != null)
            ControlsUI.Instance.ShowItemUseControls(item.useType);

        Debug.Log("Started using item: " + item.displayName);
    }

    public void FinishActiveItemUse(bool consumedTurn)
    {
        if (!itemInUse || activeContext == null || selectedItem == null)
            return;

        activeInventory.UseItem(selectedItem);

        if (activeContext.turnManager != null)
        {
            activeContext.turnManager.MarkItemUsedThisTurn();
        }

        itemInUse = false;
        itemMenuOpen = false;

        if (consumedTurn)
        {
            activeContext.turnManager.ConsumeTurnAction();
        }

        if (ControlsUI.Instance != null)
            ControlsUI.Instance.ShowNormalControls();

        if (!consumedTurn && activeContext.turnManager.CanShowDiceAgain())
        {
            activeContext.turnManager.ShowDiceVisual();
        }

        CleanupActiveUse();
    }

    public void CancelActiveItemUse()
    {
        if (!itemInUse)
            return;

        if (activeBehaviour != null)
            activeBehaviour.CancelUse();

        itemInUse = false;

        CleanupActiveUse();

        blockMenuCloseThisFrame = true;

        if (ControlsUI.Instance != null)
            ControlsUI.Instance.ShowItemMenuControls();

        Debug.Log("Cancelled active item use");
    }

    private void CleanupActiveUse()
    {
        if (activeBehaviour != null)
        {
            Destroy(activeBehaviour);
            activeBehaviour = null;
        }

        activeContext = null;
        selectedItem = null;
    }
}