using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PoolMember : MonoBehaviour
{
    public ObjectPoolingManager Owner { get; private set; }
    public PooledObjectInfo Pool { get; private set; }
    public bool IsRented { get; private set; }

    internal void Bind(ObjectPoolingManager owner, PooledObjectInfo pool)
    {
        Owner = owner;
        Pool = pool;
        IsRented = false;
    }

    internal bool TryMarkRented()
    {
        if (Owner == null || Pool == null ||
            Pool.IsClearing || IsRented)
            return false;

        IsRented = true;
        return true;
    }

    internal bool TryMarkReturned()
    {
        if (!IsRented)
            return false;

        IsRented = false;
        return true;
    }
}
