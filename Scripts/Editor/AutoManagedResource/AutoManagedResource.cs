// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Auto-managed cached <see cref="ScriptableObject"/> resource assets,
    /// dynamically finding and loading themselves from the editor database.
    /// <para/>
    /// If multiple resources of the same type exist, only the ones loaded on the first resource call will be returned.
    /// Please use <see cref="Reload"/> to force resources update.
    /// </summary>
    /// <typeparam name="T">Resource type (must inherit from <see cref="ScriptableObject"/>).</typeparam>
	public class AutoManagedResource<T> where T : ScriptableObject
    {
        #region Global Members
        private const string EditorFolder = "Editor";
        private const string AssetsFolder = "Assets";

        /// <summary>
        /// Default name for this type of resource assets.
        /// </summary>
        public readonly string DefaultAssetName = string.Empty;

        /// <summary>
        /// Prefix added to all of this type resource assets name.
        /// </summary>
        public readonly string Prefix = string.Empty;

        /// <summary>
        /// Suffix added to all of this type resource assets name.
        /// </summary>
        public readonly string Suffix = string.Empty;

        /// <summary>
        /// Indicates if this type of resource is for editor only or if it should be included in builds.
        /// <para/>
        /// Editor resources are automatically moved in an editor subfolder.
        /// </summary>
        public readonly bool IsEditorOnly = true;

        /// <summary>
        /// The directory where all of this type resource assets are created.
        /// </summary>
        public string Directory
        {
            get
            {
                // Get directory path.
                string _value = EnhancedEditorSettings.Settings.AutoManagedResourceDirectory;
                if (IsEditorOnly)
                {
                    _value = Path.Combine(_value, EditorFolder);
                }

                _value = Path.Combine(_value, typeof(T).ToString());
                return _value;
            }
        }

        private T[] resources = new T[] { };
        #endregion

        #region Constructors
        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(bool _isEditorOnly = true) : this(typeof(T).ToString(), _isEditorOnly) { }

        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(string _defaultAssetName, bool _isEditorOnly = true)
        {
            DefaultAssetName = _defaultAssetName;
            IsEditorOnly = _isEditorOnly;
        }

        /// <inheritdoc cref="AutoManagedResource{T}(string, string, string, bool)"/>
        public AutoManagedResource(string _prefix, string _suffix, bool _isEditorOnly = true) : this(_prefix, _suffix, typeof(T).ToString(), _isEditorOnly) { }

        /// <param name="_defaultAssetName"><inheritdoc cref="DefaultAssetName" path="/summary"/></param>
        /// <param name="_prefix"><inheritdoc cref="Prefix" path="/summary"/></param>
        /// <param name="_suffix"><inheritdoc cref="Suffix" path="/summary"/></param>
        /// <param name="_isEditorOnly"><inheritdoc cref="IsEditorOnly" path="/summary"/></param>
        /// <inheritdoc cref="AutoManagedResource{T}"/>
        public AutoManagedResource(string _defaultAssetName, string _prefix, string _suffix, bool _isEditorOnly = true) : this(_defaultAssetName, _isEditorOnly)
        {
            Prefix = _prefix;
            Suffix = _suffix;
        }
        #endregion

        #region Resources
        /// <summary>
        /// Get the first loaded resource of this type. If no matching asset could be found in the database, a new one is automatically created.
        /// </summary>
        /// <returns>First loaded resource asset.</returns>
        public T GetResource()
        {
            if ((resources.Length == 0) || (resources[0] == null))
                Reload();

            return resources[0];
        }

        /// <summary>
        /// Get a specific resource of this type by its name.
        /// </summary>
        /// <param name="_name">Name of the resource asset to find.</param>
        /// <param name="_resource">Found resource asset.</param>
        /// <returns>True if a resource asset with this same exist and was found, false otherwise.</returns>
        public bool GetResource(string _name, out T _resource)
        {
            if (resources.Length == 0)
                Reload();

            _name = _name.ToLower();

            bool _hasPrefix = !string.IsNullOrEmpty(Prefix);
            bool _hasSuffix = !string.IsNullOrEmpty(Suffix);

            _resource = Array.Find(resources, (r) =>
            {
                string _simplifiedName = r.name;

                if (_hasPrefix)
                    _simplifiedName = _simplifiedName.Replace(Prefix, string.Empty);

                if (_hasSuffix)
                    _simplifiedName = _simplifiedName.Replace(Suffix, string.Empty);

                return _simplifiedName.ToLower() == _name;
            });

            return _resource != null;
        }

        /// <summary>
        /// Get all loaded resources of this type. Use <see cref="Reload"/> to refresh all cached assets.
        /// </summary>
        /// <returns>All loaded resource assets.</returns>
        public T[] GetResources()
        {
            if (resources.Length == 0)
                Reload();

            return resources;
        }

        /// <summary>
        /// Reloads all cached resources.
        /// <br/>Use this after creating or destroying an asset of this type in the project.
        /// </summary>
        /// <returns>(Re)loaded resource assets.</returns>
        public T[] Reload()
        {
            resources = EnhancedEditorUtility.LoadAssets<T>();
            if (resources.Length == 0)
            {
                string _name = DefaultAssetName;
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
        /// Creates a new resource asset and save it in the database.
        /// </summary>
        /// <param name="_name">Name of the resource asset to create.</param>
        /// <param name="_resource">Resource instance to write on disk.</param>
        /// <returns>Created resource asset.</returns>
        public T CreateResource(string _name, T _resource)
        {
            _name = $"{Prefix}{_name}{Suffix}";

            string _directory = Path.Combine(Application.dataPath, Directory);
            if (!System.IO.Directory.Exists(_directory))
                System.IO.Directory.CreateDirectory(_directory);

            string _path = Path.Combine(AssetsFolder, Directory, $"{_name}.asset");
            _path = AssetDatabase.GenerateUniqueAssetPath(_path);

            AssetDatabase.CreateAsset(_resource, _path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ArrayUtility.Add(ref resources, _resource);
            return _resource;
        }
        #endregion
    }
}
