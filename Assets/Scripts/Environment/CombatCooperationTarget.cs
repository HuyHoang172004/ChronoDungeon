using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Health), typeof(EnemyAttackTarget))]
public sealed class CombatCooperationTarget : MonoBehaviour, ITimeLoopResettable
{
    public bool GhostDamaged { get; private set; }
    public bool PlayerDamaged { get; private set; }
    public event Action StateChanged;

    public void NotifyDamage(bool fromGhost)
    {
        if (fromGhost) GhostDamaged = true;
        else PlayerDamaged = true;
        StateChanged?.Invoke();
    }

    public void CaptureInitialState() => ResetToInitialState();

    public void ResetToInitialState()
    {
        GhostDamaged = false;
        PlayerDamaged = false;
        StateChanged?.Invoke();
    }
}
