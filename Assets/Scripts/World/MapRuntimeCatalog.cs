using UnityEngine;

/// <summary>Build-safe Resources catalog. Kept in a matching file so Unity can serialize its script GUID.</summary>
public sealed class MapRuntimeCatalog : ScriptableObject
{
    public Sprite[] maps;
    public ModularMapDefinition[] modularMaps;
}
