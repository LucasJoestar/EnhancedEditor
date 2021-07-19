// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace EnhancedEditor.Editor
{
    public static class ScriptCreator
    {
        public const string ScriptCreatorSubMenu = "Assets/Create/";

        #region The Dreadful Show
        public const int TheDreadfulShowOrder = 181;

        private const string MonoBehaviour = "MonoBehaviour.txt";
        private const string ScriptableObject = "ScriptableObject.txt";
        private const string AnimationEvent = "AnimationEvent.txt";
        private const string StateMachineBehaviour = "StateMachineBehaviour.txt";

        // -----------------------

        [MenuItem(ScriptCreatorSubMenu + "C# Dreadful Script/MonoBehaviour", false, TheDreadfulShowOrder)]
        public static void CreateMonoBehaviour() => ScriptCreatorWindow.GetWindow(MonoBehaviour);

        [MenuItem(ScriptCreatorSubMenu + "C# Dreadful Script/ScriptableObject", false, TheDreadfulShowOrder)]
        public static void CreateScriptableObject() => ScriptCreatorWindow.GetWindow(ScriptableObject);

        [MenuItem(ScriptCreatorSubMenu + "C# Dreadful Script/AnimationEvent", false, TheDreadfulShowOrder)]
        public static void CreateAnimationEvent() => ScriptCreatorWindow.GetWindow(AnimationEvent);

        [MenuItem(ScriptCreatorSubMenu + "C# Dreadful Script/StateMachineBehaviour", false, TheDreadfulShowOrder)]
        public static void CreateStateMachineBehaviour() => ScriptCreatorWindow.GetWindow(StateMachineBehaviour, "ANI_", string.Empty);
        #endregion

        #region Enhanced Editor
        public const int EnhancedEditorOrder = 182;

        private const string EnhancedEditor = "EnhancedEditor/EnhancedEditor.txt";
        private const string EnhancedEditorAttribute = "EnhancedEditor/EnhancedEditorAttribute.txt";
        private const string EnhancedEditorPropertyDrawer = "EnhancedEditor/EnhancedEditorPropertyDrawer.txt";

        // -----------------------

        [MenuItem(ScriptCreatorSubMenu + "C# Enhanced Editor Script/EnhancedEditor", false, EnhancedEditorOrder)]
        public static void CreateEnhancedEditor() => ScriptCreatorWindow.GetWindow(EnhancedEditor);

        [MenuItem(ScriptCreatorSubMenu + "C# Enhanced Editor Script/EnhancedEditor Attribute", false, EnhancedEditorOrder)]
        public static void CreateEnhancedEditorAttribute() => ScriptCreatorWindow.GetWindow(EnhancedEditorAttribute, string.Empty, "Attribute");

        [MenuItem(ScriptCreatorSubMenu + "C# Enhanced Editor Script/EnhancedEditor PropertyDrawer", false, EnhancedEditorOrder)]
        public static void CreateEnhancedEditorPropertyDrawer() => ScriptCreatorWindow.GetWindow(EnhancedEditorPropertyDrawer, string.Empty, "PropertyDrawer");
        #endregion

        #region Creator
        private const string TemplatePath = "/EnhancedEditor/Scripts/Editor/ScriptCreator/Templates/";

        // -----------------------

        /// <summary>
        /// Creates a new script from a given template.
        /// </summary>
        /// <param name="_template">Template to use for script creation.</param>
        /// <param name="_name">Name of the script to create.</param>
        /// <param name="_doOpen">Should the newly created script be directly opened?</param>
        public static void CreateScript(string _template, string _name, bool _doOpen = false)
        {
            // Get script path and file.
            string _path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            _path = _path.Replace("Assets", Application.dataPath);

            if (File.Exists(_path))
                _path = Path.GetDirectoryName(_path);

            _path = $"{_path}/{_name}.cs";
            if (File.Exists(_path))
            {
                EditorUtility.DisplayDialog("Error", "A script with this name already exist.", "Ok");
                return;
            }

            // Write script content and replace its name.
            string[] _lines = File.ReadAllLines(Application.dataPath + TemplatePath + _template);
            string _file = string.Empty;
            for (int _i = 0; _i < _lines.Length; _i++)
            {
                _lines[_i] = _lines[_i].Replace("#SCRIPTNAME#", _name);
                _file += _lines[_i] + "\n";
            }

            File.WriteAllText(_path, _file);
            AssetDatabase.Refresh();

            // Open script if wanted.
            if (_doOpen)
                Process.Start(_path);
        }
        #endregion
    }

    public class ScriptCreatorWindow : EditorWindow
    {
        #region Window GUI
        /// <summary>
        /// Shows the ScriptCreator window, associated with a specified template.
        /// </summary>
        /// <param name="_template">Script template.</param>
        public static void GetWindow(string _template)
        {
            GetWindow(_template, string.Empty, string.Empty);
        }

        /// <summary>
        /// Shows the ScriptCreator window, associated with a specified template.
        /// </summary>
        /// <param name="_template">Script template.</param>
        public static void GetWindow(string _template, string _prefix, string _suffix)
        {
            ScriptCreatorWindow _launcher = GetWindow<ScriptCreatorWindow>(true, "Script Creator");
            _launcher.Show(_template, _prefix, _suffix);
        }

        // -------------------------------------------
        // Window GUI
        // -------------------------------------------

        private readonly GUIContent nameGUI = new GUIContent("Name:", "Name of this script.");
        private readonly GUIContent createGUI = new GUIContent("Create", "Create a new script with this name.");
        private readonly GUIContent createAndOpenGUI = new GUIContent("Create & Open", "Create a new script with this name and open it.");

        private string scriptName = "NewScript";
        private string template = string.Empty;

        private string prefix = string.Empty;
        private string suffix = string.Empty;

        // -----------------------

        private void OnGUI()
        {
            scriptName = EditorGUILayout.TextField(nameGUI, scriptName);

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(createGUI))
            {
                ScriptCreator.CreateScript(template, $"{prefix}{scriptName}{suffix}", false);
                Close();
            }
            /*else if (GUILayout.Button(createAndOpenGUI))
            {
                ScriptCreator.CreateScript(template, scriptName, true);
                Close();
            }*/
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Utility
        private void Show(string _template, string _prefix, string _suffix)
        {
            template = _template;
            prefix = _prefix;
            suffix = _suffix;

            minSize = maxSize
                    = new Vector2(250, 50);

            ShowUtility();
        }
        #endregion
    }
}
