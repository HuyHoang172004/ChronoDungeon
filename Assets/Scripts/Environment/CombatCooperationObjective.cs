using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CombatCooperationObjective : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private CombatCooperationTarget ghostTarget;
    [SerializeField] private CombatCooperationTarget playerTarget;
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private Color incompleteColor = new Color(1f, .45f, .2f, .85f);
    [SerializeField] private Color completeColor = new Color(.3f, 1f, .75f, .95f);
    private bool wasComplete;

    public bool IsComplete => ghostTarget != null && playerTarget != null &&
        ghostTarget.GhostDamaged && playerTarget.PlayerDamaged;
    public CombatCooperationTarget GhostTarget => ghostTarget;
    public CombatCooperationTarget PlayerTarget => playerTarget;
    public event Action<bool> StateChanged;

    private void OnEnable()
    {
        if (ghostTarget != null) ghostTarget.StateChanged += Refresh;
        if (playerTarget != null) playerTarget.StateChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (ghostTarget != null) ghostTarget.StateChanged -= Refresh;
        if (playerTarget != null) playerTarget.StateChanged -= Refresh;
    }

    private void Refresh()
    {
        bool complete = IsComplete;
        if (indicator != null) indicator.color = complete ? completeColor : incompleteColor;
        if (complete == wasComplete) return;
        wasComplete = complete;
        StateChanged?.Invoke(complete);
    }

    public void CaptureInitialState() => ResetToInitialState();

    public void ResetToInitialState()
    {
        wasComplete = false;
        Refresh();
    }
}
