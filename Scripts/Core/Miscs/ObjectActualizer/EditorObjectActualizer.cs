// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor {
    /// <summary>
    /// Editor utility <see cref="Component"/> automatically disabling and enabling back its <see cref="GameObject"/>.
    /// </summary>
    [ScriptGizmos(false, true), ExecuteInEditMode]
    [AddComponentMenu(InternalUtility.MenuPath + "Editor Object Actualizer"), DisallowMultipleComponent]
    public class EditorObjectActualizer : MonoBehaviour {
        #region Mono Behaviour
        #if UNITY_EDITOR
        private void Start() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                return;
            }

            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
        #endif
        #endregion
    }
}
