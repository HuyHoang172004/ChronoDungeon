using System.Collections.Generic;
using UnityEngine;

/// <summary>Serialized modular-map data. Unity requires this ScriptableObject to live in a matching file.</summary>
public sealed class ModularMapDefinition : ScriptableObject
{
    public string zoneName;
    public Vector2 worldSize;
    public Vector2 spawnNormalized = new Vector2(.12f, .5f);
    public Sprite background;
    public Texture2D walkableMask;
    public List<ModularMapTile> tiles = new();
    public List<ModularMapColliderPath> colliderPaths = new();
}
