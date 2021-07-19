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
    [Serializable]
	public class EnhancedEditorPreferences
    {
        private const string PreferencesPath = "Preferences/EnhancedEditor";
        private const string PreferencesKey = "EnhancedEditorPreferences";

        /// <summary>
        /// 
        /// </summary>
        public static EnhancedEditorPreferences Preferences
        {
            get
            {
                if (!isLoaded)
                {
                    // Loads saved datas from EditorPrefs.
                    string _data = EditorPrefs.GetString(PreferencesKey, string.Empty);
                    if (!string.IsNullOrEmpty(_data))
                        JsonUtility.FromJsonOverwrite(_data, preferences);

                    isLoaded = true;
                }

                return preferences;
            }
            internal set
            {
                preferences = value;

                string _data = JsonUtility.ToJson(value);
                EditorPrefs.SetString(PreferencesKey, _data);
            }
        }

        private static EnhancedEditorPreferences preferences = new EnhancedEditorPreferences();
        private static bool isLoaded = false;

        public string VarString = "Message";
        public float VarFloat = 7.5f;

        #region Preferences
        /// <summary>
        /// Opens the preferences window to edit user <see cref="EnhancedEditor"/> settings.
        /// </summary>
        [MenuItem("Enhanced Editor/Preferences", false, -50)]
        public static void OpenPreferencesSettings()
        {
            SettingsService.OpenUserPreferences(PreferencesPath);
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                label = "Enhanced Editor",

                guiHandler = (string _searchContext) =>
                {
                    string _data = EditorPrefs.GetString(PreferencesKey, JsonUtility.ToJson(new EnhancedEditorPreferences()));
                    EnhancedEditorPreferences _settings = JsonUtility.FromJson<EnhancedEditorPreferences>(_data);

                    EditorGUI.BeginChangeCheck();

                    _settings.VarString = GUILayout.TextField(_settings.VarString);
                    _settings.VarFloat = EditorGUILayout.FloatField(_settings.VarFloat);

                    if (EditorGUI.EndChangeCheck())
                    {
                        //Debug.LogError("Save");

                        _data = JsonUtility.ToJson(_settings);
                        EditorPrefs.SetString(PreferencesKey, _data);
                    }
                },

                keywords = new string[] { "Enhanced", "Editor", "Build Pipeline", "Autosave" },
            };
        }

        [EditorToolbarRightExtension(Order = 500)]
        public static void OpenPreferences()
        {
            GUILayout.FlexibleSpace();

            if (EnhancedEditorToolbar.Button(EditorGUIUtility.IconContent("d_Settings"), GUILayout.Width(32f)))
            {
                OpenPreferencesSettings();
            }

            GUILayout.Space(25f);
        }
        #endregion
    }
}
