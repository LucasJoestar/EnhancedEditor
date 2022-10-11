// ===== Enhanced Editor - https://github.com/LucasJoestar/EnhancedEditor ===== //
// 
// Notes:
//
// ============================================================================ //

using System;

namespace EnhancedEditor {
    /// <summary>
    /// Attribute used to draw another field next to this one in the inspector.
    /// </summary>
	public class DuoAttribute : EnhancedPropertyAttribute {
        #region Global Members
        public const float DefaultFieldWidth = 50f;

        /// <summary>
        /// The name of the second field to draw next to this one.
        /// <br/> Be sure to assign the <see cref="NonSerializedAttribute"/> to it, to avoid drawing it twice.
        /// </summary>
        public readonly string FieldName = string.Empty;

        /// <summary>
        /// The width used to draw the second field in the inspector (in pixels).
        /// </summary>
        public readonly float FieldWidth = DefaultFieldWidth;

        // -----------------------

        /// <param name="_fieldName"><inheritdoc cref="FieldName" path="/summary"/></param>
        /// <param name="_fieldWidth"><inheritdoc cref="FieldWidth" path="/summary"/></param>
        /// <inheritdoc cref="DuoAttribute"/>
        public DuoAttribute(string _fieldName, float _fieldWidth = DefaultFieldWidth) {
            FieldName = _fieldName;
            FieldWidth = _fieldWidth;
        }
        #endregion
    }
}
