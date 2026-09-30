using UnityEngine;

public class PlayerWeaponVisuals : MonoBehaviour
{
    [Header("References")]
    public Transform weaponAnchor;

    [Header("Weapon Local Offset")]
    public Vector3 weaponLocalPositionOffset;
    public Vector3 weaponLocalRotationOffsetEuler;
   

    private GameObject currentWeaponObject;
    private Transform muzzlePoint;

    public void ShowWeapon(GameObject weaponPrefab)
    {
        HideWeapon();

        if (weaponPrefab == null || weaponAnchor == null)
            return;

        currentWeaponObject = Instantiate(weaponPrefab, weaponAnchor);
        currentWeaponObject.transform.localPosition = weaponLocalPositionOffset;
        currentWeaponObject.transform.localRotation = Quaternion.Euler(weaponLocalRotationOffsetEuler);
        

        muzzlePoint = FindMuzzlePoint(currentWeaponObject.transform);
    }

    public void HideWeapon()
    {
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
        }

        muzzlePoint = null;
    }

    public Transform GetWeaponAnchor()
    {
        return weaponAnchor;
    }

    public Transform GetMuzzlePoint()
    {
        return muzzlePoint;
    }

    private Transform FindMuzzlePoint(Transform root)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == "MuzzlePoint")
                return child;
        }

        return null;
    }
}