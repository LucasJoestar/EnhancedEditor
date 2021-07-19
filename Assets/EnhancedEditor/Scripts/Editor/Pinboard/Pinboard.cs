// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// The pinboard stores your favorite assets and folders in project. You can access them from the <see cref="PinboardWindow"/>.
    /// </summary>
    [NonEditable("Please use the PinboardWindow to edit pinned objects.")]
	public class Pinboard : ScriptableObject
    {
        #region Wrappers
        [Serializable]
        public abstract class PinObject { }

        [Serializable]
        public class Folder : PinObject
        {
            public string Name = "New Folder";
            public bool Foldout = true;
            public Asset[] Assets = new Asset[] { };
            [SerializeReference] public Folder[] Folders = new Folder[] { };
        }

        [Serializable]
        public class Asset : PinObject
        {
            public const string FolderTypeName = "UnityEditor.DefaultAsset";

            public string Type = string.Empty;
            public Object Object = null;

            public Asset(Object _object)
            {
                Object = _object;
                Type = _object.GetType().ToString();

                // Modify type name so that folders are always displayed on top.
                if (Type == FolderTypeName)
                {
                    Type = $"*{Type}";
                }
            }
        }
        #endregion

        #region Content
        /// <summary>
        /// All registered assets in pinboard, split in their respective folders.
        /// </summary>
        public Folder PinnedAssets = new Folder();
        #endregion
    }
}
