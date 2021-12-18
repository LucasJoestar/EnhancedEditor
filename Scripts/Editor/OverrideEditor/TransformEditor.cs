// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Custom <see cref="Transform"/> editor, adding multiple shortcuts and utilities in the inspector.
    /// </summary>
    [CustomEditor(typeof(Transform), true), CanEditMultipleObjects]
    public class TransformEditor : UnityObjectEditor
    {
        #region Editor Content
        private const float Spacing = 5f;
        private const float ResetButtonWidth = 20f;
        private const float SpaceModeButtonWidth = 102f;

        private const string IsLocalSpaceKey = "TransformIsLocalSpace";

        private static readonly Type transformInspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TransformInspector");

        private static readonly GUIContent localModeGUI = new GUIContent(" Local Space", "Copy / Paste operations will be performed related to the object local space.");
        private static readonly GUIContent worldModeGUI = new GUIContent(" World Space", "Copy / Paste operations will be performed related to the object world space.");

        private static readonly GUIContent resetPositionGUI = new GUIContent(string.Empty, "Reset the position of the selected object(s).");
        private static readonly GUIContent resetRotationGUI = new GUIContent(string.Empty, "Reset the rotation of the selected object(s).");
        private static readonly GUIContent resetScaleGUI = new GUIContent(string.Empty, "Reset the scale of the selected object(s).");

        private static readonly GUIContent copyPositionGUI = new GUIContent("Copy Position", "Copy the local position of the selected object(s).");
        private static readonly GUIContent copyRotationGUI = new GUIContent("Copy Rotation", "Copy the local rotation of the selected object(s).");
        private static readonly GUIContent copyScaleGUI = new GUIContent("Copy Scale", "Copy the local scale of the selected object(s).");

        private static readonly GUIContent pastePositionGUI = new GUIContent("Paste Position", "Overwrite the local position of the selected object(s) with the one in buffer.");
        private static readonly GUIContent pasteRotationGUI = new GUIContent("Paste Rotation", "Overwrite the local rotation of the selected object(s) with the one in buffer.");
        private static readonly GUIContent pasteScaleGUI = new GUIContent("Paste Scale", "Overwrite the local scale of the selected object(s) with the one in buffer.");

        private static bool isLocalSpace = true;

        /// <summary>
        /// Current position value in buffer (used to copy / paste).
        /// </summary>
        public static Vector3 PositionBuffer = Vector3.zero;

        /// <summary>
        /// Current rotation value in buffer (used to copy / paste).
        /// </summary>
        public static Quaternion RotationBuffer = Quaternion.identity;

        /// <summary>
        /// Current scale value in buffer (used to copy / paste).
        /// </summary>
        public static Vector3 ScaleBuffer = Vector3.one;

        private UnityEditor.Editor transformEditor = null;
        private SerializedProperty localPosition = null;
        private SerializedProperty localRotation = null;
        private SerializedProperty localScale = null;

        // -----------------------

        protected override void OnEnable()
        {
            base.OnEnable();

            // Initialization.
            isLocalSpace = EditorPrefs.GetBool(IsLocalSpaceKey, isLocalSpace);

            transformEditor = CreateEditor(serializedObject.targetObjects, transformInspectorType);
            localPosition = serializedObject.FindProperty("m_LocalPosition");
            localRotation = serializedObject.FindProperty("m_LocalRotation");
            localScale = serializedObject.FindProperty("m_LocalScale");

            localModeGUI.image = EditorGUIUtility.FindTexture("ToolHandleLocal");
            worldModeGUI.image = EditorGUIUtility.FindTexture("ToolHandleGlobal");

            Texture2D _resetIcon = EditorGUIUtility.FindTexture("Refresh");
            resetPositionGUI.image = _resetIcon;
            resetRotationGUI.image = _resetIcon;
            resetScaleGUI.image = _resetIcon;
        }

        public override void OnInspectorGUI()
        {
            Rect _position = EditorGUILayout.GetControlRect(false, 0f);
            float _width = EnhancedEditorGUI.ManageDynamicControlHeight(GUIContent.none, _position.width - ResetButtonWidth - Spacing);

            // Avoid jitters and first layout event draw.
            if (_width < 0f)
                _width = EnhancedEditorGUIUtility.ScreenWidth - 20f - ResetButtonWidth - Spacing;

            // Draw the original inspector.
            using (var _scope = new EditorGUILayout.VerticalScope(GUILayout.Width(_width)))
            {
                transformEditor.OnInspectorGUI();
            }

            // Reset buttons.
            _position = new Rect()
            {
                x = _position.x + _width + Spacing,
                y = _position.y + EditorGUIUtility.standardVerticalSpacing - 1f,
                width = ResetButtonWidth,
                height = EditorGUIUtility.singleLineHeight + 1f
            };

            if (EnhancedEditorGUI.IconButton(_position, resetPositionGUI))
                ResetPosition();

            _position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + .2f;
            if (EnhancedEditorGUI.IconButton(_position, resetRotationGUI))
                ResetRotation();

            _position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + .2f;
            if (EnhancedEditorGUI.IconButton(_position, resetScaleGUI))
                ResetScale();

            // Local / World space mode.
            GUIContent _modeGUI = isLocalSpace
                                ? localModeGUI
                                : worldModeGUI;

            _position = EditorGUILayout.GetControlRect(GUILayout.Width(SpaceModeButtonWidth), GUILayout.Height(20f));
            if (EnhancedEditorGUI.IconButton(_position, _modeGUI))
            {
                isLocalSpace = !isLocalSpace;
                EditorPrefs.SetBool(IsLocalSpaceKey, isLocalSpace);
            }

            // Copy buttons.
            GUILayout.Space(5f);
            using (var _scope = new EditorGUILayout.HorizontalScope())
            {
                using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(!localPosition.hasMultipleDifferentValues))
                {
                    if (GUILayout.Button(copyPositionGUI))
                        CopyPosition();
                }

                using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(!localRotation.hasMultipleDifferentValues))
                {
                    if (GUILayout.Button(copyRotationGUI))
                        CopyRotation();
                }

                using (var _enabledScope = EnhancedGUI.GUIEnabled.Scope(!localScale.hasMultipleDifferentValues))
                {
                    if (GUILayout.Button(copyScaleGUI))
                        CopyScale();
                }
            }

            // Paste buttons.
            using (var _scope = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(pastePositionGUI))
                    PastePosition();

                if (GUILayout.Button(pasteRotationGUI))
                    PasteRotation();

                if (GUILayout.Button(pasteScaleGUI))
                    PasteScale();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Avoid memory leak.
            DestroyImmediate(transformEditor);
        }
        #endregion

        #region Reset
        private void ResetPosition()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Reset {serializedObject.targetObject.name} Position");
            foreach (Transform _transform in serializedObject.targetObjects)
            {
                _transform.localPosition = Vector3.zero;
            }
        }

        private void ResetRotation()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Reset {serializedObject.targetObject.name} Rotation");
            foreach (Transform _transform in serializedObject.targetObjects)
            {
                _transform.localRotation = Quaternion.identity;
            }
        }

        private void ResetScale()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Reset {serializedObject.targetObject.name} Scale");
            foreach (Transform _transform in serializedObject.targetObjects)
            {
                _transform.localScale = Vector3.one;
            }
        }
        #endregion

        #region Copy
        private void CopyPosition()
        {
            Transform _transform = serializedObject.targetObjects[0] as Transform;
            PositionBuffer = isLocalSpace
                           ? _transform.localPosition
                           : _transform.position;
        }

        private void CopyRotation()
        {
            Transform _transform = serializedObject.targetObjects[0] as Transform;
            RotationBuffer = isLocalSpace
                           ? _transform.localRotation
                           : _transform.rotation;
        }

        private void CopyScale()
        {
            Transform _transform = serializedObject.targetObjects[0] as Transform;
            ScaleBuffer = isLocalSpace
                        ? _transform.localScale
                        : _transform.lossyScale;
        }
        #endregion

        #region Paste
        private void PastePosition()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Paste {serializedObject.targetObject.name} Position");
            if (isLocalSpace)
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                    _transform.localPosition = PositionBuffer;
            }
            else
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                    _transform.position = PositionBuffer;
            }
        }

        private void PasteRotation()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Paste {serializedObject.targetObject.name} Rotation");
            if (isLocalSpace)
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                    _transform.localRotation = RotationBuffer;
            }
            else
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                    _transform.rotation = RotationBuffer;
            }
        }

        private void PasteScale()
        {
            Undo.RecordObjects(serializedObject.targetObjects, $"Paste {serializedObject.targetObject.name} Scale");
            if (isLocalSpace)
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                    _transform.localScale = ScaleBuffer;
            }
            else
            {
                foreach (Transform _transform in serializedObject.targetObjects)
                {
                    Vector3 _lossy = _transform.lossyScale;
                    Vector3 _local = _transform.localScale;

                    Vector3 _localZero = new Vector3(_lossy.x / _local.x, _lossy.y / _local.y, _lossy.z / _local.z);
                    _transform.localScale = new Vector3(ScaleBuffer.x / _localZero.x, ScaleBuffer.y / _localZero.y, ScaleBuffer.z / _localZero.z);
                }
            }
        }
        #endregion
    }
}
