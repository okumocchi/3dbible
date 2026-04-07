using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class TMPFontSizeMapper : EditorWindow
{
    private Vector2 scrollPos;
    private List<FontSizeMapping> mappings = new List<FontSizeMapping>();

    [MenuItem("Tools/TMP Font Size Mapper")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontSizeMapper>("TMP Font Size Mapper");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Font Size Mapping", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < mappings.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            mappings[i].original = EditorGUILayout.FloatField("Original", mappings[i].original);
            mappings[i].replacement = EditorGUILayout.FloatField("New", mappings[i].replacement);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                mappings.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Mapping"))
        {
            mappings.Add(new FontSizeMapping());
        }

        if (GUILayout.Button("Apply to All TextMeshProUGUI"))
        {
            ApplyFontSizeMappings();
        }
    }

    void ApplyFontSizeMappings()
    {
        //TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(false);
        TextMeshPro[] texts = FindObjectsOfType<TextMeshPro>(false);
        int changedCount = 0;
        Debug.Log("tmp count:" + texts.Length);


        foreach (var tmp in texts)
        {
            foreach (var map in mappings)
            {
        Debug.Log(tmp.fontSize + "," +  map.original);

                if (Mathf.Approximately(tmp.fontSize, map.original))
                {
                    Undo.RecordObject(tmp, "Change TMP Font Size");
                    tmp.fontSize = map.replacement;
                    EditorUtility.SetDirty(tmp);
                    changedCount++;
                    break;
                }
            }
        }

        Debug.Log($"Updated {changedCount} TextMeshProUGUI objects.");
    }

    [System.Serializable]
    public class FontSizeMapping
    {
        public float original;
        public float replacement;
    }
}
