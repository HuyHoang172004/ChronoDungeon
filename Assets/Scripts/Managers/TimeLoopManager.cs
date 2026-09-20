using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float loopDuration = 20f;
    [SerializeField] private GameManager gameManager;
    private readonly List<MonoBehaviour> resettables = new List<MonoBehaviour>();

    public float remainingTime { get; private set; }
    public int loopIndex { get; private set; } = 1;
    public bool IsRewinding { get; private set; }
    public event Action LoopRewound;

    private void Awake() => remainingTime = loopDuration;

    private void Start()
    {
        // Capture once, including inactive objects, and retain references through death.
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!(component is ITimeLoopResettable resettable)) continue;
            resettable.CaptureInitialState();
            resettables.Add(component);
        }
    }

    private void Update()
    {
        if (IsRewinding || Time.timeScale <= 0f ||
            (gameManager != null && gameManager.IsGameOver)) return;

        // Scaled gameplay time: pause/freeze stop the timer; slow motion slows it.
        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        if (remainingTime <= 0f) Rewind();
    }

    private void Rewind()
    {
        IsRewinding = true;
        try
        {
            foreach (MonoBehaviour component in resettables)
                if (component != null)
                    ((ITimeLoopResettable)component).ResetToInitialState();

            Physics2D.SyncTransforms();
            loopIndex++;
            remainingTime = loopDuration;
        }
        finally
        {
            IsRewinding = false;
        }
        LoopRewound?.Invoke();
    }
}
