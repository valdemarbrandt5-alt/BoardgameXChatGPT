using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "BoardGame/Items/RC Car Behaviour")]
public class RCCarItemBehaviour : BaseItemBehaviour
{
    [Header("Prefab")]
    public GameObject rcCarPrefab;

    [Header("Settings")]
    public float maxDuration = 10f;
    public float explosionRadius = 1.8f;
    public int minDamage = 10;
    public int maxDamage = 15;

    [Header("Camera")]
    public float cameraHeightOffset = 2f;

    private ItemUseContext context;
    private GameObject spawnedCar;
    private RCCarController carController;

    private CameraFollow cameraFollow;
    private bool isActive = false;

    public override void BeginUse(ItemUseContext context)
    {
        this.context = context;
        cameraFollow = context.turnManager.cameraFollow;

        SpawnCar();

        if (spawnedCar == null)
        {
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        carController = spawnedCar.GetComponent<RCCarController>();

        if (carController == null)
        {
            Debug.LogError("RC Car prefab mangler RCCarController!");
            ItemUseManager.Instance.CancelActiveItemUse();
            return;
        }

        carController.Init(
            context,
            explosionRadius,
            minDamage,
            maxDamage,
            OnCarExploded
        );

        if (cameraFollow != null)
        {
            cameraFollow.SetOverrideTarget(spawnedCar.transform);
        }

        isActive = true;

        context.turnManager.StartCoroutine(DurationRoutine());
    }

    public override void HandleInput()
    {
        if (!isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            carController.TriggerExplosion();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUse();
            ItemUseManager.Instance.CancelActiveItemUse();
        }
    }

    public override void CancelUse()
    {
        Cleanup();
    }

    private void SpawnCar()
    {
        if (rcCarPrefab == null || context.userPlayer == null)
            return;

        Vector3 spawnPos =
            context.userPlayer.transform.position +
            context.userPlayer.transform.forward * 1.2f +
            Vector3.up * 0.2f;

        spawnedCar = Instantiate(rcCarPrefab, spawnPos, Quaternion.identity);
    }

    private IEnumerator DurationRoutine()
    {
        float time = 0f;

        while (time < maxDuration && isActive)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (isActive && carController != null)
        {
            carController.TriggerExplosion();
        }
    }

    private void OnCarExploded()
    {
        isActive = false;

        Cleanup();

        ItemUseManager.Instance.FinishActiveItemUse(false);
    }

    private void Cleanup()
    {
        if (cameraFollow != null)
        {
            cameraFollow.ClearOverrideTarget();
        }

        if (spawnedCar != null)
        {
            GameObject.Destroy(spawnedCar);
        }
    }
}