using UnityEngine;

public class PlayerAimRotation : MonoBehaviour
{
    public enum RotationMode
    {
        Idle,
        Moving,
        Aiming
    }

    [Header("Settings")]
    public float rotationSpeed = 15f;
    public Transform cameraTransform;

    private RotationMode currentMode = RotationMode.Idle;

    private Vector3 aimTarget;
    private Vector3 moveDirection;

    private void Update()
    {
        switch (currentMode)
        {
            case RotationMode.Idle:
                HandleIdle();
                break;

            case RotationMode.Moving:
                HandleMovement();
                break;

            case RotationMode.Aiming:
                HandleAiming();
                break;
        }
    }

    // ===== MODES =====

    public void SetIdle()
    {
        currentMode = RotationMode.Idle;
    }

    public void SetMoving(Vector3 direction)
    {
        moveDirection = direction;
        currentMode = RotationMode.Moving;
    }

    public void SetAiming(Vector3 target)
    {
        aimTarget = target;
        currentMode = RotationMode.Aiming;
    }

    // ===== HANDLERS =====

    private void HandleIdle()
    {
        if (cameraTransform == null)
            return;

        Vector3 dir = -cameraTransform.forward;
        dir.y = 0f;

        RotateTowards(dir);
    }

    private void HandleMovement()
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Vector3 dir = moveDirection;
        dir.y = 0f;

        RotateTowards(dir);
    }

    private void HandleAiming()
    {
        Vector3 dir = aimTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        RotateTowards(dir);
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}