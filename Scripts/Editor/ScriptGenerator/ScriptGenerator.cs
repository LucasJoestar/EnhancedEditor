// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Editor class used to create and generate new scripts from templates in the project.
    /// </summary>
    [InitializeOnLoad]
    public static partial class ScriptGenerator
    {
        #region Global Members
        private const string ScriptGeneratorMenuContent1 = "// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //\n" +
                                                           "//\n" +
                                                           "// Notes:\n" +
                                                           "//\n" +
                                                           "// ============================================================================ //\n\n" +
                                                           "using UnityEditor;\n\n" +
                                                           "namespace EnhancedEditor.Editor\n{\n" +
                                                           "    public static partial class ScriptGenerator\n    {\n";

        private const string ScriptGeneratorMenuContent2 = "\n    }\n}\n";
        private const string ScriptTemplateMenuItem = "        [MenuItem(ScriptCreatorSubMenu + \"{0}\", false, MenuItemOrder)]\n" +
                                                      "        public static void Create{1}() => ScriptGeneratorWindow.GetWindow(\"{2}\");\n\n";

        private const string ScriptGeneratorName = "ScriptGenerator";
        private const string ScriptGeneratorMenuName = "ScriptGeneratorMenu";

        private const string ScriptCreatorSubMenu = "Assets/Create/Template C# Script/";
        private const int MenuItemOrder = 170;

        // -----------------------

        static ScriptGenerator()
        {
            // Template directory management.
            string _templateDirectory = $"{Application.dataPath}/{EnhancedEditorSettings.Settings.ScriptTemplateDirectory}";
            if (!Directory.Exists(_templateDirectory))
                Directory.CreateDirectory(_templateDirectory);
            
            // Get all templates.
            string[] _templates = Directory.GetFiles(_templateDirectory, "*.txt", SearchOption.AllDirectories);
            string _fileContent = string.Empty;
            
            for (int _i = 0; _i < _templates.Length; _i++)
            {
                // For each one of them, add a new item in the menu.
                string _template= _templates[_i].Replace('\\', '/');
                string _itemName = ObjectNames.NicifyVariableName(_template.Remove(0, _templateDirectory.Length + 1).Split('.')[0]);
                string _name = Path.GetFileNameWithoutExtension(_template);

                _template = _template.Remove(0, Application.dataPath.Length + 1);
                _fileContent += string.Format(ScriptTemplateMenuItem, _itemName, _name, _template);
            }

            _fileContent = $"{ScriptGeneratorMenuContent1}{_fileContent.TrimEnd()}{ScriptGeneratorMenuContent2}";

            // Get script full path.
            string[] _files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            string _path = string.Empty;

            foreach (string _file in _files)
            {
                string _name = Path.GetFileNameWithoutExtension(_file);
                if (_name == ScriptGeneratorMenuName)
                {
                    _path = _file;
                    break;
                }
                else if (_name == ScriptGeneratorName)
                {
                    _path = $"{Path.GetDirectoryName(_file)}/{ScriptGeneratorMenuName}.cs";
                }
            }
            
            // Write script content.
            if (!File.Exists(_path) || (File.ReadAllText(_path) != _fileContent))
            {
                File.WriteAllText(_path, _fileContent);
                AssetDatabase.Refresh();
            }
        }
        #endregion

        #region Script Generation
        private static void GenerateScript(string _template, string _name, bool _doOpen)
        {
            // Get new script full path.
            string _path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            _path = _path.Replace("Assets", Application.dataPath);

            _template = Path.Combine(Application.dataPath, _template);

            if (File.Exists(_path))
                _path = Path.GetDirectoryName(_path);

            string _templateName = Path.GetFileNameWithoutExtension(_template);
            string[] _parts = _templateName.Split('_');

            // Prefix.
            if ((_parts.Length > 1) && (_parts[0].ToUpper() == _parts[0]))
                _name = $"{_parts[0]}_{_name}";

            // Suffix.
            if ((_parts.Length > 1) && (_parts[_parts.Length - 1].ToUpper() == _parts[_parts.Length - 1]))
                _name = $"{_name}_{_parts[_parts.Length - 1]}";

            _path = Path.Combine(_path, $"{_name}.cs");
            if (File.Exists(_path))
            {
                EditorUtility.DisplayDialog("Script Generation Error",
                                            "Cannot create the desired script in this folder, for a script with the same name already exist in it.",
                                            "OK");
                
                return;
            }

            // Write script content.
            string _content = File.ReadAllText(_template).Replace("#SCRIPTNAME#", _name);

            File.WriteAllText(_path, _content);
            AssetDatabase.Refresh();

            if (_doOpen)
                Process.Start(_path);
        }
        #endregion

        #region Generator Window
        /// <summary>
        /// Utility window used to create a new script from a specific template.
        /// </summary>
        private class ScriptGeneratorWindow : EditorWindow
        {
            public static ScriptGeneratorWindow GetWindow(string _template)
            {
                ScriptGeneratorWindow _window = GetWindow<ScriptGeneratorWindow>(true, "Create New Script");

                _window.template = _template;
                _window.minSize = _window.maxSize
                                = new Vector2(300f, 60f);

                _window.ShowUtility();
                return _window;
            }

            // -------------------------------------------
            // Window GUI
            // -------------------------------------------

            protected const float CreateButtonWidth = 50f;
            protected const float CreateAndOpenButtonWidth = 95f;

            private const string UndoRecordTitle = "New Script Name Changes";
            protected const string EmptyNameMessage = "This file name cannot be null or empty!";

            private readonly GUIContent nameGUI = new GUIContent("Name:", "Name of the script to create.");
            private readonly GUIContent createGUI = new GUIContent("Create", "Create a new script with this name.");
            private readonly GUIContent createAndOpenGUI = new GUIContent("Create & Open", "Create a new script with this name and open it.");

            private string template = string.Empty;
            private string fileName = "NewScript";

            // -----------------------

            private void OnGUI()
            {
                Undo.RecordObject(this, UndoRecordTitle);

                // Header.
                Rect _position = new Rect(5f, 5f, 300f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(_position, nameGUI);

                // File name.
                _position.x += 50f;
                _position.width = position.width - _position.x - 5f;

                fileName = EditorGUI.TextField(_position, fileName);

                string _value = fileName.Trim();
                if (string.IsNullOrEmpty(_value))
                {
                    _position = new Rect()
                    {
                        x = 5f,
                        y = _position.y + _position.height + 5f,
                        width = position.width - 10f
                    };

                    _position.height = EnhancedEditorGUIUtility.GetHelpBoxHeight(EmptyNameMessage, UnityEditor.MessageType.Error, _position.width);
                    EditorGUI.HelpBox(_position, EmptyNameMessage, UnityEditor.MessageType.Error);
                }
                else
                {
                    // Create buttons.
                    _position = new Rect()
                    {
                        x = position.width - (CreateAndOpenButtonWidth + 5f),
                        y = _position.y + _position.height + 5f,
                        width = CreateAndOpenButtonWidth,
                        height = 25f
                    };

                    if (GUI.Button(_position, createAndOpenGUI))
                    {
                        GenerateScript(template, fileName, true);
                        Close();
                    }

                    _position.x -= CreateButtonWidth + 5f;
                    _position.width = CreateButtonWidth;

                    if (GUI.Button(_position, createGUI))
                    {
                        GenerateScript(template, fileName, false);
                        Close();
                    }
                }
            }
        }
        #endregion
    }
}
