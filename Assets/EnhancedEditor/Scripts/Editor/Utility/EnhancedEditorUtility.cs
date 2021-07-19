// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Multiple Editor utility methods related to the <see cref="EnhancedEditor"/> plugin.
    /// </summary>
	public static class EnhancedEditorUtility
    {
        #region Color Picker
        private static BindingFlags colorPickerFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static object[] colorPickerArgs = new object[] { null, null, null, true, false };

        // -----------------------

        /// <summary>
        /// Opens the built-in color picker window.
        /// </summary>
        /// <param name="_color">Color displayed in the color picker.</param>
        /// <param name="_callback">Callback when color is changed.</param>
        public static void ColorPicker(Color _color, Action<Color> _callback)
        {
            colorPickerArgs[1] = _callback;
            colorPickerArgs[2] = _color;

            EditorWindow _colorPicker = ScriptableObject.CreateInstance("ColorPicker") as EditorWindow;
            _colorPicker.GetType().InvokeMember("Show", colorPickerFlags, null, _colorPicker, colorPickerArgs);
        }
        #endregion

        #region Assets
        /// <summary>
        /// Find all assets in project of a given type.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>String array of all find assets GUID.</returns>
        public static string[] FindAssetsGUID<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        }

        /// <summary>
        /// Find all assets in project of a given type.
        /// </summary>
        /// <typeparam name="T">Asset type to find.</typeparam>
        /// <returns>String array of all find assets path.</returns>
        public static string[] FindAssets<T>() where T : Object
        {
            return Array.ConvertAll(AssetDatabase.FindAssets($"t:{typeof(T).Name}"), AssetDatabase.GUIDToAssetPath);
        }

        /// <summary>
        /// Loads all assets in project of a given type.
        /// </summary>
        /// <typeparam name="T">Asset type to load.</typeparam>
        /// <returns>Array of all loaded assets.</returns>
        public static T[] LoadAssets<T>() where T : Object
        {
            return Array.ConvertAll(AssetDatabase.FindAssets($"t:{typeof(T).Name}"), (a) =>
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(a));
            });
        }
        #endregion
    }
}
