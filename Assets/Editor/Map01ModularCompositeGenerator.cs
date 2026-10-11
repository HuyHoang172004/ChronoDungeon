#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Deterministically composes the approved 3/4 Map 01 modules and a separate
/// walkable-only alpha mask. Keeping the mask separate prevents cliff faces,
/// pillars and walls in the presentation art from becoming walkable.
/// </summary>
public static class Map01ModularCompositeGenerator
{
    public const string OutputMapPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Master/CD_Map01_Modular3Q_Master_v01.png";
    public const string OutputCollisionPath = "Assets/ArtSource/Maps/Map01_RuinedEntrance/Collision/CD_Map01_Modular3Q_Walkable_v01.png";

    private const int CanvasWidth = 4562;
    private const int CanvasHeight = 3645;
    private const float OutputScale = 0.8f;

    private readonly struct Piece
    {
        public readonly string path;
        public readonly Vector2Int position;
        public Piece(string path, int x, int y)
        {
            this.path = path;
            position = new Vector2Int(x, y);
        }
    }

    [MenuItem("Tools/ChronoDungeon/Maps/Generate Map01 Modular Composite")]
    public static void Generate()
    {
        Piece[] pieces =
        {
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner01_Start_Draft_v01.png", 0, 160),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner02_EastStairs_Draft_v01.png", 1490, 0),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner04_SquareBastion_Draft_v02.png", 2986, 90),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner03_SouthStairs_Draft_v01.png", 245, 1170),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner05_LShapeStairs_Draft_v02.png", 390, 2506),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_Corner06_DiamondTerrace_Draft_v01.png", 1912, 2581),
            new Piece("Assets/ArtSource/Maps/Map01_RuinedEntrance/Review_3Q_Corners/CD_Map01_FinalBossArena_Draft_v01.png", 2054, 1559)
        };

        int outputWidth = Mathf.RoundToInt(CanvasWidth * OutputScale);
        int outputHeight = Mathf.RoundToInt(CanvasHeight * OutputScale);
        var output = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false, false);
        var clear = new Color32[outputWidth * outputHeight];
        output.SetPixels32(clear);

        foreach (Piece piece in pieces)
        {
            Texture2D source = LoadReadableCopy(piece.path);
            BlitScaled(source, output,
                Mathf.RoundToInt(piece.position.x * OutputScale),
                Mathf.RoundToInt((CanvasHeight - piece.position.y - source.height) * OutputScale),
                OutputScale);
            UnityEngine.Object.DestroyImmediate(source);
        }
        output.Apply(false, false);
        SavePng(output, OutputMapPath);
        UnityEngine.Object.DestroyImmediate(output);

        Texture2D mask = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false, true);
        mask.SetPixels32(new Color32[outputWidth * outputHeight]);
        Color32 walkable = new Color32(255, 255, 255, 255);

        // Corner 1: opening courtyard plus east/south links.
        FillPolygon(mask, walkable, P(0,160, new [] { V(130,240),V(440,100),V(700,190),V(980,170),V(1260,280),V(1300,680),V(1080,790),V(900,750),V(900,1024),V(710,1024),V(710,760),V(440,800),V(170,660) }));
        FillRect(mask, walkable, 1120, 160+360, 1536, 160+570);

        // Corner 2: two side bridges, central court and raised internal terrace.
        FillPolygon(mask, walkable, P(1490,0, new [] { V(0,500),V(390,500),V(480,300),V(880,210),V(1210,350),V(1210,760),V(720,860),V(350,770),V(350,680),V(0,680) }));
        FillRect(mask, walkable, 1490+1120, 500, 1490+1536, 680);
        FillPolygon(mask, walkable, P(1490,0, new [] { V(820,330),V(1030,80),V(1450,30),V(1500,300),V(1160,420) }));

        // Corner 4: terminal square room; only the left connector remains.
        FillRect(mask, walkable, 2986, 90+420, 2986+430, 90+610);
        FillPolygon(mask, walkable, P(2986,90, new [] { V(330,390),V(450,170),V(1110,150),V(1200,390),V(1170,790),V(430,820),V(330,610) }));

        // Corner 3: vertical transition room.
        FillRect(mask, walkable, 245+440, 1170, 245+700, 1170+1367);
        FillPolygon(mask, walkable, P(245,1170, new [] { V(430,210),V(720,210),V(1020,570),V(1080,940),V(830,1110),V(320,1110),V(90,900),V(120,610) }));

        // Corner 5: top entrance, courtyard and right turn; bottom is sealed.
        FillRect(mask, walkable, 390+330, 2506, 390+540, 2506+500);
        FillPolygon(mask, walkable, P(390,2506, new [] { V(130,420),V(650,360),V(830,500),V(840,760),V(650,900),V(120,850) }));
        FillRect(mask, walkable, 390+650, 2506+500, 390+1536, 2506+700);

        // Corner 6: left bridge, angular court and north staircase.
        FillRect(mask, walkable, 1912, 2581+450, 1912+520, 2581+640);
        FillPolygon(mask, walkable, P(1912,2581, new [] { V(400,400),V(760,170),V(1220,280),V(1370,590),V(1120,850),V(610,870),V(360,650) }));
        FillRect(mask, walkable, 1912+790, 2581, 1912+1030, 2581+470);

        // Final boss: single south entrance and a broad closed arena.
        FillRect(mask, walkable, 2054+650, 1559+650, 2054+890, 1559+1024);
        FillPolygon(mask, walkable, P(2054,1559, new [] { V(180,220),V(360,100),V(1170,100),V(1370,240),V(1390,650),V(1120,800),V(420,800),V(150,650) }));

        mask.Apply(false, false);
        SavePng(mask, OutputCollisionPath);
        UnityEngine.Object.DestroyImmediate(mask);
        AssetDatabase.Refresh();
        Debug.Log($"ChronoDungeon: generated modular Map 01 master ({outputWidth}x{outputHeight}) and collision mask.");
    }

    private static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    private static Vector2Int[] P(int ox, int oy, Vector2Int[] local)
    {
        var result = new Vector2Int[local.Length];
        for (int i = 0; i < local.Length; i++) result[i] = new Vector2Int(ox + local[i].x, oy + local[i].y);
        return result;
    }

    private static Texture2D LoadReadableCopy(string assetPath)
    {
        string absolute = Path.GetFullPath(assetPath);
        if (!File.Exists(absolute)) throw new FileNotFoundException("Missing modular map piece", assetPath);
        byte[] bytes = File.ReadAllBytes(absolute);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes, false)) throw new InvalidOperationException("Could not decode " + assetPath);
        return texture;
    }

    private static void BlitScaled(Texture2D source, Texture2D destination, int dx, int dy, float scale)
    {
        int width = Mathf.RoundToInt(source.width * scale);
        int height = Mathf.RoundToInt(source.height * scale);
        Color32[] sourcePixels = source.GetPixels32();
        Color32[] destinationPixels = destination.GetPixels32();
        for (int y = 0; y < height; y++)
        {
            int sy = Mathf.Clamp(Mathf.FloorToInt(y / scale), 0, source.height - 1);
            int targetY = dy + y;
            if (targetY < 0 || targetY >= destination.height) continue;
            for (int x = 0; x < width; x++)
            {
                int targetX = dx + x;
                if (targetX < 0 || targetX >= destination.width) continue;
                int sx = Mathf.Clamp(Mathf.FloorToInt(x / scale), 0, source.width - 1);
                Color32 over = sourcePixels[sy * source.width + sx];
                if (over.a == 0) continue;
                int index = targetY * destination.width + targetX;
                destinationPixels[index] = AlphaBlend(destinationPixels[index], over);
            }
        }
        destination.SetPixels32(destinationPixels);
    }

    private static Color32 AlphaBlend(Color32 under, Color32 over)
    {
        float oa = over.a / 255f;
        float ua = under.a / 255f;
        float alpha = oa + ua * (1f - oa);
        if (alpha <= 0f) return default;
        return new Color32(
            (byte)Mathf.RoundToInt((over.r * oa + under.r * ua * (1f - oa)) / alpha),
            (byte)Mathf.RoundToInt((over.g * oa + under.g * ua * (1f - oa)) / alpha),
            (byte)Mathf.RoundToInt((over.b * oa + under.b * ua * (1f - oa)) / alpha),
            (byte)Mathf.RoundToInt(alpha * 255f));
    }

    private static void FillRect(Texture2D texture, Color32 color, int left, int top, int right, int bottom)
    {
        FillPolygon(texture, color, new [] { V(left,top),V(right,top),V(right,bottom),V(left,bottom) });
    }

    private static void FillPolygon(Texture2D texture, Color32 color, Vector2Int[] originalPoints)
    {
        var points = new Vector2[originalPoints.Length];
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector2(originalPoints[i].x * OutputScale, (CanvasHeight - originalPoints[i].y) * OutputScale);
            minX = Mathf.Min(minX, points[i].x); maxX = Mathf.Max(maxX, points[i].x);
            minY = Mathf.Min(minY, points[i].y); maxY = Mathf.Max(maxY, points[i].y);
        }
        int x0 = Mathf.Clamp(Mathf.FloorToInt(minX), 0, texture.width - 1);
        int x1 = Mathf.Clamp(Mathf.CeilToInt(maxX), 0, texture.width - 1);
        int y0 = Mathf.Clamp(Mathf.FloorToInt(minY), 0, texture.height - 1);
        int y1 = Mathf.Clamp(Mathf.CeilToInt(maxY), 0, texture.height - 1);
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
            if (Contains(points, new Vector2(x + .5f, y + .5f))) texture.SetPixel(x, y, color);
    }

    private static bool Contains(IReadOnlyList<Vector2> polygon, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i], b = polygon[j];
            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    private static void SavePng(Texture2D texture, string assetPath)
    {
        string absolute = Path.GetFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? throw new InvalidOperationException());
        File.WriteAllBytes(absolute, texture.EncodeToPNG());
    }
}
#endif
