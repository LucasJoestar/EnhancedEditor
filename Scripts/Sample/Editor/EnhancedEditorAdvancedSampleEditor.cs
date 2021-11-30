// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using EnhancedEditor.Editor;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Sample.Editor
{
    [CustomEditor(typeof(EnhancedEditorAdvancedSample)), CanEditMultipleObjects]
    #pragma warning disable IDE0044
    #pragma warning disable IDE0051
    #pragma warning disable IDE0052
    public class EnhancedEditorAdvancedSampleEditor : UnityObjectEditor
    {
        #region Editor GUI
        private readonly GUIContent[] tabs = new GUIContent[]
                                                    {
                                                        new GUIContent("Default"),
                                                        new GUIContent("GUI"),
                                                        new GUIContent("GUI Layout")
                                                    };

        private EnhancedEditorAdvancedSample sample = null;
        private static int selectedTab = 2;

        // -----------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            sample = target as EnhancedEditorAdvancedSample;
        }

        public override void OnInspectorGUI()
        {
            // Update object.
            serializedObject.Update();

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);
            GUILayout.Space(5f);

            // GUI.
            switch (selectedTab)
            {
                case 0:
                    base.OnInspectorGUI();
                    break;

                case 1:
                    DrawGUI();
                    break;

                case 2:
                    DrawGUILayout();
                    break;

                default:
                    break;
            }           

            // Apply changes.
            serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region GUI
        public bool Foldout = false;
        private float height = 0f;

        // -----------------------

        public void DrawGUI()
        {
            // Get properties.
            var _assetPreview = serializedObject.FindProperty("AssetPreview");
            var _readonly = serializedObject.FindProperty("Readonly");
            var _inline = serializedObject.FindProperty("Inline");
            var _picker = serializedObject.FindProperty("Picker");
            var _pickerAnimator = serializedObject.FindProperty("PickerAnimator");
            var _required = serializedObject.FindProperty("Required");
            var _color = serializedObject.FindProperty("Color");
            var _textArea = serializedObject.FindProperty("TextArea");

            var _tag = serializedObject.FindProperty("Tag");
            var _tagGroup = serializedObject.FindProperty("TagGroup");
            var _sceneAsset = serializedObject.FindProperty("SceneAsset");

            var _boolean = serializedObject.FindProperty("Boolean");
            var _folderPath = serializedObject.FindProperty("FolderPath");

            var _minFloat = serializedObject.FindProperty("MinFloat");
            var _maxInt = serializedObject.FindProperty("MaxInt");
            var _progressBar = serializedObject.FindProperty("ProgressBar");
            var _precisionSlider = serializedObject.FindProperty("PrecisionSlider");
            var _validationFloat = serializedObject.FindProperty("ValidationFloat");
            var _minMaxFloat = serializedObject.FindProperty("MinMaxFloat");
            var _minMaxInt = serializedObject.FindProperty("MinMaxInt");

            Rect _original = EditorGUILayout.GetControlRect();
            _original.height = EditorGUIUtility.singleLineHeight;

            Rect _position = new Rect(_original);
            Rect _temp;

            // GUI.
            EnhancedEditorGUI.UnderlinedLabel(_position, "Underlined Label");

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.AssetPreviewField(_position, _assetPreview, out float _extraHeight);
            _position.y += _extraHeight;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.FolderField(_position, _folderPath);

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.PickerField(_position, _picker, typeof(MeshRenderer));

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.ColorPaletteField(_position, _color, out _extraHeight);
            _position.y += _extraHeight;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.TextArea(_position, _textArea, out float _totalHeight);
            _position.y += _totalHeight + EditorGUIUtility.standardVerticalSpacing;

            EnhancedEditorGUI.RequiredField(_position, _required, out _extraHeight);
            _position.y += _extraHeight;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            _position.y += EnhancedEditorGUI.Texture(_position, sample.Required) + EditorGUIUtility.standardVerticalSpacing;

            EnhancedEditorGUI.PickerField(_position, _pickerAnimator, new System.Type[] { typeof(ScriptableObject), typeof(EnhancedBehaviour) } );

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.ReadonlyField(_position, _readonly, true, true);
            _position.y += EditorGUI.GetPropertyHeight(_readonly, true) - _position.height;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.InlineField(_position, _inline, out _extraHeight);
            _position.y += _extraHeight;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.SceneAssetField(_position, new GUIContent("Scene Asset"), sample.SceneAsset);

            // Multi-Tags.
            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.Section(_position, new GUIContent("Multi-Tags"));

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.TagField(_position, _tag);

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.TagGroupField(_position, _tagGroup, out _extraHeight);
            _position.y += _extraHeight;

            // Section.
            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.Section(_position, new GUIContent("Min / Max"));

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.MinField(_position, _minFloat, "MaxInt");

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.MaxField(_position, _maxInt, "MinFloat");

            _temp = new Rect(_position.x, _position.yMax + EditorGUIUtility.standardVerticalSpacing, _position.width, 25f);
            EnhancedEditorGUI.ProgressBar(_temp, _progressBar, 250, SuperColor.Crimson.Get(), true);
            _position.y += _temp.height + EditorGUIUtility.standardVerticalSpacing;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.PrecisionSliderField(_position, _precisionSlider, 7f, 8f, .1f, out _extraHeight);
            _position.y += _extraHeight;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.ValidationMemberField(_position, _validationFloat, "ValidationFloatProperty");

            // Separator.
            _temp = new Rect(_position.x, _position.yMax + EditorGUIUtility.standardVerticalSpacing + 2f, _position.width, 2f);
            EnhancedEditorGUI.HorizontalLine(_temp, SuperColor.Chocolate.Get(), 100f);
            _position.y += _temp.height + EditorGUIUtility.standardVerticalSpacing + 4f;

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.MinMaxField(_position, _minMaxFloat, 0f, 100f);

            _position.y += _position.height + EditorGUIUtility.standardVerticalSpacing;
            EnhancedEditorGUI.MinMaxField(_position, _minMaxInt, "MinMaxFloat");

            // Let's expand view.
            height = _position.yMax - _original.yMax;
            EditorGUILayout.GetControlRect(true, height);
        }

        public void DrawGUILayout()
        {
            // Get properties.
            var _assetPreview = serializedObject.FindProperty("AssetPreview");
            var _readonly = serializedObject.FindProperty("Readonly");
            var _inline = serializedObject.FindProperty("Inline");
            var _picker = serializedObject.FindProperty("Picker");
            var _pickerAnimator = serializedObject.FindProperty("PickerAnimator");
            var _required = serializedObject.FindProperty("Required");
            var _color = serializedObject.FindProperty("Color");
            var _textArea = serializedObject.FindProperty("TextArea");

            var _tag = serializedObject.FindProperty("Tag");
            var _tagGroup = serializedObject.FindProperty("TagGroup");
            var _sceneAsset = serializedObject.FindProperty("SceneAsset");

            var _boolean = serializedObject.FindProperty("Boolean");
            var _folderPath = serializedObject.FindProperty("FolderPath");

            var _minFloat = serializedObject.FindProperty("MinFloat");
            var _maxInt = serializedObject.FindProperty("MaxInt");
            var _progressBar = serializedObject.FindProperty("ProgressBar");
            var _precisionSlider = serializedObject.FindProperty("PrecisionSlider");
            var _validationFloat = serializedObject.FindProperty("ValidationFloat");
            var _minMaxFloat = serializedObject.FindProperty("MinMaxFloat");
            var _minMaxInt = serializedObject.FindProperty("MinMaxInt");

            EditorGUI.indentLevel += 2;

            EnhancedEditorGUILayout.UnderlinedLabel(new GUIContent("Underlined"));
            EnhancedEditorGUILayout.FolderField(_folderPath);

            EnhancedEditorGUILayout.AssetPreviewField(_assetPreview);
            EnhancedEditorGUILayout.PickerField(_picker, typeof(MeshRenderer));

            EnhancedEditorGUILayout.ColorPaletteField(_color);
            EnhancedEditorGUILayout.TextArea(_textArea);

            EnhancedEditorGUILayout.RequiredField(_required);
            EnhancedEditorGUILayout.Texture(sample.Required);

            EnhancedEditorGUILayout.PickerField(_pickerAnimator, new System.Type[] { typeof(ScriptableObject), typeof(EnhancedBehaviour) });
            EnhancedEditorGUILayout.ReadonlyField(_readonly, true);
            EnhancedEditorGUILayout.InlineField(_inline);

            EnhancedEditorGUILayout.SceneAssetField(new GUIContent("Scene Asset"), sample.SceneAsset);

            EnhancedEditorGUILayout.Section("Section");

            EnhancedEditorGUILayout.TagField(_tag);
            EnhancedEditorGUILayout.TagGroupField(_tagGroup);

            EnhancedEditorGUILayout.HorizontalLine(SuperColor.Crimson.Get());

            EnhancedEditorGUILayout.MaxField(_maxInt, "MinFloat");
            EnhancedEditorGUILayout.MinField(_minFloat, "MaxInt");

            EnhancedEditorGUILayout.ProgressBar(_progressBar, "ProgressBarMax", SuperColor.Pumpkin.Get(), true, GUILayout.Height(25f));

            EnhancedEditorGUILayout.MinMaxField(_minMaxInt, "MinMaxFloat");
            EnhancedEditorGUILayout.MinMaxField(_minMaxFloat, 0f, 100f);

            EnhancedEditorGUILayout.PrecisionSliderField(_precisionSlider, 0f, 100f, .5f);
            EnhancedEditorGUILayout.ValidationMemberField(_validationFloat, "ValidationFloatProperty");

            EditorGUI.indentLevel -= 2;

            EnhancedEditorGUILayout.ValidationMemberField(_validationFloat, "ValidationFloatMethod");
        }
        #endregion
    }
}
