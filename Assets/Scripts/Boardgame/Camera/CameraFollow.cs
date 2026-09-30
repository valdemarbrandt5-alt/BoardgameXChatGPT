using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Normal Follow")]
    public Vector3 followOffset = new Vector3(3f, 2.5f, 0f);
    public float followSpeed = 5f;
    public Vector3 fixedRotation = new Vector3(45f, 0f, 0f);

    [Header("Map View")]
    public float mapHeight = 5f;
    public float mapMoveSpeed = 15f;
    public float mapTransitionSpeed = 6f;

    [Header("References")]
    public TurnManager turnManager;

    private bool isMapView = false;
    private Vector3 mapPosition;
    private Vector3 desiredPosition;

    private Transform overrideTarget;
    private bool hasOverrideWorldPosition = false;
    private Vector3 overrideWorldPosition;

    private void LateUpdate()
    {
        Vector3 followPoint;

        if (hasOverrideWorldPosition)
        {
            followPoint = overrideWorldPosition;
        }
        else
        {
            Transform currentFollowTarget = overrideTarget != null ? overrideTarget : target;

            if (currentFollowTarget == null)
                return;

            followPoint = currentFollowTarget.position;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            bool canToggle = true;

            if (turnManager != null)
            {
                canToggle = turnManager.CanOpenMap() || isMapView;
            }

            if (canToggle)
            {
                ToggleMapView();
            }
        }

        if (isMapView)
        {
            HandleMapMovement();

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                mapTransitionSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(fixedRotation),
                mapTransitionSpeed * Time.deltaTime
            );

            return;
        }

        desiredPosition = followPoint + followOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(fixedRotation),
            followSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (!isMapView && overrideTarget == null && !hasOverrideWorldPosition && target != null)
        {
            desiredPosition = target.position + followOffset;
        }
    }

    public void SetOverrideTarget(Transform newOverrideTarget)
    {
        overrideTarget = newOverrideTarget;
        hasOverrideWorldPosition = false;

        if (!isMapView && overrideTarget != null)
        {
            desiredPosition = overrideTarget.position + followOffset;
        }
    }

    public void ClearOverrideTarget()
    {
        overrideTarget = null;

        if (!isMapView && !hasOverrideWorldPosition && target != null)
        {
            desiredPosition = target.position + followOffset;
        }
    }

    public void SetOverrideWorldPosition(Vector3 worldPos)
    {
        overrideWorldPosition = worldPos;
        hasOverrideWorldPosition = true;

        if (!isMapView)
        {
            desiredPosition = overrideWorldPosition + followOffset;
        }
    }

    public void ClearOverrideWorldPosition()
    {
        hasOverrideWorldPosition = false;

        if (!isMapView)
        {
            Transform currentFollowTarget = overrideTarget != null ? overrideTarget : target;

            if (currentFollowTarget != null)
            {
                desiredPosition = currentFollowTarget.position + followOffset;
            }
        }
    }

    public bool IsMapViewOpen()
    {
        return isMapView;
    }

    private void ToggleMapView()
    {
        Vector3 currentFollowPoint;

        if (hasOverrideWorldPosition)
        {
            currentFollowPoint = overrideWorldPosition;
        }
        else
        {
            Transform currentFollowTarget = overrideTarget != null ? overrideTarget : target;

            if (currentFollowTarget == null)
                return;

            currentFollowPoint = currentFollowTarget.position;
        }

        isMapView = !isMapView;

        if (isMapView)
        {
            mapPosition = transform.position;
            mapPosition.y = mapHeight;
            desiredPosition = mapPosition;

            if (turnManager != null)
            {
                turnManager.HideDiceVisual();
            }

            if (ControlsUI.Instance != null && turnManager != null)
            {
                PlayerController currentPlayer = turnManager.GetCurrentPlayer();
                string playerName = currentPlayer != null ? currentPlayer.name : "Player";
                ControlsUI.Instance.ShowMapView(playerName);
            }
        }
        else
        {
            desiredPosition = currentFollowPoint + followOffset;

            if (ControlsUI.Instance != null)
            {
                ControlsUI.Instance.RestoreStateAfterMap();
            }

            if (turnManager != null && ControlsUI.Instance != null)
            {
                ControlsUI.UIState restoredState = ControlsUI.Instance.GetCurrentState();

                if (restoredState == ControlsUI.UIState.Normal)
                {
                    turnManager.ShowDiceVisual();
                }
            }
        }
    }

    private void HandleMapMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.W)) h -= 1f;
        if (Input.GetKey(KeyCode.S)) h += 1f;
        if (Input.GetKey(KeyCode.D)) v += 1f;
        if (Input.GetKey(KeyCode.A)) v -= 1f;

        Vector3 move = new Vector3(h, 0f, v).normalized * mapMoveSpeed * Time.deltaTime;
        desiredPosition += move;
    }
}