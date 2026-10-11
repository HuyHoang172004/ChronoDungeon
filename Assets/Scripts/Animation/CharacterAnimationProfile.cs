using System;
using UnityEngine;

public enum CharacterAnimationState
{
    Spawn,
    Idle,
    Patrol,
    Chase,
    Move,
    Turn,
    Windup,
    Attack,
    Cast,
    Dash,
    Recover,
    Guard,
    Block,
    Hit,
    Stagger,
    Knockback,
    Enraged,
    PhaseTransition,
    Death
}

[Serializable]
public struct CharacterAnimationStateAlias
{
    public CharacterAnimationState state;
    public string clipStem;
}

[CreateAssetMenu(menuName = "ChronoDungeon/Animation/Character Animation Profile",
    fileName = "CharacterAnimationProfile")]
public sealed class CharacterAnimationProfile : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private RuntimeAnimatorController controller;
    [SerializeField] private bool hasIndependentLeftFrames = true;
    [SerializeField] private bool supportsEightDirections;
    [SerializeField] private CharacterAnimationStateAlias[] aliases;

    public string CharacterId => characterId;
    public RuntimeAnimatorController Controller => controller;
    public bool HasIndependentLeftFrames => hasIndependentLeftFrames;
    public bool SupportsEightDirections => supportsEightDirections;

    public string StateName(CharacterAnimationState state, string direction)
    {
        string stem = state.ToString();
        if (aliases != null)
        {
            for (int i = 0; i < aliases.Length; i++)
                if (aliases[i].state == state && !string.IsNullOrWhiteSpace(aliases[i].clipStem))
                {
                    stem = aliases[i].clipStem.Trim();
                    break;
                }
        }
        return stem + "_" + direction;
    }
}
