// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// <see cref="EnhancedEditor"/>-related <see cref="UnityEditor.AssetModificationProcessor"/>,
    /// especially used to update the last time a <see cref="GameObject"/> was modified in its <see cref="EnhancedBehaviour"/> component. 
    /// </summary>
    [InitializeOnLoad]
    public class AssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        #region Global Members
        private const string ModifiedObjectKey = "ModifiedObjects";

        [SerializeField] private static List<int> modifiedObjects = new List<int>();

        // -----------------------

        static AssetModificationProcessor()
        {
            Undo.postprocessModifications -= UndoPostProcess;
            Undo.postprocessModifications += UndoPostProcess;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        #endregion

        #region Asset Modifications
        internal static string saveAssetPath = string.Empty;

        // -----------------------

        #pragma warning disable IDE0051
        private static string[] OnWillSaveAssets(string[] _paths)
        {
            int[] _ids = GetModifiedObjects();
            if (_ids != null)
            {
                foreach (var _id in _ids)
                {
                    // Retrieve all modified objects and update their last modified state.
                    Object _object = EditorUtility.InstanceIDToObject(_id);

                    if (EditorUtility.IsDirty(_object)
                        && (((_object is Component _component) && _component.TryGetComponent(out EnhancedBehaviour _behaviour))
                           || ((_object is GameObject _gameObject) && _gameObject.TryGetComponent(out _behaviour))))
                    {
                        _behaviour.UpdateLastModifiedState();
                        EditorUtility.SetDirty(_behaviour);
                    }
                }

                modifiedObjects.Clear();
                SetModifiedObjects();
            }

            // When asked to only save one asset, discard the others.
            if (!string.IsNullOrEmpty(saveAssetPath)) {
                _paths = new string[] { saveAssetPath };
                saveAssetPath = string.Empty;
            }

            return _paths;
        }
        #endregion

        #region Modification Management
        private static UndoPropertyModification[] UndoPostProcess(UndoPropertyModification[] _propertyModifications)
        {
            foreach (UndoPropertyModification _property in _propertyModifications)
            {
                Object _object = _property.currentValue.target;
                if ((_object is Component) || (_object is GameObject))
                {
                    // Save modified object(s) id as one string to keep their value between compilations & play modes.
                    int _id = _object.GetInstanceID();
                    if (!modifiedObjects.Contains(_id))
                    {
                        modifiedObjects.Add(_id);
                        SetModifiedObjects();                        
                    }
                }
            }

            return _propertyModifications;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange _state)
        {
            if (_state == PlayModeStateChange.ExitingEditMode)
            {
                // Save dirty objects when exiting edit mode.
                modifiedObjects = new List<int>(GetModifiedObjects());
                foreach (var _id in modifiedObjects)
                {
                    if (!EditorUtility.IsDirty(_id))
                        modifiedObjects.Remove(_id);

                    SetModifiedObjects();
                }
            }
            else if (_state == PlayModeStateChange.EnteredEditMode)
            {
                // When re-entering edit mode, set registered objects as dirty
                // as they are no longer in this state after play mode.
                modifiedObjects = new List<int>(GetModifiedObjects());
                foreach (var _id in modifiedObjects)
                {
                    EditorUtility.SetDirty(EditorUtility.InstanceIDToObject(_id));
                }
            }
        }
        #endregion

        #region Utility
        private static readonly int[] defaultModifiedObjects = new int[0];

        // -----------------------

        private static void SetModifiedObjects()
        {
            SessionState.SetIntArray(ModifiedObjectKey, modifiedObjects.ToArray());
        }

        private static int[] GetModifiedObjects()
        {
            return SessionState.GetIntArray(ModifiedObjectKey, defaultModifiedObjects);
        }
        #endregion
    }
}
