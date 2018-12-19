using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameConfigs))]
public class GameConfigsInspector : Editor
{
    public static string[] baseUrls =
    {
        "https://www.baidu.com",
        "https://www.google.com",
        "http://"
    };
    public static string[] baseNames =
    {
       "○ 内网测试机",
       "● 外网正式机",
       "○ 自定义"
    };

    private static int[] netTypeValues = new int[0];
    private static int urlIndex;

    SerializedProperty publishModeProperty;
    SerializedProperty webURLProperty;

    protected void OnEnable()
    {
        publishModeProperty = serializedObject.FindProperty("publishMode");
        webURLProperty = serializedObject.FindProperty("webURL");

        int length = baseUrls.Length;
        if (netTypeValues.Length != length)
        {
            netTypeValues = new int[length];
            for (int index = 0; index < length; ++index)
            {
                netTypeValues[index] = index;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Bundle拓展名", GameConfigs.ExtName);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(publishModeProperty, new GUIContent("发布模式"));

        int length = baseUrls.Length;
        urlIndex = length - 1;
        for (int index = 0; index < length; ++index)
        {
            if (string.Equals(webURLProperty.stringValue, baseUrls[index]) || string.Equals(webURLProperty.stringValue + "/", baseUrls[index]))
            {
                urlIndex = index;
            }
        }

        urlIndex = EditorGUILayout.IntPopup("服务器选择", urlIndex, baseNames, netTypeValues);
        if (GUI.changed)
            webURLProperty.stringValue = baseUrls[urlIndex];
        EditorGUILayout.PropertyField(webURLProperty, new GUIContent("服务器配置"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
