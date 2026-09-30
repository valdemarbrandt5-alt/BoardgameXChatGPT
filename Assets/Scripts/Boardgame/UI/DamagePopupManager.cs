using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public GameObject damagePopupPrefab;
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamagePopup(Vector3 worldPosition, int amount)
    {
        if (damagePopupPrefab == null)
            return;

        GameObject popupObj = Instantiate(damagePopupPrefab, worldPosition + worldOffset, Quaternion.identity);

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(amount, false);
        }
    }

    public void ShowHealPopup(Vector3 worldPosition, int amount)
    {
        if (damagePopupPrefab == null)
            return;

        GameObject popupObj = Instantiate(damagePopupPrefab, worldPosition + worldOffset, Quaternion.identity);

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(amount, true);
        }
    }
}