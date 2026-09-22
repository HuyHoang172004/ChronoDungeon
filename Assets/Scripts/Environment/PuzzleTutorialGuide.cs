using UnityEngine;

[DisallowMultipleComponent]
public sealed class PuzzleTutorialGuide : MonoBehaviour, ITimeLoopResettable
{
    [SerializeField] private PressureSwitch first;
    [SerializeField] private PressureSwitch second;
    [SerializeField] private DualPressureDoor puzzle;
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private TextMesh message;
    [SerializeField] private SpriteRenderer ghostMarker;
    [SerializeField] private Color hintColor = new Color(.55f, .9f, 1f, 1f);
    [SerializeField] private Color solvedColor = new Color(.75f, .55f, 1f, 1f);
    private bool rewound;

    public string CurrentMessage => message == null ? string.Empty : message.text;
    public bool IsShowingFailureHint => rewound && puzzle != null && !puzzle.IsSolved;

    private void OnEnable()
    {
        if (first != null) first.StateChanged += OnSwitchChanged;
        if (second != null) second.StateChanged += OnSwitchChanged;
        if (loop != null) loop.LoopRewound += OnRewound;
        Refresh();
    }

    private void OnDisable()
    {
        if (first != null) first.StateChanged -= OnSwitchChanged;
        if (second != null) second.StateChanged -= OnSwitchChanged;
        if (loop != null) loop.LoopRewound -= OnRewound;
    }

    private void OnSwitchChanged(bool _) => Refresh();
    private void OnRewound() { rewound = true; Refresh(); }

    private void Refresh()
    {
        if (message == null) return;
        bool solved = puzzle != null && puzzle.IsSolved;
        message.color = solved ? solvedColor : hintColor;
        if (solved)
        {
            message.text = "TIME LINK COMPLETE";
            if (ghostMarker != null) ghostMarker.enabled = false;
            return;
        }
        if (loop != null && loop.loopIndex > 1 && first != null && first.IsActive)
        {
            message.text = "GHOST HOLDS A  •  ACTIVATE B";
            if (ghostMarker != null) ghostMarker.enabled = true;
            return;
        }
        if (rewound)
        {
            message.text = "REWIND COMPLETE  •  LEAVE A GHOST ON A";
            if (ghostMarker != null) ghostMarker.enabled = true;
            return;
        }
        if (first != null && first.IsActive)
        {
            message.text = "HOLD A  •  REWIND  •  FOLLOW YOUR GHOST";
            if (ghostMarker != null) ghostMarker.enabled = true;
            return;
        }
        message.text = "STEP ON A TO BEGIN";
        if (ghostMarker != null) ghostMarker.enabled = true;
    }

    public void CaptureInitialState() => ResetToInitialState();

    public void ResetToInitialState()
    {
        rewound = false;
        Refresh();
    }
}
