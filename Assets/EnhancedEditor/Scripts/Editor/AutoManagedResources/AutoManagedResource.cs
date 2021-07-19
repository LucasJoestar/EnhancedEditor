// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Auto-managed cached resource assets,
    /// dynamically finding and loading themselves from the editor database.
    /// <para/>
    /// If multiple resources of the same type exist, only the ones loaded on first resource call will be returned.
    /// Please use <see cref="Reload"/> to force resources update.
    /// </summary>
    /// <typeparam name="T">Resource type.</typeparam>
	public class AutoManagedResource<T> where T : ScriptableObject
    {
        #region Global Members
        public const string DefaultResourcesDirectory = "Assets/EnhancedEditor/AutoManagedResources";

        public readonly string DefaultDirectory = DefaultResourcesDirectory;
        public readonly string DefaultName = string.Empty;

        public readonly string Prefix = string.Empty;
        public readonly string Suffix = string.Empty;
        public readonly bool IsEditorOnly = true;

        private T[] resources = new T[] { };
        #endregion

        #region Constructors
        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(string _defaultName, bool _isEditorOnly = true)
        {
            DefaultName = _defaultName;
            IsEditorOnly = _isEditorOnly;

            // Get default directory path.
            if (IsEditorOnly)
                DefaultDirectory = Path.Combine(DefaultDirectory, "Editor");

            DefaultDirectory = Path.Combine(DefaultDirectory, typeof(T).ToString());
        }

        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(bool _isEditorOnly = true) : this (typeof(T).ToString(), _isEditorOnly)
        {
        }

        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(string _prefix, string _suffix, bool _isEditorOnly = true) : this(_prefix, _suffix, typeof(T).ToString(), _isEditorOnly)
        {
        }

        /// <inheritdoc cref="AutoManagedResource{T}"/>
        /// <param name="_defaultName">Default name of resource assets to create.</param>
        /// <param name="_prefix">Prefix of all resource file names.</param>
        /// <param name="_suffix">Suffix of all resource file names.</param>
        /// <param name="_isEditorOnly">Is this resource editor only or should it be included in builds?</param>
        public AutoManagedResource(string _defaultName, string _prefix, string _suffix, bool _isEditorOnly = true) : this(_defaultName, _isEditorOnly)
        {
            Prefix = _prefix;
            Suffix = _suffix;
        }
        #endregion

        #region Resources
        /// <summary>
        /// Get first loaded resource. If no matching asset is found in the database, one is automatically created.
        /// </summary>
        /// <returns>First loaded resource asset.</returns>
        public T GetResource()
        {
            if (resources.Length == 0)
                Reload();

            return resources[0];
        }

        /// <summary>
        /// Get all loaded resources. Use <see cref="Reload"/> to refresh resources.
        /// </summary>
        /// <returns>All loaded resource assets.</returns>
        public T[] GetResources()
        {
            if (resources.Length == 0)
                Reload();

            return resources;
        }

        /// <summary>
        /// Reloads cached resources.
        /// Use this when creating / destroying assets in the project.
        /// </summary>
        /// <returns>(Re)loaded resources.</returns>
        public T[] Reload()
        {
            resources = EnhancedEditorUtility.LoadAssets<T>();
            if (resources.Length == 0)
            {
                string _name = DefaultName;
                CreateResource(_name);
            }

            return resources;
        }

        /// <inheritdoc cref="CreateResource(string, T)"/>
        public T CreateResource(string _name)
        {
            T _resource = CreateResource(_name, ScriptableObject.CreateInstance<T>());
            return _resource;
        }

        /// <summary>
        /// Creates a new resource and save it in database.
        /// </summary>
        /// <param name="_name">Name of the resource asset file to create.</param>
        /// <param name="_name">Resource asset to write on disk.</param>
        /// <returns>Created resource asset.</returns>
        public T CreateResource(string _name, T _resource)
        {
            _name = $"{Prefix}{_name}{Suffix}";

            string _directory = Application.dataPath + DefaultDirectory.Remove(0, 6);
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            int _amount = 0;
            string _fileName = _name;
            while (File.Exists(Path.Combine(_directory, $"{_fileName}.asset")))
            {
                _amount++;
                _fileName = $"{_name}_{_amount}.asset";
            }

            string _path = Path.Combine(DefaultDirectory, $"{_fileName}.asset");

            AssetDatabase.CreateAsset(_resource, _path);
            AssetDatabase.SaveAssets();

            ArrayUtility.Add(ref resources, _resource);
            return _resource;
        }
        #endregion
    }
}
