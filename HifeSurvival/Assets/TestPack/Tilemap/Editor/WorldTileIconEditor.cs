//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Tilemaps;

////[InitializeOnLoad]
//public class WorldTileIconEditor
//{
//    static WorldTileIconEditor()
//    {
//        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
//    }

//    ///
//    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
//    {
//        //Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));

//        //// CustomTile ���¿� ���� ���� ������ ����
//        //if (asset != null && asset.GetType() == typeof(WorldTile))
//        //{
//        //    // ���ϴ� ������ �̹����� �ε��մϴ�.
//        //    Texture2D customIcon = EditorGUIUtility.Load("YourCustomIconPath.png") as Texture2D;

//        //    if(asset is WorldTile tile)
//        //    {
//        //        Rect iconRect = new Rect(selectionRect.x, selectionRect.y, 16, 16);
//        //        GUI.DrawTexture(iconRect, tile.sprite.texture);
//        //    }


//        //    //if (customIcon != null)
//        //    //{
//        //    //    Rect iconRect = new Rect(selectionRect.x, selectionRect.y, 16, 16);
//        //    //    GUI.DrawTexture(iconRect, customIcon);
//        //    //}
//        //}
//    }
//}