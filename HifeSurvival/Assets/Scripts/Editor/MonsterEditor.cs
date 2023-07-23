using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Monster))]
public class MonsterEditor : Editor 
{
    private Monster monsterObject;

    private void OnEnable()
    {
        monsterObject =(Monster)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("[Monster Editor]");
        
        GUI.enabled = false;
        
        var splitted = monsterObject.name.Split('_');
        if (splitted.Length > 1)
        {
            int fxNumber = int.TryParse(splitted[1], out var parsedNumber) ? parsedNumber : 0;
            monsterObject.SetMonsterName((EMonster)fxNumber);
        }

        EditorGUILayout.EnumPopup("Monster Name",  monsterObject.MonsterName);
        GUI.enabled = true;
    }
}