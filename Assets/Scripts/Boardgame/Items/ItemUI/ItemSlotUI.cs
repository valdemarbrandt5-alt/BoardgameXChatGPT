using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Item")]
    public ItemDefinition item;

    [Header("UI References")]
    public RawImage iconImage;
    public TMP_Text countText;
    public GameObject selectedHighlight;

    [Header("Visual Settings")]
    [Range(0f, 1f)] public float ownedAlpha = 1f;
    [Range(0f, 1f)] public float unownedAlpha = 0.25f;

    public ItemDefinition ItemDefinition => item;

    public void Refresh(int count, bool selected)
    {
        bool owned = count > 0;

        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = owned ? ownedAlpha : unownedAlpha;
            iconImage.color = c;
        }

        if (countText != null)
        {
            countText.text = owned ? count.ToString() : "";
        }

        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(selected);
        }
    }
}