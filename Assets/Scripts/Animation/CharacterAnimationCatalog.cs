using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ChronoDungeon/Animation/Character Animation Catalog",
    fileName = "CharacterAnimationCatalog")]
public sealed class CharacterAnimationCatalog : ScriptableObject
{
    [SerializeField] private CharacterAnimationProfile[] profiles = Array.Empty<CharacterAnimationProfile>();

    public CharacterAnimationProfile[] Profiles => profiles;

    public CharacterAnimationProfile Find(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || profiles == null) return null;
        for (int i = 0; i < profiles.Length; i++)
            if (profiles[i] != null && string.Equals(profiles[i].CharacterId, characterId,
                StringComparison.OrdinalIgnoreCase)) return profiles[i];
        return null;
    }
}
