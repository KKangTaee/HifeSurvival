using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[CustomEditor(typeof(FXBase))]
public class FXBaseEditor : Editor
{
    private FXBase fxObject;

    private void OnEnable()
    {
        fxObject = (FXBase)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("[FXBase Editor]");
        
        GUI.enabled = false;
        
        var splitted = fxObject.name.Split('_');
        if (splitted.Length > 1)
        {
            int fxNumber = int.TryParse(splitted[1], out var parsedNumber) ? parsedNumber : (int) fxObject.FX_ID;
            fxObject.SetId((EFX_ID)fxNumber);
        }

        EditorGUILayout.EnumPopup("ID",  fxObject.FX_ID);
        GUI.enabled = true;
    }
}