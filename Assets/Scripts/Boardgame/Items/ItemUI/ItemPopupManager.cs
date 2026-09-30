using UnityEngine;

public class ItemPopupManager : MonoBehaviour
{
    public static ItemPopupManager Instance;

    public GameObject itemPopupPrefab;
    public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private void Awake()
    {
        Instance = this;
    }

    public void ShowItemPopup(Vector3 worldPosition, Sprite icon)
    {
        if (itemPopupPrefab == null)
            return;

        GameObject popupObj = Instantiate(itemPopupPrefab, worldPosition + worldOffset, Quaternion.identity);

        ItemPopup popup = popupObj.GetComponent<ItemPopup>();
        if (popup != null)
        {
            popup.Setup(icon);
        }
    }
}