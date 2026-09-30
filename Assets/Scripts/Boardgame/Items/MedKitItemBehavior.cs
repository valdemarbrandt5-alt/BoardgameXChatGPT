using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/Medkit Behaviour")]
public class MedkitItemBehaviour : BaseItemBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 15;

    [Header("Weapon Visual")]
    public GameObject medkitVisualPrefab;

    private ItemUseContext context;
    private PlayerWeaponVisuals weaponVisuals;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;

        if (context == null || context.userStats == null || context.userPlayer == null)
        {
            Debug.LogWarning("Medkit missing context.");
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        weaponVisuals = context.userPlayer.GetComponent<PlayerWeaponVisuals>();

        if (weaponVisuals != null && medkitVisualPrefab != null)
        {
            weaponVisuals.ShowWeapon(medkitVisualPrefab);
        }
    }

    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseMedkit();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
        }
    }

    public override void CancelUse()
    {
        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }
    }

    private void UseMedkit()
    {
        int currentHp = context.userStats.hp;
        int maxHp = context.userStats.maxHp;

        if (currentHp >= maxHp)
        {
            Debug.Log(context.userPlayer.name + " is already at full HP.");

            if (weaponVisuals != null)
            {
                weaponVisuals.HideWeapon();
            }

            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        int newHp = Mathf.Min(currentHp + healAmount, maxHp);
        int healedAmount = newHp - currentHp;

        context.userStats.hp = newHp;
        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowHealPopup(
                context.userPlayer.transform.position,
                healedAmount
            );
        }

        Debug.Log(context.userPlayer.name + " used Medkit and healed " + healedAmount + " HP.");

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }
}