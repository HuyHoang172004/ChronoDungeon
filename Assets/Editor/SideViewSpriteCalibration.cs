#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Offline sprite import calibration. Gameplay colliders never follow changing cape/weapon bounds.
public static class SideViewSpriteCalibration
{
    public static Sprite[] ImportSheet(string source, string output, int columns, int rows, int topRow, float height)
    {
        Directory.CreateDirectory(output);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(File.ReadAllBytes(source));
        for (int n = 0; n < columns; n++)
        {
            int x = Mathf.RoundToInt(n * texture.width / (float)columns);
            int x1 = Mathf.RoundToInt((n + 1) * texture.width / (float)columns);
            int y = Mathf.RoundToInt((rows - topRow - 1) * texture.height / (float)rows);
            int y1 = Mathf.RoundToInt((rows - topRow) * texture.height / (float)rows);
            var frame = new Texture2D(x1-x, y1-y, TextureFormat.RGBA32, false);
            frame.SetPixels(texture.GetPixels(x,y,x1-x,y1-y)); frame.Apply();
            File.WriteAllBytes($"{output}/Frame_{n:00}.png", frame.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(frame);
        }
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.Refresh();
        return Calibrate(Directory.GetFiles(output, "Frame_*.png").OrderBy(p=>p).ToArray(), height);
    }

    public static Sprite[] Calibrate(string[] paths, float height)
    {
        // First frame in each source state is the standing/reference pose. One PPU for
        // the whole clip preserves crouching and collapsing instead of inflating death frames.
        float ppu = 100;
        for (int n=0;n<paths.Length;n++)
        {
            string path=paths[n].Replace('\\','/');
            var t=new Texture2D(2,2,TextureFormat.RGBA32,false);
            t.LoadImage(File.ReadAllBytes(path));
            var pixels=t.GetPixels32(); int w=t.width,h=t.height;
            int left=w,right=-1,bottom=h,top=-1;
            // Ignore sparse detached particles/neighbor fragments when locating the body.
            for(int y=0;y<h;y++) {
                int count=0; for(int x=0;x<w;x++) if(pixels[y*w+x].a>=128) count++;
                if(count<Mathf.Max(4,w/40)) continue;
                bottom=Mathf.Min(bottom,y); top=Mathf.Max(top,y);
                for(int x=0;x<w;x++) if(pixels[y*w+x].a>=128){left=Mathf.Min(left,x);right=Mathf.Max(right,x);}
            }
            if(top<bottom) throw new InvalidOperationException("Empty sprite: "+path);
            if(n==0) ppu=(top-bottom+1)/height;
            // The center of the lower leg band is a body anchor, unlike the center
            // of the full rectangle (which shifts when a weapon/cape extends).
            double sum=0; int samples=0;
            int bandTop=bottom+Mathf.Max(2,(top-bottom)/8);
            for(int y=bottom;y<=bandTop;y++) for(int x=left;x<=right;x++)
                if(pixels[y*w+x].a>=128){sum+=x+.5;samples++;}
            float anchor=samples>0?(float)(sum/samples):(left+right+1)*.5f;
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=ppu;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
            importer.maxTextureSize=4096; importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Point;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteAlignment=(int)SpriteAlignment.Custom;
            settings.spritePivot=new Vector2(anchor/w,bottom/(float)h);
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            UnityEngine.Object.DestroyImmediate(t);
        }
        return paths.Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\','/'))).ToArray();
    }

    public static void FitBody(GameObject actor,float height,float width)
    {
        var box=actor.GetComponent<BoxCollider2D>();
        if(box!=null){box.size=new Vector2(width,height);box.offset=new Vector2(0,height*.5f);}
        var capsule=actor.GetComponent<CapsuleCollider2D>();
        if(capsule!=null){capsule.size=new Vector2(width,height);capsule.offset=new Vector2(0,height*.5f);}
        var rb=actor.GetComponent<Rigidbody2D>();
        if(rb!=null){rb.constraints=RigidbodyConstraints2D.FreezeRotation;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;}
    }
}
#endif
