using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialGuideUI : MonoBehaviour
{
    private enum TutorialStep { Movement, Attack, Dash, TimeLoop, Ghost, FirstPuzzle, Complete }

    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerAttack attack;
    [SerializeField] private PlayerDash dash;
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private TemporalGhostManager ghosts;
    [SerializeField] private PuzzleTutorialGuide puzzle;
    private GameObject panel;
    private TMP_Text label;
    private TutorialStep step;
    private float completeUntil;

    public int CurrentStep => (int)step;
    public string CurrentPrompt => label == null ? string.Empty : label.text;
    public bool IsVisible => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (!enabled) return;
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (attack == null && player != null) attack = player.GetComponent<PlayerAttack>();
        if (dash == null && player != null) dash = player.GetComponent<PlayerDash>();
        if (loop == null) loop = FindAnyObjectByType<TimeLoopManager>();
        if (ghosts == null) ghosts = FindAnyObjectByType<TemporalGhostManager>();
        if (puzzle == null) puzzle = FindAnyObjectByType<PuzzleTutorialGuide>();
        BuildPanel();
        Refresh();
    }

    private void OnEnable()
    {
        if (attack != null) attack.AttackPerformed += HandleAttack;
        if (dash != null) dash.DashPerformed += HandleDash;
        if (loop != null) loop.LoopRewound += HandleRewind;
    }

    private void OnDisable()
    {
        if (attack != null) attack.AttackPerformed -= HandleAttack;
        if (dash != null) dash.DashPerformed -= HandleDash;
        if (loop != null) loop.LoopRewound -= HandleRewind;
    }

    private void Update()
    {
        if (step == TutorialStep.Movement && player != null && player.CurrentMoveDirection.sqrMagnitude > 0.01f)
            SetStep(TutorialStep.Attack);
        else if (step == TutorialStep.Ghost && ghosts != null && ghosts.ActiveGhostCount > 0)
            SetStep(TutorialStep.FirstPuzzle);
        else if (step == TutorialStep.FirstPuzzle && puzzle != null && puzzle.CurrentMessage == "TIME LINK COMPLETE")
            SetStep(TutorialStep.Complete);

        if (step == TutorialStep.Complete && Time.unscaledTime >= completeUntil)
            panel.SetActive(false);
    }

    private void HandleAttack(AttackSnapshot _) { if (step == TutorialStep.Attack) SetStep(TutorialStep.Dash); }
    private void HandleDash(Vector2 _) { if (step == TutorialStep.Dash) SetStep(TutorialStep.TimeLoop); }
    private void HandleRewind() { if (step == TutorialStep.TimeLoop) SetStep(TutorialStep.Ghost); }

    private void SetStep(TutorialStep next)
    {
        if (step == next) return;
        step = next;
        Refresh();
        if (step == TutorialStep.Complete) completeUntil = Time.unscaledTime + 2.5f;
    }

    private void Refresh()
    {
        if (label == null) return;
        label.text = step switch
        {
            TutorialStep.Movement => "MOVE  •  DRAG THE LEFT JOYSTICK",
            TutorialStep.Attack => "ATTACK  •  TAP THE ATTACK BUTTON",
            TutorialStep.Dash => "DASH  •  TAP DASH TO EVADE",
            TutorialStep.TimeLoop => "TIME LOOP  •  SURVIVE UNTIL THE TIMER REWINDS",
            TutorialStep.Ghost => "GHOST  •  YOUR LAST LOOP NOW REPLAYS",
            TutorialStep.FirstPuzzle => "TEMPORAL PUZZLE  •  LET YOUR GHOST HOLD A, THEN ACTIVATE B",
            _ => "TUTORIAL COMPLETE"
        };
        label.color = step == TutorialStep.Complete ? new Color(.75f, .9f, 1f) : Color.white;
        if (panel != null) panel.SetActive(step != TutorialStep.Complete || completeUntil > Time.unscaledTime);
    }

    private void BuildPanel()
    {
        panel = new GameObject("Tutorial Guide", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var rect = panel.GetComponent<RectTransform>();
        // Keep the authored TIME/LOOP readout at the top center unobstructed.
        rect.anchorMin = new Vector2(.18f, .70f); rect.anchorMax = new Vector2(.82f, .80f);
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.03f, .12f, .2f, .88f);
        label = new GameObject("Tutorial Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
        label.transform.SetParent(panel.transform, false);
        var textRect = label.rectTransform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 4f); textRect.offsetMax = new Vector2(-18f, -4f);
        label.alignment = TextAlignmentOptions.Center; label.fontSize = 22f; label.enableAutoSizing = true;
        label.fontSizeMin = 13f; label.fontSizeMax = 22f; label.raycastTarget = false;
    }
}
