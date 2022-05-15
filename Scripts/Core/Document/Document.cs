// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;
using UnityEngine;

namespace EnhancedEditor
{
    /// <summary>
    /// <see cref="ScriptableObject"/> used for editor purposes, allowing to write and share
    /// nicely displayed documents across the project.
    /// </summary>
    [CreateAssetMenu(fileName = "DOC_NewDocument", menuName = "Enhanced Editor/Document", order = 185)]
	public class Document : ScriptableObject
    {
        #region Section
        [Serializable]
        public class Section
        {
            public string Name = string.Empty;
            public float Spacing = 0f;
            
            [Space(10f)]

            public string Header = string.Empty;
            [Enhanced, EnhancedTextArea] public string Text = string.Empty;
            [Enhanced, EnhancedTextArea] public string InfoText = string.Empty;
            public Texture2D Image = null;

            [Space(10f)]

            public string LinkText = string.Empty;
            public string URL = string.Empty;
        }
        #endregion

        #region Global Members
        [Section("Document Title")]

        public string Title = "Document";
        [Enhanced, Required] public Texture2D Icon = null;

        [Section("Sections")]

        public Section[] Sections = new Section[] { };
        #endregion
    }
}
