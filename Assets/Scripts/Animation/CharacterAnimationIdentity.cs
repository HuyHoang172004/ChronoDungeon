using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAnimationIdentity : MonoBehaviour
{
    [SerializeField] private string characterId;
    public string CharacterId => characterId;

    public void Configure(string value) => characterId = value != null ? value.Trim() : string.Empty;
}

public static class CharacterAnimationResolver
{
    private static CharacterAnimationCatalog catalog;

    public static CharacterAnimationProfile Resolve(GameObject target)
    {
        if (target == null) return null;
        if (catalog == null) catalog = Resources.Load<CharacterAnimationCatalog>("CharacterAnimationCatalog");
        if (catalog == null) return null;

        CharacterAnimationIdentity identity = target.GetComponent<CharacterAnimationIdentity>();
        if (identity == null) identity = target.GetComponentInParent<CharacterAnimationIdentity>();
        if (identity != null && !string.IsNullOrWhiteSpace(identity.CharacterId))
        {
            CharacterAnimationProfile exact = catalog.Find(identity.CharacterId);
            if (exact != null) return exact;
        }

        string selectedMapGuess = GuessCharacterId(target);
        CharacterAnimationProfile selectedMapProfile = catalog.Find(selectedMapGuess);
        if (selectedMapProfile != null) return selectedMapProfile;

        string search = target.name;
        SpriteRenderer renderer = target.GetComponentInChildren<SpriteRenderer>(true);
        if (renderer != null && renderer.sprite != null) search += " " + renderer.sprite.name;
        CharacterAnimationProfile[] profiles = catalog.Profiles;
        if (profiles == null) return null;
        for (int i = 0; i < profiles.Length; i++)
        {
            CharacterAnimationProfile candidate = profiles[i];
            if (candidate == null || string.IsNullOrWhiteSpace(candidate.CharacterId)) continue;
            string shortName = candidate.CharacterId.Substring(candidate.CharacterId.LastIndexOf('_') + 1);
            if (search.IndexOf(shortName, StringComparison.OrdinalIgnoreCase) >= 0) return candidate;
        }
        return null;
    }

    public static string GuessCharacterId(GameObject target, string mapId = null)
    {
        if (target == null) return string.Empty;
        if (catalog == null) catalog = Resources.Load<CharacterAnimationCatalog>("CharacterAnimationCatalog");
        if (string.IsNullOrWhiteSpace(mapId))
            mapId = "Map" + (MapSelectionRuntime.SelectedMap + 1).ToString("00");
        string search = target.name;
        SpriteRenderer renderer = target.GetComponentInChildren<SpriteRenderer>(true);
        if (renderer != null && renderer.sprite != null) search += " " + renderer.sprite.name;
        CharacterAnimationProfile[] profiles = catalog != null ? catalog.Profiles : null;
        if (profiles != null)
        {
            string mapToken = "_" + mapId + "_";
            for (int i = 0; i < profiles.Length; i++)
            {
                CharacterAnimationProfile candidate = profiles[i];
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.CharacterId) ||
                    candidate.CharacterId.IndexOf(mapToken, StringComparison.OrdinalIgnoreCase) < 0) continue;
                string shortName = candidate.CharacterId.Substring(candidate.CharacterId.LastIndexOf('_') + 1);
                if (search.IndexOf(shortName, StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate.CharacterId;
            }
        }
        // Do not silently assign RuinHusk to an unknown character. An empty identity
        // leaves the existing prefab controller untouched and makes validation expose
        // the missing mapping instead of displaying the wrong enemy.
        return string.Empty;
    }
}
