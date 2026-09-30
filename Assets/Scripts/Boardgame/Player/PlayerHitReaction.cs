using UnityEngine;
using System.Collections;

public class PlayerHitReaction : MonoBehaviour
{
    [Header("References")]
    public GameObject visualModel;
    public GameObject ragdollPrefab;

    [Header("Timing")]
    public float respawnVisualDelay = 2f;

    [Header("Force")]
    public float minHorizontalForce = 10f;
    public float maxHorizontalForce = 16f;
    public float minUpwardForce = 6f;
    public float maxUpwardForce = 10f;
    public float torqueForce = 8f;

    public void PlayHitReaction(Vector3 hitDirection, int damage)
    {
        if (visualModel == null || ragdollPrefab == null)
        {
            Debug.LogWarning("Missing visualModel or ragdollPrefab on " + name);
            return;
        }

        SpawnRagdoll(hitDirection, damage);
        StartCoroutine(ShowVisualAgainAfterDelay());
    }

    private void SpawnRagdoll(Vector3 hitDirection, int damage)
    {
        Vector3 spawnPosition = visualModel.transform.position;
        Quaternion spawnRotation = visualModel.transform.rotation;

        GameObject ragdoll = Instantiate(ragdollPrefab, spawnPosition, spawnRotation);

        visualModel.SetActive(false);

        Vector3 dir = hitDirection.normalized;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        float damageFactor = Mathf.Clamp(damage / 15f, 0.8f, 1.8f);

        float horizontalForce = Random.Range(minHorizontalForce, maxHorizontalForce) * damageFactor;
        float upwardForce = Random.Range(minUpwardForce, maxUpwardForce) * damageFactor;

        Vector3 launchForce = dir * horizontalForce + Vector3.up * upwardForce;

        Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();

        if (rigidbodies == null || rigidbodies.Length == 0)
            return;

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Rigidbody mainBody = rigidbodies[0];

        mainBody.AddForce(launchForce, ForceMode.Impulse);
        mainBody.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);

        for (int i = 1; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].AddTorque(Random.insideUnitSphere * (torqueForce * 0.35f), ForceMode.Impulse);
        }
    }

    private IEnumerator ShowVisualAgainAfterDelay()
    {
        yield return new WaitForSeconds(respawnVisualDelay);

        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
    }
}