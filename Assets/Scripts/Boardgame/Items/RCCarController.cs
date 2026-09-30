using UnityEngine;
using System;

public class RCCarController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float turnSpeed = 120f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.25f;
    public LayerMask groundMask = ~0;

    [Header("Explosion")]
    public GameObject explosionEffectPrefab;

    private ItemUseContext context;
    private float explosionRadius;
    private int minDamage;
    private int maxDamage;

    private bool hasExploded = false;
    private Action onExplodedCallback;

    private Rigidbody rb;

    public void Init(
        ItemUseContext context,
        float explosionRadius,
        int minDamage,
        int maxDamage,
        Action onExploded
    )
    {
        this.context = context;
        this.explosionRadius = explosionRadius;
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.onExplodedCallback = onExploded;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("RC Car mangler Rigidbody.");
        }
    }

    private void Update()
    {
        if (hasExploded)
            return;

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W)) move += 1f;
        if (Input.GetKey(KeyCode.S)) move -= 1f;
        if (Input.GetKey(KeyCode.A)) turn -= 1f;
        if (Input.GetKey(KeyCode.D)) turn += 1f;

        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);

        Vector3 forward = transform.forward * move * moveSpeed * Time.deltaTime;

        if (rb != null)
        {
            Vector3 targetPosition = rb.position + forward;
            rb.MovePosition(targetPosition);
        }
        else
        {
            transform.position += forward;
        }
    }

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (!IsGrounded())
            return;

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask);
    }

    public void TriggerExplosion()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        Explode();

        onExplodedCallback?.Invoke();
    }

    private void Explode()
    {
        Vector3 center = transform.position;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, center + Vector3.up * 0.1f, Quaternion.identity);
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

            float distance = Vector3.Distance(center, player.transform.position);

            if (distance <= explosionRadius)
            {
                int damage = UnityEngine.Random.Range(minDamage, maxDamage + 1);

                stats.TakeDamageFromSource(
                    damage,
                    context.userPlayer.transform
                );

                Debug.Log("RC Car hit " + player.name + " for " + damage);
            }
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.25f);
        }
    }
}