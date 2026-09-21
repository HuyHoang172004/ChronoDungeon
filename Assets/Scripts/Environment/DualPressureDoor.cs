using UnityEngine;

// Reusable mechanism adapter: this component knows only its referenced inputs
// and output, so another pair of switches or another Door can be assigned.
[DisallowMultipleComponent]
public sealed class DualPressureDoor : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PressureSwitch first;
    [SerializeField] private PressureSwitch second;
    [SerializeField] private Door output;
    [SerializeField] private bool resetOutputOnRewind = true;
    public bool IsSolved { get; private set; }

    private void OnEnable()
    {
        if (first != null) first.StateChanged += OnInputChanged;
        if (second != null) second.StateChanged += OnInputChanged;
        Evaluate();
    }

    private void OnDisable()
    {
        if (first != null) first.StateChanged -= OnInputChanged;
        if (second != null) second.StateChanged -= OnInputChanged;
    }

    private void OnInputChanged(bool _) => Evaluate();

    public void Evaluate()
    {
        IsSolved = first != null && second != null && first.IsActive && second.IsActive;
        if (output != null) output.SetOpen(IsSolved);
    }

    public void CaptureInitialState() { }

    public void ResetToInitialState()
    {
        IsSolved = false;
        if (resetOutputOnRewind && output != null) output.Close();
    }
}
