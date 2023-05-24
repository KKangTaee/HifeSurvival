using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

//[CustomEditor(typeof(WorldTile))]
//public class WorldTileEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // 타겟 사용자 정의 타일 가져오기
//        WorldTile customTile = (WorldTile)target;

//        // 스프라이트 미리보기를 그리기 위한 레이아웃 생성
//        EditorGUILayout.BeginHorizontal();
//        GUILayout.Space(EditorGUIUtility.labelWidth);

//        if (customTile.sprite != null)
//        {
//            // 사용자 정의 타일의 스프라이트 미리보기를 그림
//            Rect spriteRect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
//            EditorGUI.DrawPreviewTexture(spriteRect, customTile.sprite.texture);
//        }

//        EditorGUILayout.EndHorizontal();

//        //if (customTile.sprite != null)
//        //{
//        //    // 사용자 정의 타일의 스프라이트 미리보기를 그림
//        //    GUILayout.BeginHorizontal();
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.Label(customTile.sprite.texture, GUILayout.Width(64), GUILayout.Height(64));
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.EndHorizontal();
//        //}

//        // 사용자 정의 타일의 스프라이트를 그림
//        //if (customTile.sprite != null)
//        //{
//        //    // 사용자 정의 타일의 스프라이트 미리보기를 그림
//        //    GUILayout.BeginHorizontal();
//        //    GUILayout.FlexibleSpace();
//        //    EditorGUILayout.ObjectField("", customTile.sprite, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.EndHorizontal();
//        //}

//        // 기본 Inspector 기능을 그대로 사용
//        base.OnInspectorGUI();
//    }
//}