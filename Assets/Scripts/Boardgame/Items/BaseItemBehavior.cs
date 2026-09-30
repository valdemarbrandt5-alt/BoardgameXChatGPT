using UnityEngine;

public abstract class BaseItemBehaviour : ScriptableObject
{
    public abstract void BeginUse(ItemUseContext context);
    public abstract void HandleInput();
    public abstract void CancelUse();
}