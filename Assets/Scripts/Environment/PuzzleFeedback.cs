using UnityEngine;

// Presentation-only adapter for the authored dual-switch encounter. Gameplay
// remains in PressureSwitch/DualPressureDoor/Door; this component only listens.
[DisallowMultipleComponent, RequireComponent(typeof(LineRenderer))]
public sealed class PuzzleFeedback : MonoBehaviour
{
    [SerializeField] private PressureSwitch first;
    [SerializeField] private PressureSwitch second;
    [SerializeField] private Door door;
    [SerializeField] private SpriteRenderer firstLight;
    [SerializeField] private SpriteRenderer secondLight;
    [SerializeField] private SpriteRenderer doorLight;
    [SerializeField] private TextMesh hint;
    [SerializeField] private Color inactiveLine = new Color(0.15f, 0.35f, 0.45f, 0.35f);
    [SerializeField] private Color activeLine = new Color(0.2f, 1f, 1f, 0.95f);
    [SerializeField] private Color solvedLine = new Color(0.8f, 0.35f, 1f, 1f);
    [SerializeField] private float pulseAmount = 0.08f;
    [SerializeField] private float pulseSpeed = 4f;
    private LineRenderer line;
    private Vector3 firstScale, secondScale, doorScale;
    private AudioSource audioSource;
    private AudioClip switchClip, doorClip;
    private bool previousFirst, previousSecond, previousDoor;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 3;
        line.useWorldSpace = true;
        line.widthMultiplier = 0.035f;
        line.sortingOrder = -2;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        switchClip = MakeTone("SwitchFeedback", 520f, 0.09f);
        doorClip = MakeTone("DoorFeedback", 740f, 0.14f);
        if (firstLight != null) firstScale = firstLight.transform.localScale;
        if (secondLight != null) secondScale = secondLight.transform.localScale;
        if (doorLight != null) doorScale = doorLight.transform.localScale;
        Subscribe();
        Refresh(true);
    }

    private void Subscribe()
    {
        if (first != null) first.StateChanged += OnSwitch;
        if (second != null) second.StateChanged += OnSwitch;
        if (door != null) door.StateChanged += OnDoor;
    }

    private void OnDestroy()
    {
        if (first != null) first.StateChanged -= OnSwitch;
        if (second != null) second.StateChanged -= OnSwitch;
        if (door != null) door.StateChanged -= OnDoor;
    }

    private void OnSwitch(bool _) { Refresh(false); if (switchClip != null) audioSource.PlayOneShot(switchClip); }
    private void OnDoor(bool open) { Refresh(false); if (open && doorClip != null) audioSource.PlayOneShot(doorClip); }

    private void Update()
    {
        float wave = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        if (firstLight != null) firstLight.transform.localScale = firstScale * (first.IsActive ? wave : 1f);
        if (secondLight != null) secondLight.transform.localScale = secondScale * (second.IsActive ? wave : 1f);
        if (doorLight != null) doorLight.transform.localScale = doorScale * (door.IsOpen ? wave : 1f);
    }

    private void Refresh(bool initial)
    {
        bool a = first != null && first.IsActive;
        bool b = second != null && second.IsActive;
        bool open = door != null && door.IsOpen;
        line.SetPosition(0, first == null ? transform.position : first.transform.position);
        line.SetPosition(1, door == null ? transform.position : door.transform.position);
        line.SetPosition(2, second == null ? transform.position : second.transform.position);
        line.startColor = line.endColor = a && b && open ? solvedLine : (a || b ? activeLine : inactiveLine);
        line.enabled = a || b || open;
        if (hint != null)
            hint.text = a && b && open ? "TIME LINK COMPLETE" : a || b ? "ONE MORE SWITCH" : "STAND ON BOTH TIME PLATES";
        if (!initial && !a && !b && !open) line.enabled = false;
        previousFirst = a; previousSecond = b; previousDoor = open;
    }

    private static AudioClip MakeTone(string name, float frequency, float duration)
    {
        int rate = 22050, samples = Mathf.CeilToInt(rate * duration);
        var clip = AudioClip.Create(name, samples, 1, rate, false);
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * (1f - i / (float)samples) * 0.12f;
        clip.SetData(data, 0);
        return clip;
    }
}
