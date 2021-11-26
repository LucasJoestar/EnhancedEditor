// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
//      To avoid issues with serialization, circular references, parent folder
//  reference and multiple array iterations, all pinned objects are stored in
//  a single array. As so, a single class is used for both folders and assets,
//  as Unity serialization does not support inheritance in custom classes.
//
//      Another way of serializing this data would be to have two custom
//  classes: one for assets, and one for folders, the latter containing each
//  an array of folders and an array of assets. Then, to avoid the circular
//  reference serialization issue, either the folders should use the
//  SerializeReference attribute (which is unfortunately only available since
//  the Unity 2019.3 version), or the Pinboard class should implement the
//  ISerializationCallbackReceiver interface to receive callbacks on Unity
//  serialization, and write a custom serialize / deserialize system like the
//  one described here:
//
//      https://docs.unity3d.com/Manual/script-Serialization-Custom.html
//
// ============================================================================ //

using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// The pinboard stores your favorite assets and folders in the project. You can access them from the <see cref="PinboardWindow"/>.
    /// </summary>
    [NonEditable("Please use the Pinboard window if you want to edit pinned assets.")]
	public class Pinboard : ScriptableObject
    {
        #region Pin Object
        /// <summary>
        /// Data stored in the <see cref="Pinboard"/> used to whether reference an existing asset in the database,
        /// or acting as a folder to store other objects.
        /// </summary>
        [Serializable]
        public class PinObject
        {
            public const string FolderTypeName = "UnityEditor.DefaultAsset";

            // Folder.
            public string FolderName = string.Empty;
            public bool Foldout = true;

            // Asset.
            public string Type = string.Empty;
            public Object Asset = null;

            // Global.
            public int Indent = 0;
            public bool IsFolder = false;

            public bool IsVisible = true;
            public bool IsSelected = false;

            // -----------------------

            /// <summary>
            /// Folder constructor.
            /// </summary>
            public PinObject()
            {
                IsFolder = true;
                FolderName = "New Folder";
            }

            /// <summary>
            /// Asset reference constructor.
            /// </summary>
            /// <param name="_asset"><see cref="Object"/> to be pinned.
            /// <br/> Must be an asset in the database, not a scene instance.</param>
            public PinObject(Object _asset, int _indent)
            {
                Asset = _asset;
                Type = _asset.GetType().ToString();

                // Modify the folder type name so that they are always displayed on top.
                if (Type == FolderTypeName)
                {
                    Type = $"*{Type}";
                }

                Indent = _indent;
                IsFolder = false;
            }
        }
        #endregion

        #region Content
        /// <summary>
        /// All pinned objects in the pinboard.
        /// </summary>
        public PinObject[] PinObjects = new PinObject[] { };
        #endregion

        #region Utility
        [Button(SuperColor.Green, IsDrawnOnTop = false)]
        #pragma warning disable IDE0051
        private static void OpenPinboardWindow()
        {
            PinboardWindow.GetWindow();
        }
        #endregion
    }
}
