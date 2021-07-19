// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using UnityEditor;
using UnityEngine;

namespace EnhancedEditor.Editor
{
    /// <summary>
    /// Special drawer (inheriting from <see cref="UnityObjectDrawer"/>) for classes with attribute <see cref="PinnedObjectAttribute"/>.
    /// </summary>
    [CustomDrawer(typeof(PinnedObjectAttribute))]
    public class PinnedObjectDrawer : UnityObjectDrawer
    {
        #region Drawer Content
        public override void OnEnable()
        {
            string _folder = (Attribute as PinnedObjectAttribute).Path;
            MonoBehaviour _mono = (MonoBehaviour)SerializedObject.targetObject;

            PinAsset.CreatePinAsset(_folder, _mono);
        }
        #endregion
    }
}
