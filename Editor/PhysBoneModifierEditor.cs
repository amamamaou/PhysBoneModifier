using UnityEditor;
using UnityEngine;

namespace Amama.PhysBoneModifier.Editor
{
    [CustomEditor(typeof(PhysBoneModifier))]
    public class PhysBoneModifierEditor : UnityEditor.Editor
    {
        private SerializedProperty _allowCollisionProperty;
        private SerializedProperty _allowGrabbingProperty;
        private SerializedProperty _allowPosingProperty;
        private SerializedProperty _excludeObjectsProperty;

        private void OnEnable()
        {
            _allowCollisionProperty = serializedObject.FindProperty(nameof(PhysBoneModifier.AllowCollision));
            _allowGrabbingProperty = serializedObject.FindProperty(nameof(PhysBoneModifier.AllowGrabbing));
            _allowPosingProperty = serializedObject.FindProperty(nameof(PhysBoneModifier.AllowPosing));
            _excludeObjectsProperty = serializedObject.FindProperty(nameof(PhysBoneModifier.ExcludeObjects));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("「継承／変更なし」は親の設定を引き継ぎます。親からの設定がない項目は、PhysBone の元の値を保持します。", MessageType.Info);

            DrawInteractionSettingGUI(_allowCollisionProperty, new GUIContent("Allow Collision"));
            DrawInteractionSettingGUI(_allowGrabbingProperty, new GUIContent("Allow Grabbing"));
            DrawInteractionSettingGUI(_allowPosingProperty, new GUIContent("Allow Posing"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_excludeObjectsProperty, new GUIContent("除外するオブジェクト (設定した要素と子要素が対象)"), true);

            serializedObject.ApplyModifiedProperties();
        }

        // InteractionSetting プロパティを表示する
        private static void DrawInteractionSettingGUI(SerializedProperty settingProperty, GUIContent label)
        {
            SerializedProperty typeProperty = settingProperty.FindPropertyRelative(nameof(InteractionSetting.Type));
            SerializedProperty selfProperty = settingProperty.FindPropertyRelative(nameof(InteractionSetting.AllowSelf));
            SerializedProperty othersProperty = settingProperty.FindPropertyRelative(nameof(InteractionSetting.AllowOthers));

            EditorGUILayout.PropertyField(typeProperty, label);

            // Other の場合に追加のチェックボックスを表示する
            if ((InteractionSetting.OverrideType)typeProperty.enumValueIndex == InteractionSetting.OverrideType.Other)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(selfProperty, new GUIContent("Allow Self"));
                EditorGUILayout.PropertyField(othersProperty, new GUIContent("Allow Others"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
