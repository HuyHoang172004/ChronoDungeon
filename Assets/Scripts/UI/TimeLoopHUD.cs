using TMPro;
using UnityEngine;

public class TimeLoopHUD : MonoBehaviour
{
    [SerializeField] private TimeLoopManager loop;
    [SerializeField] private TMP_Text label;

    private void LateUpdate()
    {
        if (loop != null && label != null)
            label.SetText("TIME: {0:1}\nLOOP: {1:0}", loop.remainingTime, loop.loopIndex);
    }
}
