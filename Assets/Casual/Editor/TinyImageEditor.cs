using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TinyImage), true)]
[CanEditMultipleObjects]
public class TinyImageEditor : ImageEditor
{
    SerializedProperty m_mirrorProperty;
    SerializedProperty m_Type;
    SerializedProperty m_Sprite;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_mirrorProperty = serializedObject.FindProperty("m_Mirror");
        m_Type = serializedObject.FindProperty("m_Type");
        m_Sprite = serializedObject.FindProperty("m_Sprite");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (m_Sprite.objectReferenceValue != null && m_Type.enumValueIndex == (int)Image.Type.Simple)
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_mirrorProperty, new GUIContent("Mirror Type"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
