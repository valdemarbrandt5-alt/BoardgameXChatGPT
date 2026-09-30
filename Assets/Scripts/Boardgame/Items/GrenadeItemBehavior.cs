using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "BoardGame/Items/Grenade Behaviour")]
public class GrenadeItemBehaviour : BaseItemBehaviour
{
    [Header("Visuals")]
    public GameObject grenadeVisualPrefab;
    public GameObject grenadeProjectilePrefab;
    public GameObject grenadeAimMarkerPrefab;
    public GameObject grenadeRangeCirclePrefab;
    public GameObject explosionEffectPrefab;

    [Header("Aim Settings")]
    public float aimRange = 3f;
    public float aimMoveSpeed = 3f;
    public float aimHeightOffset = 0.05f;
    public float defaultAimDistance = 2f;

    [Header("Camera")]
    [Range(0f, 1f)] public float cameraFollowTowardAim = 0.45f;

    [Header("Explosion Shake")]
    public float explosionShakeDuration = 0.2f;
    public float explosionShakeStrength = 0.2f;

    [Header("Camera After Explosion")]
    public float postExplosionCameraDelay = 1f;
    [Header("Throw Settings")]
    public float throwDuration = 0.35f;
    public float throwArcHeight = 1.5f;

    [Header("Explosion Settings")]
    public float explosionRadius = 1.5f;
    public Vector3 explosionOffset = new Vector3(0f, 0.1f, 0f);

    private ItemUseContext context;
    private PlayerWeaponVisuals weaponVisuals;
    private PlayerAimRotation aimRotation;
    private CameraFollow cameraFollow;

    private GameObject spawnedAimMarker;
    private GameObject spawnedRangeCircle;

    private Vector3 currentAimOffset;
    private bool isThrowing = false;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        weaponVisuals = context.userPlayer.GetComponent<PlayerWeaponVisuals>();
        aimRotation = context.userPlayer.GetComponent<PlayerAimRotation>();
        cameraFollow = context.turnManager.cameraFollow;
        isThrowing = false;

        Vector3 forward = context.userPlayer.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        currentAimOffset = forward.normalized * Mathf.Min(defaultAimDistance, aimRange);

        if (weaponVisuals != null && grenadeVisualPrefab != null)
        {
            weaponVisuals.ShowWeapon(grenadeVisualPrefab);
        }

        if (grenadeAimMarkerPrefab != null)
        {
            spawnedAimMarker = Instantiate(grenadeAimMarkerPrefab);
        }

        if (grenadeRangeCirclePrefab != null)
        {
            spawnedRangeCircle = Instantiate(grenadeRangeCirclePrefab);
        }

        UpdateRangeCircle();
        UpdateAimVisuals();
        UpdateCameraAimFollow();
    }

    public override void HandleInput()
    {
        if (isThrowing)
            return;

        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.D)) input += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) input += Vector3.back;
        if (Input.GetKey(KeyCode.W)) input += Vector3.left;
        if (Input.GetKey(KeyCode.S)) input += Vector3.right;

        if (input.sqrMagnitude > 0.001f)
        {
            input.Normalize();
            currentAimOffset += input * aimMoveSpeed * Time.deltaTime;
            currentAimOffset.y = 0f;
            currentAimOffset = Vector3.ClampMagnitude(currentAimOffset, aimRange);

            if (currentAimOffset.sqrMagnitude < 0.001f)
            {
                currentAimOffset = Vector3.forward * 0.1f;
            }

            UpdateAimVisuals();
            UpdateCameraAimFollow();
        }

        UpdateRangeCircle();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            context.turnManager.StartCoroutine(ThrowRoutine());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
        }
    }

    public override void CancelUse()
    {
        CleanupVisuals();

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideWorldPosition();
        }
    }

    private void UpdateAimVisuals()
    {
        Vector3 targetPos = GetAimTargetPosition();

        if (spawnedAimMarker != null)
        {
            spawnedAimMarker.transform.position = targetPos + Vector3.up * aimHeightOffset;
        }

        if (aimRotation != null)
        {
            aimRotation.SetAiming(targetPos);
        }
    }

    private void UpdateCameraAimFollow()
    {
        if (cameraFollow == null || context == null || context.userPlayer == null)
            return;

        Vector3 playerPos = context.userPlayer.transform.position;
        Vector3 aimPos = GetAimTargetPosition();

        Vector3 cameraFocusPoint = Vector3.Lerp(playerPos, aimPos, cameraFollowTowardAim);
        cameraFocusPoint.y = playerPos.y;

        cameraFollow.SetOverrideWorldPosition(cameraFocusPoint);
    }

    private void UpdateRangeCircle()
    {
        if (spawnedRangeCircle == null)
            return;

        spawnedRangeCircle.transform.position = context.userPlayer.transform.position + Vector3.up * aimHeightOffset;
    }

    private Vector3 GetAimTargetPosition()
    {
        Vector3 origin = context.userPlayer.transform.position;
        Vector3 target = origin + currentAimOffset;
        target.y = origin.y;
        return target;
    }

    private IEnumerator ThrowRoutine()
    {
        if (isThrowing)
            yield break;

        isThrowing = true;

        Vector3 startPos = GetProjectileStartPosition();
        Vector3 targetPos = GetAimTargetPosition();

        GameObject projectile = null;

        if (grenadeProjectilePrefab != null)
        {
            projectile = Instantiate(grenadeProjectilePrefab, startPos, Quaternion.identity);
        }

        if (weaponVisuals != null)
        {
            weaponVisuals.HideWeapon();
        }

        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / throwDuration);

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * throwArcHeight;
            Vector3 arcPos = flatPos + Vector3.up * arc;

            if (projectile != null)
            {
                projectile.transform.position = arcPos;
            }

            yield return null;
        }

        if (projectile != null)
        {
            Destroy(projectile);
        }

        Explode(targetPos);

        if (cameraFollow != null)
        {
            cameraFollow.SetOverrideWorldPosition(targetPos);
        }

        yield return new WaitForSeconds(postExplosionCameraDelay);

        CleanupVisuals();

        if (aimRotation != null)
        {
            aimRotation.SetIdle();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideWorldPosition();
        }

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }

    private void Explode(Vector3 center)
    {
        Vector3 explosionCenter = center + explosionOffset;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, explosionCenter, Quaternion.identity);
        }


        PlayerController[] players = context.turnManager.players;
        if (players == null)
            return;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null)
                continue;

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats == null)
                continue;

            float dist = Vector3.Distance(player.transform.position, center);

            if (dist <= explosionRadius)
            {
                int damage = Random.Range(9, 12);
                stats.TakeDamageFromSource(damage, context.userPlayer.transform);

                Debug.Log(context.userPlayer.name + " hit " + player.name + " with Grenade for " + damage + " damage.");
            }
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(explosionShakeDuration, explosionShakeStrength);
        }
    }

    private Vector3 GetProjectileStartPosition()
    {
        if (weaponVisuals != null)
        {
            Transform muzzle = weaponVisuals.GetMuzzlePoint();
            if (muzzle != null)
                return muzzle.position;

            Transform anchor = weaponVisuals.GetWeaponAnchor();
            if (anchor != null)
                return anchor.position;
        }

        return context.userPlayer.transform.position + Vector3.up;
    }

    private void CleanupVisuals()
    {
        if (spawnedAimMarker != null)
        {
            Destroy(spawnedAimMarker);
            spawnedAimMarker = null;
        }

        if (spawnedRangeCircle != null)
        {
            Destroy(spawnedRangeCircle);
            spawnedRangeCircle = null;
        }
    }
}