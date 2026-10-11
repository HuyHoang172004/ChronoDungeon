#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MapRuntimeCatalogBuilder
{
    private const string ResourceFolder = "Assets/Resources";
    private const string AssetPath = ResourceFolder + "/MapRuntimeCatalog.asset";
    private static readonly string[] MapPaths =
    {
        "Assets/ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_Modular3Q_Master_v01.png",
        "Assets/ArtSource/Maps/Map02_ForgottenBarracks/Master/CD_Map02_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map03_OvergrownWilds/Master/CD_Map03_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map04_EchoLibrary/Master/CD_Map04_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map05_AshForge/Master/CD_Map05_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map06_ShatteredMirrorPrison/Master/CD_Map06_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map07_WardenApproach/Master/CD_Map07_Master_Base_Composition_v01.png",
        "Assets/ArtSource/Maps/Map08_ChronoSanctum/Master/CD_Map08_Master_Base_Composition_v01.png"
    };

    private static bool rebuildQueued;

    static MapRuntimeCatalogBuilder() => QueueRebuild();

    public static void QueueRebuild()
    {
        if (rebuildQueued) return;
        rebuildQueued = true;
        EditorApplication.delayCall += () =>
        {
            rebuildQueued = false;
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating) Rebuild();
            else QueueRebuild();
        };
    }

    [MenuItem("ChronoDungeon/Rebuild Modular Maps")]
    public static void Rebuild()
    {
        if (!AssetDatabase.IsValidFolder(ResourceFolder))
            AssetDatabase.CreateFolder("Assets", "Resources");

        // Building a modular definition can create/import an asset. That operation may
        // invalidate Unity object wrappers acquired before it, especially during an
        // automatic refresh. Build dependencies first, then acquire the catalog once.
        ModularMapDefinition[] modularMaps = new ModularMapDefinition[MapPaths.Length];
        modularMaps[1] = BuildMap02();
        modularMaps[2] = BuildMap03();
        modularMaps[3] = BuildMap04();
        modularMaps[4] = BuildMap05();
        modularMaps[5] = BuildMap06();
        modularMaps[6] = BuildMap07();
        modularMaps[7] = BuildMap08();

        MapRuntimeCatalog catalog = LoadOrReplaceInvalidAsset<MapRuntimeCatalog>(AssetPath);

        if (catalog == null)
        {
            Debug.LogError("Unable to create or reload MapRuntimeCatalog at " + AssetPath + ".");
            return;
        }

        catalog.maps = new Sprite[MapPaths.Length];
        catalog.modularMaps = modularMaps;
        for (int i = 0; i < MapPaths.Length; i++)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(MapPaths[i]);
            Sprite largest = null;
            float largestArea = -1f;
            foreach (Object asset in assets)
            {
                if (asset is not Sprite sprite) continue;
                float area = sprite.rect.width * sprite.rect.height;
                if (area <= largestArea) continue;
                largest = sprite;
                largestArea = area;
            }
            catalog.maps[i] = largest;
            // Maps 4-8 remain null until their newly-authored modules are complete.
            // Never load the retired assets that were produced by splitting old masters.
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("ChronoDungeon/Validate Modular Maps")]
    private static void ValidateModularMaps()
    {
        Rebuild();
        MapRuntimeCatalog catalog = AssetDatabase.LoadAssetAtPath<MapRuntimeCatalog>(AssetPath);
        int errorCount = 0;
        for (int i = 1; i < catalog.modularMaps.Length; i++)
        {
            ModularMapDefinition map = catalog.modularMaps[i];
            if (map == null) continue;
            string label = "Map " + (i + 1).ToString("00");
            if (map.background == null) { Debug.LogError(label + " has no non-walkable background."); errorCount++; }
            if (map.walkableMask == null) { Debug.LogError(label + " has no walkable mask."); errorCount++; }
            else if (!map.walkableMask.isReadable) { Debug.LogError(label + " mask is not readable."); errorCount++; }
            if (map.worldSize.x <= 0f || map.worldSize.y <= 0f)
            { Debug.LogError(label + " has invalid world size."); errorCount++; }
            if (map.tiles == null || map.tiles.Count < 2)
            { Debug.LogError(label + " must contain multiple authored ground modules."); errorCount++; }
            else
            {
                for (int tile = 0; tile < map.tiles.Count; tile++)
                    if (map.tiles[tile] == null || map.tiles[tile].ground == null)
                    { Debug.LogError(label + " has a missing ground sprite at tile " + tile + "."); errorCount++; }
            }
            if (map.spawnNormalized.x < 0f || map.spawnNormalized.x > 1f ||
                map.spawnNormalized.y < 0f || map.spawnNormalized.y > 1f)
            { Debug.LogError(label + " spawnNormalized is outside its map."); errorCount++; }
        }
        if (errorCount == 0) Debug.Log("Modular map validation passed for all completed maps.");
        else Debug.LogError("Modular map validation failed with " + errorCount + " issue(s).");
    }

    private static ModularMapDefinition BuildMap02()
    {
        const string source = "Assets/ArtSource/Maps/Map02_ForgottenBarracks/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map02";
        const string output = outputFolder + "/Map02_Modular.asset";

        EnsureFolder("Assets", "Generated");
        EnsureFolder(generatedRoot, "ModularMaps");
        EnsureFolder(generatedMaps, "Map02");

        string backgroundPath = source + "/Background/CD_Map02_Background_Abyss_v02.png";
        string maskPath = source + "/WalkableMask/CD_Map02_Combined_WalkableMask.png";
        string room01Path = source + "/Ground/CD_Map02_Room01_Entrance_WE_v02.png";
        string room02Path = source + "/Ground/CD_Map02_Room02_Command_WES_v02.png";
        string room03Path = source + "/Ground/CD_Map02_Room03_Boss_N_v02.png";
        string room04Path = source + "/Ground/CD_Map02_Room04_Armory_W_v02.png";

        ConfigureSprite(backgroundPath, false);
        ConfigureSprite(room01Path, true);
        ConfigureSprite(room02Path, true);
        ConfigureSprite(room03Path, true);
        ConfigureSprite(room04Path, true);
        ConfigureMask(maskPath);

        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);

        definition.zoneName = "Zone02_ForgottenBarracks";
        definition.worldSize = new Vector2(37.62f, 25.08f);
        definition.spawnNormalized = new Vector2(.045f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(room01Path, new Vector2(-12.54f, 6.27f)),
            Tile(room02Path, new Vector2(0f, 6.27f)),
            Tile(room04Path, new Vector2(12.54f, 6.27f)),
            Tile(room03Path, new Vector2(0f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap03()
    {
        const string source = "Assets/ArtSource/Maps/Map03_OvergrownWilds/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map03";
        const string output = outputFolder + "/Map03_Modular.asset";
        EnsureFolder("Assets", "Generated");
        EnsureFolder(generatedRoot, "ModularMaps");
        EnsureFolder(generatedMaps, "Map03");

        string backgroundPath = source + "/Background/CD_Map03_Background_ForestChasm_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map03_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map03_Room01_Entrance_WE_v01.png";
        string grovePath = source + "/Ground/CD_Map03_Room02_Grove_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map03_Room03_Boss_N_v01.png";
        string shrinePath = source + "/Ground/CD_Map03_Room04_Shrine_W_v01.png";
        ConfigureSprite(backgroundPath, false);
        ConfigureSprite(entrancePath, true);
        ConfigureSprite(grovePath, true);
        ConfigureSprite(bossPath, true);
        ConfigureSprite(shrinePath, true);
        ConfigureMask(maskPath);

        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone03_OvergrownWilds";
        definition.worldSize = new Vector2(42.09f, 23.75f);
        definition.spawnNormalized = new Vector2(.025f, .765f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-14.03f, 6.27f)),
            Tile(grovePath, new Vector2(0f, 6.27f)),
            Tile(shrinePath, new Vector2(14.03f, 6.27f)),
            Tile(bossPath, new Vector2(0f, -5.605f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap04()
    {
        const string source = "Assets/ArtSource/Maps/Map04_EchoLibrary/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map04";
        const string output = outputFolder + "/Map04_Modular.asset";
        EnsureFolder("Assets", "Generated");
        EnsureFolder(generatedRoot, "ModularMaps");
        EnsureFolder(generatedMaps, "Map04");

        string backgroundPath = source + "/Background/CD_Map04_Background_EndlessStacks_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map04_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map04_Room01_Entrance_WE_v01.png";
        string archivePath = source + "/Ground/CD_Map04_Room02_Archive_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map04_Room03_Boss_N_v01.png";
        string sidePath = source + "/Ground/CD_Map04_Room04_Scriptorium_W_v01.png";
        ConfigureSprite(backgroundPath, false);
        ConfigureSprite(entrancePath, true);
        ConfigureSprite(archivePath, true);
        ConfigureSprite(bossPath, true);
        ConfigureSprite(sidePath, true);
        ConfigureMask(maskPath);

        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone04_EchoLibrary";
        definition.worldSize = new Vector2(39.11f, 25.08f);
        definition.spawnNormalized = new Vector2(.025f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-12.54f, 6.27f)),
            Tile(archivePath, new Vector2(.745f, 6.27f)),
            Tile(sidePath, new Vector2(13.285f, 6.27f)),
            Tile(bossPath, new Vector2(.745f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap05()
    {
        const string source = "Assets/ArtSource/Maps/Map05_AshForge/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map05";
        const string output = outputFolder + "/Map05_Modular.asset";
        EnsureFolder("Assets", "Generated");
        EnsureFolder(generatedRoot, "ModularMaps");
        EnsureFolder(generatedMaps, "Map05");
        string backgroundPath = source + "/Background/CD_Map05_Background_ForgeAbyss_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map05_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map05_Room01_Entrance_WE_v01.png";
        string centerPath = source + "/Ground/CD_Map05_Room02_Foundry_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map05_Room03_Boss_N_v01.png";
        string sidePath = source + "/Ground/CD_Map05_Room04_Workshop_W_v01.png";
        ConfigureSprite(backgroundPath, false);
        ConfigureSprite(entrancePath, true); ConfigureSprite(centerPath, true);
        ConfigureSprite(bossPath, true); ConfigureSprite(sidePath, true); ConfigureMask(maskPath);
        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone05_AshForge";
        definition.worldSize = new Vector2(39.11f, 25.08f);
        definition.spawnNormalized = new Vector2(.025f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-12.54f, 6.27f)),
            Tile(centerPath, new Vector2(.745f, 6.27f)),
            Tile(sidePath, new Vector2(13.285f, 6.27f)),
            Tile(bossPath, new Vector2(.745f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap06()
    {
        const string source = "Assets/ArtSource/Maps/Map06_ShatteredMirrorPrison/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map06";
        const string output = outputFolder + "/Map06_Modular.asset";
        EnsureFolder("Assets", "Generated"); EnsureFolder(generatedRoot, "ModularMaps"); EnsureFolder(generatedMaps, "Map06");
        string backgroundPath = source + "/Background/CD_Map06_Background_MirrorVoid_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map06_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map06_Room01_Entrance_WE_v01.png";
        string centerPath = source + "/Ground/CD_Map06_Room02_Cellblock_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map06_Room03_Boss_N_v01.png";
        string sidePath = source + "/Ground/CD_Map06_Room04_Interrogation_W_v01.png";
        ConfigureSprite(backgroundPath, false); ConfigureSprite(entrancePath, true);
        ConfigureSprite(centerPath, true); ConfigureSprite(bossPath, true);
        ConfigureSprite(sidePath, true); ConfigureMask(maskPath);
        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone06_ShatteredMirrorPrison";
        definition.worldSize = new Vector2(39.11f, 25.08f);
        definition.spawnNormalized = new Vector2(.025f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-12.54f, 6.27f)), Tile(centerPath, new Vector2(.745f, 6.27f)),
            Tile(sidePath, new Vector2(13.285f, 6.27f)), Tile(bossPath, new Vector2(.745f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap07()
    {
        const string source = "Assets/ArtSource/Maps/Map07_WardenApproach/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map07";
        const string output = outputFolder + "/Map07_Modular.asset";
        EnsureFolder("Assets", "Generated"); EnsureFolder(generatedRoot, "ModularMaps"); EnsureFolder(generatedMaps, "Map07");
        string backgroundPath = source + "/Background/CD_Map07_Background_FinalCitadel_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map07_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map07_Room01_Entrance_WE_v01.png";
        string centerPath = source + "/Ground/CD_Map07_Room02_Rampart_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map07_Room03_Boss_N_v01.png";
        string sidePath = source + "/Ground/CD_Map07_Room04_GuardPost_W_v01.png";
        ConfigureSprite(backgroundPath, false); ConfigureSprite(entrancePath, true);
        ConfigureSprite(centerPath, true); ConfigureSprite(bossPath, true);
        ConfigureSprite(sidePath, true); ConfigureMask(maskPath);
        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone07_WardenApproach";
        definition.worldSize = new Vector2(39.11f, 25.08f);
        definition.spawnNormalized = new Vector2(.025f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-12.54f, 6.27f)), Tile(centerPath, new Vector2(.745f, 6.27f)),
            Tile(sidePath, new Vector2(13.285f, 6.27f)), Tile(bossPath, new Vector2(.745f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapDefinition BuildMap08()
    {
        const string source = "Assets/ArtSource/Maps/Map08_ChronoSanctum/ModularNew";
        const string generatedRoot = "Assets/Generated";
        const string generatedMaps = generatedRoot + "/ModularMaps";
        const string outputFolder = generatedMaps + "/Map08";
        const string output = outputFolder + "/Map08_Modular.asset";
        EnsureFolder("Assets", "Generated"); EnsureFolder(generatedRoot, "ModularMaps"); EnsureFolder(generatedMaps, "Map08");
        string backgroundPath = source + "/Background/CD_Map08_Background_TimeVoid_v01.png";
        string maskPath = source + "/WalkableMask/CD_Map08_Combined_WalkableMask.png";
        string entrancePath = source + "/Ground/CD_Map08_Room01_Entrance_WE_v01.png";
        string centerPath = source + "/Ground/CD_Map08_Room02_Observatory_WES_v01.png";
        string bossPath = source + "/Ground/CD_Map08_Room03_FinalBoss_N_v01.png";
        string sidePath = source + "/Ground/CD_Map08_Room04_Chronicle_W_v01.png";
        ConfigureSprite(backgroundPath, false); ConfigureSprite(entrancePath, true);
        ConfigureSprite(centerPath, true); ConfigureSprite(bossPath, true);
        ConfigureSprite(sidePath, true); ConfigureMask(maskPath);
        ModularMapDefinition definition = LoadOrReplaceInvalidAsset<ModularMapDefinition>(output);
        definition.zoneName = "Zone08_ChronoSanctum";
        definition.worldSize = new Vector2(39.11f, 25.08f);
        definition.spawnNormalized = new Vector2(.025f, .75f);
        definition.background = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
        definition.walkableMask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        definition.tiles = new System.Collections.Generic.List<ModularMapTile>
        {
            Tile(entrancePath, new Vector2(-12.54f, 6.27f)), Tile(centerPath, new Vector2(.745f, 6.27f)),
            Tile(sidePath, new Vector2(13.285f, 6.27f)), Tile(bossPath, new Vector2(.745f, -6.27f))
        };
        definition.colliderPaths = new System.Collections.Generic.List<ModularMapColliderPath>();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static ModularMapTile Tile(string spritePath, Vector2 position) => new ModularMapTile
    {
        ground = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath),
        localPosition = position
    };

    private static void ConfigureSprite(string path, bool transparent)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
        bool changed = importer.textureType != TextureImporterType.Sprite ||
            importer.spritePixelsPerUnit != 100f || importer.mipmapEnabled ||
            importer.textureCompression != TextureImporterCompression.Uncompressed ||
            importer.alphaIsTransparency != transparent;
        if (!changed) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = transparent;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void ConfigureMask(string path)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
        bool changed = importer.textureType != TextureImporterType.Default || !importer.isReadable ||
            importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed;
        if (!changed) return;
        importer.textureType = TextureImporterType.Default;
        importer.isReadable = true;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static T LoadOrReplaceInvalidAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;

        // A file may exist but have m_Script: {fileID: 0} when it was created
        // during compilation. It is generated data, so rebuild it with the current
        // script GUID rather than leaving a Missing Script reference in the project.
        // Missing-script ScriptableObjects often return null from LoadMainAssetAtPath
        // even though their YAML file still occupies the path. Check the file itself,
        // then delete that exact generated asset before recreating it.
        string absolutePath = Path.GetFullPath(path);
        if (File.Exists(absolutePath))
        {
            AssetDatabase.DeleteAsset(path);
            if (File.Exists(absolutePath))
                throw new IOException("Could not replace invalid generated asset: " + path);
        }
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}

/// <summary>
/// Auto Refresh may be disabled while new map PNGs are written. Rebuild only after
/// Unity has imported those files, so definitions never capture null sprites/masks.
/// </summary>
public sealed class ModularMapAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < importedAssets.Length; i++)
        {
            string path = importedAssets[i].Replace('\\', '/');
            if (!path.Contains("/Maps/Map") || !path.Contains("/ModularNew/")) continue;
            MapRuntimeCatalogBuilder.QueueRebuild();
            return;
        }
    }
}
#endif
