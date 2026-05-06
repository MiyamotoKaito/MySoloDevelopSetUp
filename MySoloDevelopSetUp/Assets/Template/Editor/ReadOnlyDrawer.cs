using System.ComponentModel;
using UnityEditor;
using UnityEngine;
namespace Template.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false; // ここで編集不可にする
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;  // 戻す
        }
    }
}