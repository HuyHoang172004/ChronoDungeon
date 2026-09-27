using System;
using UnityEngine;

public sealed class RuneKeyInventory : MonoBehaviour
{
    public bool HasRuneKey { get; private set; }
    public event Action<bool> KeyChanged;
    public void CollectRuneKey()
    {
        if (HasRuneKey) return;
        HasRuneKey = true;
        KeyChanged?.Invoke(true);
    }
}
