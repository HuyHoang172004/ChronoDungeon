using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAnimationEventRelay : MonoBehaviour
{
    public event Action Impact;
    public event Action CastRelease;
    public event Action Footstep;
    public event Action AnimationCompleted;

    public void OnAnimationImpact() => Impact?.Invoke();
    public void OnAnimationCastRelease() => CastRelease?.Invoke();
    public void OnAnimationFootstep() => Footstep?.Invoke();
    public void OnAnimationCompleted() => AnimationCompleted?.Invoke();
}
