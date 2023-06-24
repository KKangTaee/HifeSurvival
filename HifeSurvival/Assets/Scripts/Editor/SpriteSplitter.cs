using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSplitter : MonoBehaviour
{
    [MenuItem("Assets/Split Sprite")]
    static void SplitSprite()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture != null && texture.isReadable)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

            string folderPath = "Assets/" + texture.name + "_pack";
            Directory.CreateDirectory(folderPath);

            int count = 0;
            foreach (Object obj in objects)
            {
                Sprite sprite = obj as Sprite;
                if (sprite != null)
                {
                    SaveSpriteAsPNG(sprite, folderPath + "/" + sprite.name + ".png");
                    count++;
                }
            }

            Debug.Log("Split " + count + " sprites from " + texture.name + " and saved as PNGs.");
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("No texture or selected texture is not readable.");
        }
    }

    [MenuItem("Assets/Split Sprite", true)]
    static bool ValidateSplitSprite()
    {
        return Selection.activeObject is Texture2D;
    }

    static void SaveSpriteAsPNG(Sprite sprite, string filename)
    {
        Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.ARGB32, false);
        tex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.xMin, (int)sprite.rect.yMin, (int)sprite.rect.width, (int)sprite.rect.height));
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(filename, pngData);
    }
}