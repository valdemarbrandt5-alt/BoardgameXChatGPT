using TMPro;
using UnityEngine;

public class ControlsUI : MonoBehaviour
{
    public static ControlsUI Instance;

    public enum UIState
    {
        Hidden,
        Normal,
        ItemMenu,
        ItemUseLockedTarget,
        ItemUseFreeAim,
        ItemUseControlledSequence,
        ItemUseInstantSelf,
        MapView
    }

    public GameObject controlsTextObject;
    public Canvas canvas;

    public GameObject mapViewTextObject;
    public TMP_Text mapViewText;

    public TMP_Text controlsText;

    [Header("Item UI")]
    public GameObject itemBarRoot;

    private UIState currentState = UIState.Hidden;
    private UIState stateBeforeMap = UIState.Normal;

    private void Awake()
    {
        Instance = this;
        canvas.enabled = true;
    }

    public UIState GetCurrentState()
    {
        return currentState;
    }

    public bool IsMapViewOpen()
    {
        return currentState == UIState.MapView;
    }

    public void HideAll()
    {
        SetState(UIState.Hidden);
    }

    public void ShowNormalControls()
    {
        SetState(UIState.Normal);
    }

    public void ShowItemMenuControls()
    {
        SetState(UIState.ItemMenu);
    }

    public void ShowItemUseControls(ItemUseType useType)
    {
        switch (useType)
        {
            case ItemUseType.LockedTarget:
                SetState(UIState.ItemUseLockedTarget);
                break;

            case ItemUseType.FreeAim:
                SetState(UIState.ItemUseFreeAim);
                break;

            case ItemUseType.ControlledSequence:
                SetState(UIState.ItemUseControlledSequence);
                break;

            case ItemUseType.InstantSelf:
                SetState(UIState.ItemUseInstantSelf);
                break;
        }
    }

    public void ShowMapView(string playerName)
    {
        if (currentState != UIState.MapView)
        {
            stateBeforeMap = currentState;
        }

        currentState = UIState.MapView;
        ApplyCurrentState(playerName);
    }

    public void RestoreStateAfterMap()
    {
        currentState = stateBeforeMap;
        ApplyCurrentState();
    }

    private void SetState(UIState newState)
    {
        currentState = newState;
        ApplyCurrentState();
    }

    private void ApplyCurrentState(string mapPlayerName = "")
    {
        switch (currentState)
        {
            case UIState.Hidden:
                SetControlsVisible(false);
                SetMapVisible(false);
                SetItemBarVisible(false);
                break;

            case UIState.Normal:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(false);
                SetControlsText("Q Open Items\nSpace Roll Dice\nR View Map");
                break;

            case UIState.ItemMenu:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(true);
                SetControlsText("A/D Select Item\nQ Confirm\nR View Map\nEsc Close");
                break;

            case UIState.ItemUseLockedTarget:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(true);
                SetControlsText("A/D Change Target\nQ Confirm\nR View Map\nEsc Cancel");
                break;

            case UIState.ItemUseFreeAim:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(true);
                SetControlsText("Aim Item\nQ Confirm\nR View Map\nEsc Cancel");
                break;

            case UIState.ItemUseControlledSequence:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(true);
                SetControlsText("Control Item\nQ Confirm/Detonate\nR View Map");
                break;

            case UIState.ItemUseInstantSelf:
                SetControlsVisible(true);
                SetMapVisible(false);
                SetItemBarVisible(true);
                SetControlsText("Q Use Item\nR View Map\nEsc Cancel");
                break;

            case UIState.MapView:
                SetControlsVisible(false);
                SetMapVisible(true);
                SetItemBarVisible(false);

                if (mapViewText != null)
                {
                    mapViewText.text = string.IsNullOrEmpty(mapPlayerName)
                        ? "Viewing map"
                        : mapPlayerName + " is viewing map";
                }
                break;
        }
    }

    private void SetControlsVisible(bool visible)
    {
        if (controlsTextObject != null)
            controlsTextObject.SetActive(visible);
    }

    private void SetMapVisible(bool visible)
    {
        if (mapViewTextObject != null)
            mapViewTextObject.SetActive(visible);
    }

    private void SetItemBarVisible(bool visible)
    {
        if (itemBarRoot != null)
            itemBarRoot.SetActive(visible);
    }

    private void SetControlsText(string text)
    {
        if (controlsText != null)
            controlsText.text = text;
    }
}