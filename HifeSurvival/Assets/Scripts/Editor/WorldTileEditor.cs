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
//        // Ÿ�� ����� ���� Ÿ�� ��������
//        WorldTile customTile = (WorldTile)target;

//        // ��������Ʈ �̸����⸦ �׸��� ���� ���̾ƿ� ����
//        EditorGUILayout.BeginHorizontal();
//        GUILayout.Space(EditorGUIUtility.labelWidth);

//        if (customTile.sprite != null)
//        {
//            // ����� ���� Ÿ���� ��������Ʈ �̸����⸦ �׸�
//            Rect spriteRect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
//            EditorGUI.DrawPreviewTexture(spriteRect, customTile.sprite.texture);
//        }

//        EditorGUILayout.EndHorizontal();

//        //if (customTile.sprite != null)
//        //{
//        //    // ����� ���� Ÿ���� ��������Ʈ �̸����⸦ �׸�
//        //    GUILayout.BeginHorizontal();
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.Label(customTile.sprite.texture, GUILayout.Width(64), GUILayout.Height(64));
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.EndHorizontal();
//        //}

//        // ����� ���� Ÿ���� ��������Ʈ�� �׸�
//        //if (customTile.sprite != null)
//        //{
//        //    // ����� ���� Ÿ���� ��������Ʈ �̸����⸦ �׸�
//        //    GUILayout.BeginHorizontal();
//        //    GUILayout.FlexibleSpace();
//        //    EditorGUILayout.ObjectField("", customTile.sprite, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));
//        //    GUILayout.FlexibleSpace();
//        //    GUILayout.EndHorizontal();
//        //}

//        // �⺻ Inspector ����� �״�� ���
//        base.OnInspectorGUI();
//    }
//}